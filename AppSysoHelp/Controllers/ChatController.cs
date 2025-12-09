using AppSysoHelp.Models;
using AppSysoHelp.Service.SignalRService;
using AppSysoHelp.Service.WhatsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly HelpdesksysoContext _context;
        private readonly WhatsAppService _whatsAppService;
        private readonly SessionManager _sessionManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            HelpdesksysoContext context,
            WhatsAppService whatsAppService,
            SessionManager sessionManager,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatController> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _sessionManager = sessionManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/chat/{chamadoId}/messages
        /// Busca histórico de mensagens de um chamado
        /// </summary>
        [HttpGet("{chamadoId}/messages")]
        public async Task<IActionResult> GetMessages(long chamadoId)
        {
            try
            {
                var chamado = await _context.Chamados
                    .FirstOrDefaultAsync(c => c.ChamadoId == chamadoId);

                if (chamado == null)
                {
                    return NotFound(new { error = "Chamado não encontrado" });
                }

                if (string.IsNullOrEmpty(chamado.TelefoneContato))
                {
                    return Ok(new { messages = new List<object>() });
                }

                // Buscar mensagens do telefone
                var messages = await _sessionManager.GetMessageHistoryAsync(
                    phoneNumber: chamado.TelefoneContato,
                    limit: 100
                );

                // Mapear para formato JSON
                var result = messages.Select(m => new
                {
                    id = m.Id,
                    direction = m.Direction,
                    content = m.MessageContent,
                    sentBy = m.SentBy,
                    timestamp = m.Timestamp,
                    messageType = m.MessageType
                }).ToList();

                return Ok(new { messages = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagens do chamado {ChamadoId}", chamadoId);
                return StatusCode(500, new { error = "Erro ao buscar mensagens" });
            }
        }

        /// <summary>
        /// POST: api/chat/send
        /// Envia mensagem do técnico para cliente via WhatsApp
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Mensagem não pode ser vazia" });
                }

                // Buscar chamado
                var chamado = await _context.Chamados
                    .FirstOrDefaultAsync(c => c.ChamadoId == request.ChamadoId);

                if (chamado == null)
                {
                    return NotFound(new { error = "Chamado não encontrado" });
                }

                if (string.IsNullOrEmpty(chamado.TelefoneContato))
                {
                    return BadRequest(new { error = "Chamado não possui telefone de contato" });
                }

                // Enviar mensagem via WhatsApp
                var sent = await _whatsAppService.SendTextMessageAsync(
                    to: chamado.TelefoneContato,
                    message: request.Message
                );

                if (!sent)
                {
                    return StatusCode(500, new { error = "Falha ao enviar mensagem via WhatsApp" });
                }

                // Salvar mensagem no histórico
                await _sessionManager.SaveMessageAsync(
                    phoneNumber: chamado.TelefoneContato,
                    direction: "outgoing",
                    messageType: "text",
                    content: request.Message,
                    sentBy: "agent"
                );

                // Notificar via SignalR (atualizar UI de outros técnicos conectados)
                await _hubContext.Clients
                    .Group($"chamado_{request.ChamadoId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        direction = "outgoing",
                        content = request.Message,
                        sentBy = "agent",
                        timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("✅ Mensagem enviada - Chamado #{ChamadoId}, Técnico → Cliente",
                    request.ChamadoId);

                return Ok(new { success = true, message = "Mensagem enviada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem para chamado {ChamadoId}", request.ChamadoId);
                return StatusCode(500, new { error = "Erro ao enviar mensagem" });
            }
        }
    }

    // Modelo para receber requisição de envio
    public class SendMessageRequest
    {
        public long ChamadoId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
