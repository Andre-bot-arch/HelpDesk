using AppSysoHelp.Models.WhatApp;

namespace AppSysoHelp.Service.WhatsService
{
    public class MessageProcessorService(
        WhatsAppService whatsAppService,
        ILogger<MessageProcessorService> logger,
        SessionManager sessionManager)
    {
        private readonly WhatsAppService _whatsAppService = whatsAppService;
        private readonly ILogger<MessageProcessorService> _logger = logger;
        private readonly SessionManager _sessionManager = sessionManager;

        // Método principal para processar mensagem recebida
        public async Task ProcessMessageAsync(WhatsAppMessage message, string senderName)
        {
            try
            {
                var from = message.From;
                var messageType = message.Type;
                if (from.StartsWith("55") && from.Length == 12)
                {
                    // Número brasileiro com código do país (55), DDD (2 dígitos) e 8 dígitos
                    // Formato atual: 55DDNNNNNNNN (12 caracteres)
                    // Formato correto: 55DD9NNNNNNNN (13 caracteres)

                    var ddd = from.Substring(2, 2); // Extrai o DDD
                    var numero = from.Substring(4); // Extrai o número

                    // Celulares brasileiros devem ter 9 dígitos (começando com 9)
                    if (numero.Length == 8)
                    {
                        from = $"55{ddd}9{numero}";

                        _logger.LogWarning("Número corrigido de {Original} para {Corrected}",
                            message.From, from);
                    }
                }

                // Obter sessão
                var session = await _sessionManager.GetOrCreateSessionAsync(from);

                _logger.LogInformation("Mensagem de {From} ({Name}) - Estado: {State}",
                    from, senderName, session.State);

                // Se atendente está ativo, NÃO processar automaticamente
                if (session.State == 2) // AgentActive
                {
                    _logger.LogInformation("⚠️ Atendente ativo para {From}. Bot não irá responder.", from);

                    // Salvar mensagem recebida no histórico
                    await _sessionManager.SaveMessageAsync(from, "incoming", message.Type ?? "unknown",
                        message.Text?.Body ?? message.Interactive?.ButtonReply?.Id, "customer", message.Id);

                    return;
                }

                // Se está aguardando atendente, apenas confirmar
                if (session.State == 1) // WaitingForAgent
                {
                    _logger.LogInformation("⏳ Cliente {From} aguardando atendente", from);

                    await _whatsAppService.SendTextMessageAsync(from,
                        "Você já está na fila de atendimento. ⏳\n\nEm breve um atendente irá responder!");

                    return;
                }

                // Bot ativo - continua processando normalmente
                _logger.LogInformation("🤖 Bot ativo para {From}. Processando mensagem...", from);

                _logger.LogInformation("Processando mensagem de {From} ({Name}), Tipo: {Type}",
                    from, senderName, messageType);

                // Processar mensagem de texto
                if (messageType == "text" && message.Text != null)
                {
                    var userMessage = message.Text.Body.Trim().ToLower();
                    await ProcessTextMessageAsync(from, userMessage, senderName);
                }
                // Processar resposta de botão
                else if (messageType == "interactive" && message.Interactive != null)
                {
                    if (message.Interactive.ButtonReply != null)
                    {
                        var buttonId = message.Interactive.ButtonReply.Id;
                        await ProcessButtonResponseAsync(from, buttonId);
                    }
                    else if (message.Interactive.ListReply != null)
                    {
                        var listId = message.Interactive.ListReply.Id;
                        await ProcessListResponseAsync(from, listId);
                    }
                }
                else
                {
                    _logger.LogWarning("Tipo de mensagem não suportado: {Type}", messageType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem");
            }
        }

        // Processar mensagem de texto do usuário
        private async Task ProcessTextMessageAsync(string from, string message, string senderName)
        {
            // Saudações iniciais
            if (message.Contains("oi") || message.Contains("olá") || message.Contains("ola") ||
                message.Contains("bom dia") || message.Contains("boa tarde") || message.Contains("boa noite"))
            {
                await SendMainMenuAsync(from, senderName);
            }
            // Menu ou ajuda
            else if (message.Contains("menu") || message.Contains("ajuda") || message.Contains("opções") || message.Contains("opcoes"))
            {
                await SendMainMenuAsync(from, senderName);
            }
            // Mensagem não reconhecida
            else
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    $"Desculpe, não entendi sua mensagem. 🤔\n\nDigite *menu* para ver as opções disponíveis.");
            }
        }

        // Enviar menu principal
        private async Task SendMainMenuAsync(string to, string senderName)
        {
            var buttons = new List<(string id, string title)>
            {
               ("btn_financeiro", "💰 Financeiro"),
               ("btn_suporte", "💬 Suporte"),
               ("btn_info", "ℹ️ Informações")
            };

            await _whatsAppService.SendButtonMessageAsync(
                to: to,
                bodyText: $"Olá *{senderName}*! 👋\n\nBem-vindo ao *ZapSyso*!\n\nComo posso ajudar você hoje?",
                buttons: buttons,
                headerText: "Menu Principal",
                footerText: "Selecione uma opção abaixo"
            );
        }

        // Processar resposta de botão
        private async Task ProcessButtonResponseAsync(string from, string buttonId)
        {
            _logger.LogInformation("Botão clicado: {ButtonId}", buttonId);

            switch (buttonId)
            {
                case "btn_financeiro":
                    await SendFinanceiroMenuAsync(from);
                    break;

                case "btn_suporte":
                    await _sessionManager.UpdateStateAsync(from, 1); // 1 = WaitingForAgent

                    await SendSupportOptionsAsync(from);

                    await _sessionManager.SaveMessageAsync(from, "outgoing", "text", "Cliente solicitou atendimento humano", "bot");

                    _logger.LogWarning("🔔 Cliente {From} aguardando atendente!", from);
                    break;

                case "btn_info":
                    await SendInfoAsync(from);
                    break;

                case "btn_voltar":
                    await SendMainMenuAsync(from, "");
                    break;

                default:
                    await _whatsAppService.SendTextMessageAsync(from,
                        "Opção não reconhecida. Digite *menu* para ver as opções.");
                    break;
            }
        }

        // Processar resposta de lista
        private async Task ProcessListResponseAsync(string from, string listId)
        {
            _logger.LogInformation("Item da lista selecionado: {ListId}", listId);

            switch (listId)
            {
                case "srv_consultoria":
                    await _whatsAppService.SendTextMessageAsync(from,
                        "📊 *Consultoria*\n\n" +
                        "Oferecemos consultoria especializada em:\n" +
                        "• Desenvolvimento de Sistemas\n" +
                        "• Integração de APIs\n" +
                        "• Automação de Processos\n\n" +
                        "Entre em contato: contato@zapsyso.com");
                    break;

                case "srv_desenvolvimento":
                    await _whatsAppService.SendTextMessageAsync(from,
                        "💻 *Desenvolvimento*\n\n" +
                        "Desenvolvemos soluções personalizadas:\n" +
                        "• Aplicações Web\n" +
                        "• APIs REST\n" +
                        "• Integrações WhatsApp\n\n" +
                        "Solicite um orçamento!");
                    break;

                case "srv_suporte":
                    await _whatsAppService.SendTextMessageAsync(from,
                        "🛡️ *Suporte Técnico*\n\n" +
                        "Suporte técnico especializado:\n" +
                        "• Manutenção de sistemas\n" +
                        "• Correção de bugs\n" +
                        "• Atualizações\n\n" +
                        "Disponível 24/7");
                    break;

                default:
                    await _whatsAppService.SendTextMessageAsync(from,
                        "Opção não reconhecida. Digite *menu* para voltar.");
                    break;
            }

            // Após mostrar info, oferecer voltar ao menu
            await Task.Delay(1000);
            var backButton = new List<(string id, string title)>
            {
                ("btn_voltar", "⬅️ Voltar ao Menu")
            };
            await _whatsAppService.SendButtonMessageAsync(from,
                "Precisa de mais alguma coisa?",
                backButton);
        }

        // Menu de serviços (com lista)
        private async Task SendFinanceiroMenuAsync(string to)
        {
            var services = new List<(string id, string title, string description)>
            {
                ("srv_consultoria", "Consultoria", "Consultoria especializada em tecnologia"),
                ("srv_desenvolvimento", "Desenvolvimento", "Desenvolvimento de sistemas personalizados"),
                ("srv_suporte", "Suporte Técnico", "Suporte e manutenção de sistemas")
            };

            await _whatsAppService.SendListMessageAsync(
                to: to,
                bodyText: "Confira nossos serviços disponíveis:",
                buttonText: "Ver Serviços",
                listItems: services,
                headerText: "🛠️ Nossos Serviços",
                footerText: "Selecione para mais detalhes"
            );
        }

        // Opções de suporte
        private async Task SendSupportOptionsAsync(string to)
        {
            await _whatsAppService.SendTextMessageAsync(to,
                "💬 *Suporte*\n\n" +
                "Entre em contato:\n\n" +
                "📧 Email: sysotecnologia@zapsyso.com\n" +
                "📞 Telefone: (11) 9999-9999\n" +
                "⏰ Horário: Seg-Sex 9h às 18h\n\n" +
                "Ou aguarde que um atendente entrará em contato em breve!");

            await Task.Delay(1000);
            var backButton = new List<(string id, string title)>
            {
                ("btn_voltar", "⬅️ Voltar ao Menu")
            };
            await _whatsAppService.SendButtonMessageAsync(to,
                "Precisa de mais alguma coisa?",
                backButton);
        }

        // Informações sobre a empresa
        private async Task SendInfoAsync(string to)
        {
            await _whatsAppService.SendTextMessageAsync(to,
                "ℹ️ *Sobre o ZapSyso*\n\n" +
                "Somos especialistas em desenvolvimento de sistemas e automação com WhatsApp!\n\n" +
                "🚀 Soluções inovadoras\n" +
                "💡 Tecnologia de ponta\n" +
                "🤝 Atendimento personalizado\n\n" +
                "Visite: www.sysotecnologia.com.br");

            await Task.Delay(1000);
            var backButton = new List<(string id, string title)>
            {
                ("btn_voltar", "⬅️ Voltar ao Menu")
            };
            await _whatsAppService.SendButtonMessageAsync(to,
                "Precisa de mais alguma coisa?",
                backButton);
        }

        private async Task SendAtendenteAsync(string to)
        {
            await _whatsAppService.SendTextMessageAsync(to,
                "🤝 Solicitação recebida em alguns segundos um técnico entrara em contato\n\n" +
                "Visite: www.sysotecnologia.com.br");

            await Task.Delay(1000);
            var backButton = new List<(string id, string title)>
            {
                ("btn_voltar", "⬅️ Voltar ao Menu")
            };
            await _whatsAppService.SendButtonMessageAsync(to,
                "Precisa de mais alguma coisa?",
                backButton);
        }
    }
}
