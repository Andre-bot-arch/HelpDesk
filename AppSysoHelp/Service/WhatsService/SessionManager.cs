using AppSysoHelp.Models;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Service.WhatsService
{
    public class SessionManager
    {
        private readonly HelpdesksysoContext _context;
        private readonly ILogger<SessionManager> _logger;

        public SessionManager(HelpdesksysoContext context, ILogger<SessionManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Obter ou criar sessão
        public async Task<CustomerSessions> GetOrCreateSessionAsync(string phoneNumber)
        {
            var session = await _context.CustomerSessions
                .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);

            if (session == null)
            {
                session = new CustomerSessions
                {
                    PhoneNumber = phoneNumber,
                    State = 0, // BotActive
                    CreatedAt = DateTime.UtcNow,
                    LastInteraction = DateTime.UtcNow
                };

                _context.CustomerSessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nova sessão criada para {Phone}", phoneNumber);
            }

            return session;
        }

        // Atualizar estado da sessão
        public async Task UpdateStateAsync(string phoneNumber, int newState, string? agentName = null)
        {
            var session = await GetOrCreateSessionAsync(phoneNumber);
            var oldState = session.State;

            session.State = newState;
            session.LastInteraction = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(agentName))
            {
                session.AssignedAgent = agentName;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sessão {Phone}: Estado {OldState} → {NewState} (Agente: {Agent})",
                phoneNumber, oldState, newState, agentName ?? "N/A");
        }

        /// <summary>
        /// Atualiza sessão completa (sem mudar State)
        /// </summary>
        public async Task UpdateSessionAsync(CustomerSessions session)
        {
            session.LastInteraction = DateTime.UtcNow;
            _context.CustomerSessions.Update(session);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Sessão {Phone} atualizada.  Flow: {Flow}",
                session.PhoneNumber, session.CurrentFlow ?? "nenhum");
        }

        // Verificar se bot deve responder
        public async Task<bool> ShouldBotRespondAsync(string phoneNumber)
        {
            var session = await GetOrCreateSessionAsync(phoneNumber);
            return session.State == 0; // BotActive
        }

        // Verificar se está aguardando atendente
        public async Task<bool> IsWaitingForAgentAsync(string phoneNumber)
        {
            var session = await GetOrCreateSessionAsync(phoneNumber);
            return session.State == 1; // WaitingForAgent
        }

        // Verificar se atendente está ativo
        public async Task<bool> IsAgentActiveAsync(string phoneNumber)
        {
            var session = await GetOrCreateSessionAsync(phoneNumber);
            return session.State == 2; // AgentActive
        }

        // Finalizar atendimento (volta para bot)
        public async Task CloseSessionAsync(string phoneNumber)
        {
            var session = await GetOrCreateSessionAsync(phoneNumber);
            session.State = 0; // BotActive
            session.AssignedAgent = null;
            session.LastInteraction = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sessão {Phone} finalizada.  Bot reativado.", phoneNumber);
        }

        // Obter todas as sessões aguardando atendente
        public async Task<List<CustomerSessions>> GetPendingSessionsAsync()
        {
            return await _context.CustomerSessions
                .Where(s => s.State == 1) // WaitingForAgent
                .OrderBy(s => s.LastInteraction)
                .ToListAsync();
        }

        // Salvar mensagem no histórico
        public async Task SaveMessageAsync(string phoneNumber, string direction, string messageType,
            string? content, string? sentBy = null, string? whatsappMessageId = null)
        {
            var message = new MessageHistories
            {
                PhoneNumber = phoneNumber,
                Direction = direction,
                MessageType = messageType,
                MessageContent = content,
                SentBy = sentBy ?? (direction == "incoming" ? "customer" : "bot"),
                WhatsAppMessageId = whatsappMessageId,
                Timestamp = DateTime.UtcNow
            };

            _context.MessageHistories.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Mensagem salva: {Phone} - {Direction} - {Type}",
                phoneNumber, direction, messageType);
        }

        // Obter histórico de mensagens
        public async Task<List<MessageHistories>> GetMessageHistoryAsync(string phoneNumber, int limit = 50)
        {
            return await _context.MessageHistories
                .Where(m => m.PhoneNumber == phoneNumber)
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Marca sessão como em atendimento humano (bot para de responder)
        /// </summary>
        public async Task SetHumanAttendanceAsync(string phoneNumber, bool isHumanAttendance)
        {
            try
            {
                var session = await GetOrCreateSessionAsync(phoneNumber);
                session.IsHumanAttendance = isHumanAttendance;

                if (isHumanAttendance)
                {
                    // Limpar fluxo do bot
                    session.CurrentFlow = null;
                    session.FlowData = null;
                }

                await UpdateSessionAsync(session);

                _logger.LogInformation("✅ Sessão {Phone} marcada como atendimento humano: {IsHuman}",
                    phoneNumber, isHumanAttendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar sessão {Phone} como atendimento humano", phoneNumber);
            }
        }
    }
}
