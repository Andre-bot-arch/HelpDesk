using AppSysoHelp.Service.WhatsService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers.Whats
{
    [Route("api/agent")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly SessionManager _sessionManager;
        private readonly WhatsAppService _whatsAppService;
        private readonly ILogger<AgentController> _logger;

        public AgentController(
            SessionManager sessionManager,
            WhatsAppService whatsAppService,
            ILogger<AgentController> logger)
        {
            _sessionManager = sessionManager;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        // GET: api/agent/pending - Listar clientes aguardando
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingCustomers()
        {
            var pending = await _sessionManager.GetPendingSessionsAsync();

            return Ok(new
            {
                count = pending.Count,
                customers = pending.Select(s => new
                {
                    phone = s.PhoneNumber,
                    waitingTime = DateTime.UtcNow - s.LastInteraction,
                    createdAt = s.CreatedAt,
                    state = s.State
                })
            });
        }

        // POST: api/agent/take/5569993849824? agentName=João - Atendente assume cliente
        [HttpPost("take/{phoneNumber}")]
        public async Task<IActionResult> TakeCustomer(string phoneNumber, [FromQuery] string agentName)
        {
            await _sessionManager.UpdateStateAsync(phoneNumber, 2, agentName); // 2 = AgentActive

            await _whatsAppService.SendTextMessageAsync(phoneNumber,
                $"✅ *{agentName}* entrou no atendimento!\n\n" +
                "Olá! Como posso ajudar você?   😊");

            await _sessionManager.SaveMessageAsync(phoneNumber, "outgoing", "text",
                $"{agentName} entrou no atendimento", $"agent:{agentName}");

            _logger.LogInformation("Agente {Agent} assumiu cliente {Phone}", agentName, phoneNumber);

            return Ok(new { message = "Cliente assumido com sucesso", agent = agentName, phone = phoneNumber });
        }

        // POST: api/agent/close/5569993849824 - Finalizar atendimento
        [HttpPost("close/{phoneNumber}")]
        public async Task<IActionResult> CloseCustomer(string phoneNumber)
        {
            await _sessionManager.CloseSessionAsync(phoneNumber);

            await _whatsAppService.SendTextMessageAsync(phoneNumber,
                "✅ *Atendimento Finalizado*\n\n" +
                "Obrigado por entrar em contato!\n\n" +
                "Se precisar de algo, envie uma mensagem que o bot irá te ajudar!   🤖");

            await _sessionManager.SaveMessageAsync(phoneNumber, "outgoing", "text",
                "Atendimento finalizado", "bot");

            _logger.LogInformation("Atendimento finalizado para {Phone}", phoneNumber);

            return Ok(new { message = "Atendimento finalizado", phone = phoneNumber });
        }

        // POST: api/agent/send - Atendente envia mensagem manual
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] AgentMessageRequest request)
        {
            var session = await _sessionManager.GetOrCreateSessionAsync(request.To);

            if (session.State != 2) // AgentActive
            {
                return BadRequest(new { error = "Cliente não está em atendimento ativo" });
            }

            await _whatsAppService.SendTextMessageAsync(request.To, request.Message);

            await _sessionManager.SaveMessageAsync(request.To, "outgoing", "text",
                request.Message, $"agent:{session.AssignedAgent}");

            return Ok(new { message = "Mensagem enviada", to = request.To });
        }

        // GET: api/agent/history/5569993849824 - Obter histórico
        [HttpGet("history/{phoneNumber}")]
        public async Task<IActionResult> GetHistory(string phoneNumber, [FromQuery] int limit = 50)
        {
            var history = await _sessionManager.GetMessageHistoryAsync(phoneNumber, limit);

            return Ok(new
            {
                phone = phoneNumber,
                messageCount = history.Count,
                messages = history.Select(m => new
                {
                    direction = m.Direction,
                    type = m.MessageType,
                    content = m.MessageContent,
                    sentBy = m.SentBy,
                    timestamp = m.Timestamp
                })
            });
        }
    }

    // Modelo para enviar mensagem manual
    public class AgentMessageRequest
    {
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
