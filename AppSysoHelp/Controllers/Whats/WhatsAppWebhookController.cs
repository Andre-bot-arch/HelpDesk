using AppSysoHelp.Models;
using AppSysoHelp.Models.WhatApp;
using AppSysoHelp.Service.SignalRService;
using AppSysoHelp.Service.WhatsService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers.Whats
{
    [Route("api/webhook")]
    [ApiController]
    public sealed class WhatsAppWebhookController : ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;  // ✅ Mudança aqui
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppWebhookController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public WhatsAppWebhookController(
            IHubContext<ChatHub> hubContext,
            IServiceScopeFactory serviceScopeFactory,  // ✅ Mudança aqui
            IConfiguration configuration,
            ILogger<WhatsAppWebhookController> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;  // ✅ Mudança aqui
            _configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// GET: Verificação do Webhook pela Meta
        /// A Meta chama este endpoint para validar que o webhook está configurado corretamente
        /// </summary>
        [HttpGet]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            _logger.LogInformation("Recebida requisição de verificação do webhook");
            _logger.LogDebug("Mode: {Mode}, Token: {Token}, Challenge: {Challenge}", mode, token, challenge);

            var verifyToken = _configuration["WhatsApp:VerifyToken"];

            // Verificar se o token enviado pela Meta é o mesmo que configuramos
            if (mode == "subscribe" && token == verifyToken)
            {
                _logger.LogInformation("Webhook verificado com sucesso!");
                // Retornar o challenge para confirmar a verificação
                return Ok(challenge);
            }
            else
            {
                _logger.LogWarning("Falha na verificação do webhook. Token inválido.");
                return Unauthorized("Token de verificação inválido");
            }
        }

        /// <summary>
        /// POST: Receber mensagens e eventos do WhatsApp
        /// A Meta envia mensagens recebidas, status de entrega, etc.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] WhatsAppWebhookRequest webhookData)
        {
            try
            {
                _logger.LogInformation("Webhook recebido da Meta");

                // Validar se é uma mensagem do WhatsApp Business
                if (webhookData?.Object != "whatsapp_business_account")
                {
                    _logger.LogWarning("Webhook recebido não é do WhatsApp Business Account");
                    return BadRequest("Tipo de objeto inválido");
                }

                // Processar cada entrada (entry) do webhook
                foreach (var entry in webhookData.Entry ?? new List<WhatsAppEntry>())
                {
                    foreach (var change in entry.Changes ?? new List<WhatsAppChange>())
                    {
                        var value = change.Value;

                        // Processar mensagens recebidas
                        if (value?.Messages != null && value.Messages.Any())
                        {
                            foreach (var message in value.Messages)
                            {
                                _logger.LogInformation("Mensagem recebida de: {From}, Tipo: {Type}, ID: {Id}",
                                    message.From, message.Type, message.Id);

                                // Pegar o nome do contato (se disponível)
                                var senderName = value.Contacts?.FirstOrDefault()?.Profile?.Name ?? "Cliente";

                                // ✅ CRIAR NOVO SCOPE PARA CADA MENSAGEM
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        using (var scope = _serviceScopeFactory.CreateScope())
                                        {
                                            var messageProcessor = scope.ServiceProvider.GetRequiredService<MessageProcessorService>();
                                            await messageProcessor.ProcessMessageAsync(message, senderName);

                                            // 🆕 ADICIONAR AQUI - Notificar SignalR
                                            await NotifySignalRNewMessage(scope, message, senderName);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", message.Id);
                                    }
                                });
                            }
                        }

                        // Processar status de mensagens (entregue, lido, etc) - OPCIONAL
                        if (value?.Statuses != null && value.Statuses.Any())
                        {
                            foreach (var status in value.Statuses)
                            {
                                _logger.LogInformation("Status recebido: ID: {Id}, Status: {Status}",
                                    status.Id, status.Status);
                                // Aqui você pode salvar no banco se quiser rastrear status
                            }
                        }
                    }
                }

                // IMPORTANTE: Sempre retornar 200 OK rapidamente
                // A Meta espera resposta em até 20 segundos
                return Ok(new { success = true, message = "Webhook processado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook");
                // Mesmo com erro, retornar 200 para não reenviar
                return Ok(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de teste para verificar se a API está funcionando
        /// Acesse: /api/webhook/test
        /// </summary>a
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                status = "online",
                message = "Webhook do WhatsApp está funcionando!",
                timestamp = DateTime.UtcNow
            });
        }


        /// <summary>
        /// Notifica técnicos conectados via SignalR quando mensagem chega do WhatsApp
        /// </summary>
        //private async Task NotifySignalRNewMessage(IServiceScope scope, WhatsAppMessage message, string senderName)
        //{
        //    try
        //    {
        //        var context = scope.ServiceProvider.GetRequiredService<HelpdesksysoContext>();
        //        var sessionManager = scope.ServiceProvider.GetRequiredService<SessionManager>();

        //        // Corrigir número brasileiro (adicionar 9 se necessário)
        //        var phoneNumber = message.From;
        //        if (phoneNumber.StartsWith("55") && phoneNumber.Length == 12)
        //        {
        //            var ddd = phoneNumber.Substring(2, 2);
        //            var numero = phoneNumber.Substring(4);
        //            phoneNumber = $"55{ddd}9{numero}";
        //        }

        //        // Buscar sessão para pegar chamado vinculado
        //        var session = await sessionManager.GetOrCreateSessionAsync(phoneNumber);

        //        if (session.LinkedTicketId.HasValue)
        //        {
        //            var chamadoId = session.LinkedTicketId.Value;

        //            // Extrair conteúdo da mensagem
        //            var messageContent = message.Text?.Body
        //                ?? message.Interactive?.ButtonReply?.Title
        //                ?? message.Interactive?.ListReply?.Title
        //                ?? "[Mensagem não suportada]";

        //            // Enviar para grupo do chamado via SignalR
        //            await _hubContext.Clients
        //                .Group($"chamado_{chamadoId}")
        //                .SendAsync("ReceiveMessage", new
        //                {
        //                    direction = "incoming",
        //                    content = messageContent,
        //                    sentBy = "customer",
        //                    senderName = senderName,
        //                    timestamp = DateTime.UtcNow,
        //                    messageType = message.Type
        //                });

        //            _logger.LogInformation("📡 SignalR notificado - Chamado #{ChamadoId}, Cliente → Técnico:  {Message}",
        //                chamadoId, messageContent.Substring(0, Math.Min(50, messageContent.Length)));
        //        }
        //        else
        //        {
        //            _logger.LogDebug("Mensagem de {Phone} não tem chamado vinculado.  SignalR não notificado.", phoneNumber);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao notificar SignalR para mensagem {MessageId}", message.Id);
        //    }
        //}

        /// <summary>
        /// Notifica técnicos conectados via SignalR quando mensagem chega do WhatsApp
        /// </summary>
        private async Task NotifySignalRNewMessage(IServiceScope scope, WhatsAppMessage message, string senderName)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<HelpdesksysoContext>();
                var sessionManager = scope.ServiceProvider.GetRequiredService<SessionManager>();

                // Corrigir número brasileiro
                var phoneNumber = message.From;
                if (phoneNumber.StartsWith("55") && phoneNumber.Length == 12)
                {
                    var ddd = phoneNumber.Substring(2, 2);
                    var numero = phoneNumber.Substring(4);
                    phoneNumber = $"55{ddd}9{numero}";
                }

                // Buscar sessão
                var session = await sessionManager.GetOrCreateSessionAsync(phoneNumber);

                if (session.LinkedTicketId.HasValue)
                {
                    var chamadoId = session.LinkedTicketId.Value;

                    // ✅ BUSCAR A ÚLTIMA MENSAGEM SALVA PARA PEGAR A MEDIA URL
                    var ultimaMensagem = await context.MessageHistories
                        .Where(m => m.PhoneNumber == phoneNumber)
                        .Where(m => m.WhatsAppMessageId == message.Id)
                        .Take(10)
                        .OrderByDescending(m => m.Timestamp)
                        .FirstOrDefaultAsync();

                    var messageContent = ultimaMensagem?.MessageContent ?? "[Mensagem]";
                    var mediaUrl = ultimaMensagem?.MediaUrl;

                    // Enviar para SignalR
                    await _hubContext.Clients
                        .Group($"chamado_{chamadoId}")
                        .SendAsync("ReceiveMessage", new
                        {
                            direction = "incoming",
                            content = messageContent,
                            sentBy = "customer",
                            senderName = senderName,
                            timestamp = DateTime.UtcNow,
                            messageType = message.Type,
                            mediaUrl = mediaUrl  // ✅ URL DA MÍDIA
                        });

                    _logger.LogInformation("📡 SignalR notificado - Chamado #{ChamadoId}, Tipo: {Type}, Media: {HasMedia}",
                        chamadoId, message.Type, mediaUrl != null ? "Sim" : "Não");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar SignalR");
            }
        }

        /// <summary>
        /// Extrai o conteúdo da mensagem baseado no tipo
        /// </summary>
        private (string content, string? mediaId, string? mediaUrl) ExtractMessageContent(WhatsAppMessage message)
        {
            return message.Type?.ToLower() switch
            {
                "text" => (
                    message.Text?.Body ?? "[Texto vazio]",
                    null,
                    null
                ),

                "interactive" => (
                    message.Interactive?.ButtonReply?.Title
                    ?? message.Interactive?.ListReply?.Title
                    ?? "[Resposta interativa]",
                    null,
                    null
                ),

                "image" => (
                    message.Image?.Caption ?? "📷 Imagem",
                    message.Image?.Id,
                    null // Você pode gerar URL depois do download
                ),

                "audio" => (
                    "🎵 Áudio",
                    message.Audio?.Id,
                    null
                ),

                "voice" => (
                    "🎤 Mensagem de voz",
                    message.Voice?.Id,
                    null
                ),

                "video" => (
                    message.Video?.Caption ?? "🎥 Vídeo",
                    message.Video?.Id,
                    null
                ),

                "document" => (
                    $"📄 {message.Document?.Filename ?? "Documento"}",
                    message.Document?.Id,
                    null
                ),

                "sticker" => (
                    "😊 Figurinha",
                    message.Sticker?.Id,
                    null
                ),

                "location" => (
                    $"📍 Localização:  {message.Location?.Name ?? "Sem nome"}",
                    null,
                    null
                ),

                "contacts" => (
                    $"👤 Contato: {message.Contacts?.FirstOrDefault()?.Name?.FormattedName ?? "Desconhecido"}",
                    null,
                    null
                ),

                _ => (
                    $"[Tipo não suportado: {message.Type}]",
                    null,
                    null
                )
            };
        }
    }

}
