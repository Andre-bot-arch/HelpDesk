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
                    limit: 30
                );

                // Mapear para formato JSON
                var result = messages.Select(m => new
                {
                    id = m.Id,
                    direction = m.Direction,
                    content = m.MessageContent,
                    sentBy = m.SentBy,
                    timestamp = m.Timestamp,
                    messageType = m.MessageType,
                    mediaUrl = m.MediaUrl  
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
    

    /// <summary>
        /// GET: api/chat/conversations
        /// Lista todas as conversas ativas do WhatsApp
        /// </summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                // Buscar todas as conversas com a última mensagem e informações do chamado
                var conversations = await _context.MessageHistories
                    .GroupBy(m => m.PhoneNumber)
                    .Select(g => new
                    {
                        phoneNumber = g.Key,
                        lastMessage = g.OrderByDescending(m => m.Timestamp).First().MessageContent,
                        lastMessageTime = g.OrderByDescending(m => m.Timestamp).First().Timestamp,
                        lastMessageType = g.OrderByDescending(m => m.Timestamp).First().MessageType,
                        chamadoId = g.OrderByDescending(m => m.Timestamp).First().ChamadoId,

                        // Buscar nome do cliente do chamado relacionado
                        customerName = _context.Chamados
                            .Where(c => c.TelefoneContato == g.Key)
                            .OrderByDescending(c => c.DataCriacao)
                            .Select(c => c.Contato)
                            .FirstOrDefault() ?? g.Key,

                        // Contar mensagens não lidas (incoming do customer)
                        unreadCount = g.Count(m => m.Direction == "incoming" && m.SentBy == "customer")
                    })
                    .OrderByDescending(c => c.lastMessageTime)
                    .ToListAsync();

                _logger.LogInformation("📋 Listadas {Count} conversas", conversations.Count);

                return Ok(new { conversations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar conversas");
                return StatusCode(500, new { error = "Erro ao buscar conversas" });
            }
        }

        /// <summary>
        /// GET: api/chat/phone/{phoneNumber}/messages
        /// Busca mensagens por número de telefone (para o chat multi-conversas)
        /// </summary>
        [HttpGet("phone/{phoneNumber}/messages")]
        public async Task<IActionResult> GetMessagesByPhone(string phoneNumber, [FromQuery] int limit = 50)
        {
            try
            {
                var messages = await _context.MessageHistories
                    .Where(m => m.PhoneNumber == phoneNumber)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(limit)
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new
                    {
                        id = m.Id,
                        content = m.MessageContent,
                        direction = m.Direction,
                        messageType = m.MessageType,
                        sentBy = m.SentBy,
                        timestamp = m.Timestamp,
                        mediaUrl = m.MediaUrl
                    })
                    .ToListAsync();

                _logger.LogInformation("📥 Carregadas {Count} mensagens do telefone {Phone}",
                    messages.Count, phoneNumber);

                return Ok(new { messages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagens do telefone {Phone}", phoneNumber);
                return StatusCode(500, new { error = "Erro ao buscar mensagens" });
            }
        }

        /// <summary>
        /// POST: api/chat/send-to-phone
        /// Envia mensagem para um telefone específico (para o chat multi-conversas)
        /// </summary>
        [HttpPost("send-to-phone")]
        public async Task<IActionResult> SendMessageToPhone([FromBody] SendMessageToPhoneRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Mensagem não pode ser vazia" });
                }

                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return BadRequest(new { error = "Telefone não pode ser vazio" });
                }

                // Enviar mensagem via WhatsApp
                var sent = await _whatsAppService.SendTextMessageAsync(
                    to: request.PhoneNumber,
                    message: request.Message
                );

                if (!sent)
                {
                    return StatusCode(500, new { error = "Falha ao enviar mensagem via WhatsApp" });
                }

                // Salvar mensagem no histórico
                await _sessionManager.SaveMessageAsync(
                    phoneNumber: request.PhoneNumber,
                    direction: "outgoing",
                    messageType: "text",
                    content: request.Message,
                    sentBy: "agent"
                );

                // Buscar se há chamado associado para notificar via SignalR
                var chamado = await _context.Chamados
                    .Where(c => c.TelefoneContato == request.PhoneNumber)
                    .OrderByDescending(c => c.DataCriacao)
                    .FirstOrDefaultAsync();

                if (chamado != null)
                {
                    await _hubContext.Clients
                        .Group($"chamado_{chamado.ChamadoId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            direction = "outgoing",
                            content = request.Message,
                            sentBy = "agent",
                            timestamp = DateTime.UtcNow,
                            messageType = "text"
                        });
                }

                // Notificar grupo do telefone (para chat multi-conversas)
                await _hubContext.Clients
                    .Group($"phone_{request.PhoneNumber}")
                    .SendAsync("ReceiveMessage", new
                    {
                        phoneNumber = request.PhoneNumber,
                        direction = "outgoing",
                        content = request.Message,
                        sentBy = "agent",
                        timestamp = DateTime.UtcNow,
                        messageType = "text"
                    });

                _logger.LogInformation("✅ Mensagem enviada para {Phone}", request.PhoneNumber);

                return Ok(new { success = true, message = "Mensagem enviada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem para {Phone}", request.PhoneNumber);
                return StatusCode(500, new { error = "Erro ao enviar mensagem" });
            }
        }

        /// <summary>
        /// GET: api/chat/conversation/{phoneNumber}/info
        /// Busca informações de uma conversa específica
        /// </summary>
        [HttpGet("conversation/{phoneNumber}/info")]
        public async Task<IActionResult> GetConversationInfo(string phoneNumber)
        {
            try
            {
                // Buscar chamado relacionado
                var chamado = await _context.Chamados
                    .Where(c => c.TelefoneContato == phoneNumber)
                    .OrderByDescending(c => c.DataCriacao)
                    .FirstOrDefaultAsync();

                // Buscar sessão
                var session = await _sessionManager.GetOrCreateSessionAsync(phoneNumber);

                // Contar mensagens
                var messageCount = await _context.MessageHistories
                    .Where(m => m.PhoneNumber == phoneNumber)
                    .CountAsync();

                var info = new
                {
                    phoneNumber,
                    customerName = chamado?.Contato ?? phoneNumber,
                    chamadoId = chamado?.ChamadoId,
                    sessionState = session.State,
                    currentFlow = session.CurrentFlow,
                    messageCount,
                    linkedTicketId = session.LinkedTicketId
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar info da conversa {Phone}", phoneNumber);
                return StatusCode(500, new { error = "Erro ao buscar informações" });
            }
        }
    }


    // Modelo para receber requisição de envio
    public class SendMessageRequest
    {
        public long ChamadoId { get; set; }
        public string Message { get; set; } = string.Empty;
    }


    public class SendMessageToPhoneRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
