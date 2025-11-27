using AppSysoHelp.Models.WhatApp;
using AppSysoHelp.Service.WhatsService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers.Whats
{
    [Route("api/webhook")]
    [ApiController]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;  // ✅ Mudança aqui
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppWebhookController> _logger;

        public WhatsAppWebhookController(
            IServiceScopeFactory serviceScopeFactory,  // ✅ Mudança aqui
            IConfiguration configuration,
            ILogger<WhatsAppWebhookController> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;  // ✅ Mudança aqui
            _configuration = configuration;
            _logger = logger;
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
                                            var messageProcessor = scope.ServiceProvider
                                                .GetRequiredService<MessageProcessorService>();

                                            await messageProcessor.ProcessMessageAsync(message, senderName);
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
        /// </summary>
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
    }
}
