using AppSysoHelp.Models;
using AppSysoHelp.Models.WhatApp;
using System.Text.Json;

namespace AppSysoHelp.Service.WhatsService
{
    public class MessageProcessorService
    {
        private readonly WhatsAppService _whatsAppService;
        private readonly ILogger<MessageProcessorService> _logger;
        private readonly SessionManager _sessionManager;
        private readonly HelpDeskIntegrationService _helpDeskService;

        public MessageProcessorService(
            WhatsAppService whatsAppService,
            ILogger<MessageProcessorService> logger,
            SessionManager sessionManager,
            HelpDeskIntegrationService helpDeskService)
        {
            _whatsAppService = whatsAppService;
            _logger = logger;
            _sessionManager = sessionManager;
            _helpDeskService = helpDeskService;
        }

        #region Processamento Principal

        /// <summary>
        /// Método principal para processar mensagem recebida
        /// </summary>
        public async Task ProcessMessageAsync(WhatsAppMessage message, string senderName)
        {
            try
            {
                var from = message.From;
                var messageType = message.Type;

                // Correção de número brasileiro (adiciona o 9)
                if (from.StartsWith("55") && from.Length == 12)
                {
                    var ddd = from.Substring(2, 2);
                    var numero = from.Substring(4);
                    from = $"55{ddd}9{numero}";

                    _logger.LogWarning("Número corrigido de {Original} para {Corrected}",
                        message.From, from);
                }

                // Obter sessão
                var session = await _sessionManager.GetOrCreateSessionAsync(from);

                _logger.LogInformation("Mensagem de {From} ({Name}) - Estado: {State}, Fluxo: {Flow}",
                    from, senderName, session.State, session.CurrentFlow ?? "nenhum");

                if (messageType == "interactive" && message.Interactive != null)
                {
                    _logger.LogWarning("🔍 DEBUG Interactive - ButtonReply: {Button}, ListReply: {List}",
                        message.Interactive.ButtonReply?.Id ?? "null",
                        message.Interactive.ListReply?.Id ?? "null");
                }

                // SE ATENDENTE ESTÁ ATIVO → NÃO PROCESSAR
                if (session.State == 2) // AgentActive
                {
                    _logger.LogInformation("⚠️ Atendente ativo para {From}.  Bot não irá responder.", from);

                    await _sessionManager.SaveMessageAsync(from, "incoming", message.Type ?? "unknown",
                        message.Text?.Body ?? message.Interactive?.ButtonReply?.Id, "customer", message.Id);
                      
                    return;
                }

                // SE ESTÁ AGUARDANDO ATENDENTE → APENAS CONFIRMAR
                if (session.State == 1) // WaitingForAgent
                {
                    _logger.LogInformation("⏳ Cliente {From} aguardando atendente", from);

                    await _whatsAppService.SendTextMessageAsync(from,
                        "Você já está na fila de atendimento. ⏳\n\nEm breve um atendente irá responder!");

                    return;
                }

                // BOT ATIVO - PROCESSAR MENSAGEM
                _logger.LogInformation("🤖 Bot ativo para {From}. Processando mensagem...", from);

                // Salvar mensagem recebida
                await _sessionManager.SaveMessageAsync(from, "incoming", message.Type ?? "unknown",
                    message.Text?.Body ?? message.Interactive?.ButtonReply?.Id, "customer", message.Id);

                // Processar mensagem de texto
                if (messageType == "text" && message.Text != null)
                {
                    var userMessage = message.Text.Body.Trim();
                    await ProcessTextMessageAsync(from, userMessage, senderName, session);
                }
                // Processar resposta de botão
                else if (messageType == "interactive" && message.Interactive != null)
                {
                    if (message.Interactive.ButtonReply != null)
                    {
                        var buttonId = message.Interactive.ButtonReply.Id;
                        await ProcessButtonResponseAsync(from, buttonId, session);
                    }
                    else if (message.Interactive.ListReply != null)
                    {
                        var listId = message.Interactive.ListReply.Id;
                        await ProcessListResponseAsync(from, listId, session);
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

        #endregion

        #region Processamento de Texto

        /// <summary>
        /// Processa mensagem de texto do usuário
        /// Verifica se está em algum fluxo ou se é comando geral
        /// </summary>
        private async Task ProcessTextMessageAsync(string from, string message, string senderName, CustomerSessions session)
        {
            var messageLower = message.ToLower();

            // COMANDOS GLOBAIS (funcionam em qualquer fluxo)
            if (messageLower.Contains("menu") || messageLower.Contains("voltar") || messageLower.Contains("cancelar"))
            {
                await CancelCurrentFlowAsync(from, session);
                await SendMainMenuAsync(from, senderName);
                return;
            }

            // SE ESTÁ EM UM FLUXO, PROCESSAR CONFORME O ESTADO
            if (!string.IsNullOrEmpty(session.CurrentFlow))
            {
                await ProcessFlowMessageAsync(from, message, session);
                return;
            }

            // SAUDAÇÕES INICIAIS (apenas se não estiver em fluxo)
            if (messageLower.Contains("oi") || messageLower.Contains("olá") || messageLower.Contains("ola") ||
                messageLower.Contains("bom dia") || messageLower.Contains("boa tarde") || messageLower.Contains("boa noite"))
            {
                await SendMainMenuAsync(from, senderName);
            }
            // MENSAGEM NÃO RECONHECIDA
            else
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    $"Desculpe, não entendi sua mensagem. 🤔\n\nDigite *menu* para ver as opções disponíveis.");
            }
        }

        #endregion

        #region Fluxo de Criação de Chamado

        /// <summary>
        /// Processa mensagem quando usuário está em um fluxo específico
        /// </summary>
        private async Task ProcessFlowMessageAsync(string from, string message, CustomerSessions session)
        {
            switch (session.CurrentFlow)
            {
                case "awaiting_contact_name":  // ✨ NOVO
                    await HandleContactNameInputAsync(from, message, session);
                    break;

                case "awaiting_document":  // ✨ NOVO
                    await HandleDocumentInputAsync(from, message, session);
                    break;

                case "awaiting_description":
                    await HandleDescriptionInputAsync(from, message, session);
                    break;

                // ========================================
                // 🆕 PROCESSAR COMENTÁRIO DA AVALIAÇÃO
                // ========================================
                case "awaiting_rating_comment":
                    await HandleRatingCommentInputAsync(from, message, session);
                    break;


                default:
                    _logger.LogWarning("Fluxo desconhecido: {Flow}", session.CurrentFlow);
                    await CancelCurrentFlowAsync(from, session);
                    await SendMainMenuAsync(from, "");
                    break;
            }
        }

        /// <summary>
        /// Inicia o fluxo de criação de chamado
        /// Primeiro pede NOME da pessoa, depois CPF/CNPJ
        /// </summary>
        private async Task StartTicketCreationFlowAsync(string from, CustomerSessions session)
        {
            _logger.LogInformation("🎫 Iniciando fluxo de criação de chamado para {Phone}", from);

            // Começar pedindo o NOME da pessoa
            session.CurrentFlow = "awaiting_contact_name";
            session.FlowData = "{}";
            await _sessionManager.UpdateSessionAsync(session);

            await _whatsAppService.SendTextMessageAsync(from,
                "📋 *Abertura de Chamado*\n\n" +
                "Para começar, informe seu *nome completo*:\n\n" +
                "_Exemplo: João da Silva_");
        }


        /// <summary>
        /// Mostra lista de categorias
        /// </summary>
        //private async Task ShowCategoriasAsync(string from)
        //{
        //    var categorias = await _helpDeskService.GetCategoriasAsync();

        //    if (!categorias.Any())
        //    {
        //        await _whatsAppService.SendTextMessageAsync(from,
        //            "❌ Desculpe, não consegui carregar as categorias.\n\nTente novamente mais tarde.");
        //        return;
        //    }

        //    var listItems = categorias.Select(c => (
        //        id: $"cat_{c.CategoriaId}",
        //        title: c.Descricao.Length > 24 ? c.Descricao.Substring(0, 21) + "..." : c.Descricao
        //        //description: "Categoria de atendimento"
        //    )).ToList();

        //    await _whatsAppService.SendListMessageAsync(
        //        to: from,
        //        bodyText: "Selecione a *categoria* do seu problema:",
        //        buttonText: "📋 Ver Categorias",
        //        listItems: listItems,
        //        headerText: "Categorias Disponíveis",
        //        footerText: "Digite 'menu' para voltar"
        //    );
        //}

        private async Task ShowCategoriasAsync(string from, int pagina = 1)
        {
            var categorias = await _helpDeskService.GetCategoriasAsync();

            if (!categorias.Any())
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "❌ Desculpe, não consegui carregar as categorias.\n\nTente novamente mais tarde.");
                return;
            }

            var categoriasList = categorias.ToList();
            var totalCategorias = categoriasList.Count;

            const int itensPorPagina = 10;
            var totalPaginas = (int)Math.Ceiling(totalCategorias / (double)itensPorPagina);

            // Validar página
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            // Calcular índices
            var inicio = (pagina - 1) * itensPorPagina;
            var fim = Math.Min(inicio + itensPorPagina, totalCategorias);

            _logger.LogInformation("Exibindo categorias {Inicio}-{Fim} de {Total} (página {Pagina}/{TotalPag}) para {Phone}",
                inicio + 1, fim, totalCategorias, pagina, totalPaginas, from);

            // Pegar categorias da página atual
            var categoriasPage = categoriasList
                .Skip(inicio)
                .Take(itensPorPagina)
                .ToList();

            // Criar seção com categorias da página
            var sections = new List<(string sectionTitle, List<(string id, string title)> items)>
    {
        (
            sectionTitle: $"📂 Itens {inicio + 1}-{fim}",
            items: categoriasPage. Select(c => (
                id: $"cat_{c.CategoriaId}",
                title: c. Descricao. Length > 24 ? c.Descricao.Substring(0, 21) + "..." : c. Descricao
            )).ToList()
        )
    };

            // Enviar lista
            await _whatsAppService.SendMultiSectionListMessageAsync(
                to: from,
                headerText: $"📂 Categorias (Pág {pagina}/{totalPaginas})",
                bodyText: $"Mostrando {inicio + 1}-{fim} de {totalCategorias} categorias.\n\nSelecione a categoria:",
                footerText: "Use os botões para navegar",
                buttonText: "📋 Ver Categorias",
                sections: sections
            );

            // Aguardar um pouco antes de enviar botões
            await Task.Delay(500);

            // Criar botões de navegação
            var buttons = new List<(string id, string text)>();

            if (pagina > 1)
            {
                buttons.Add(($"cat_page_{pagina - 1}", "⬅️ Anterior"));
            }

            if (pagina < totalPaginas)
            {
                buttons.Add(($"cat_page_{pagina + 1}", "⏭️ Próxima"));
            }

            buttons.Add(("menu_principal", "🏠 Menu"));

            // Enviar botões de navegação
            if (buttons.Count <= 3)
            {
                await _whatsAppService.SendButtonMessageAsync(
                    to: from,
                    bodyText: $"📄 Página {pagina} de {totalPaginas}",
                    buttons: buttons
                );
            }

            _logger.LogInformation("✅ Lista de categorias (página {Pagina}) enviada com {Count} itens", pagina, categoriasPage.Count);
        }

        /// <summary>
        /// Mostra lista de subcategorias
        /// </summary>
        //private async Task ShowSubCategoriasAsync(string from, long categoriaId)
        //{
        //    var subCategorias = await _helpDeskService.GetSubCategoriasByCategoriaAsync(categoriaId);

        //    if (!subCategorias.Any())
        //    {
        //        await _whatsAppService.SendTextMessageAsync(from,
        //            "❌ Não há subcategorias disponíveis para esta categoria.\n\nDigite *menu* para voltar.");
        //        return;
        //    }

        //    var listItems = subCategorias.Select(sc => (
        //        id: $"sub_{sc.SubCategoriaId}",
        //        title: sc.Descricao.Length > 24 ? sc.Descricao.Substring(0, 21) + "..." : sc.Descricao
        //        //description: sc.Prioridade ?? "Normal"
        //    )).ToList();

        //    await _whatsAppService.SendListMessageAsync(
        //        to: from,
        //        bodyText: "Agora escolha o *tipo específico* do problema:",
        //        buttonText: "🔧 Ver Problemas",
        //        listItems: listItems,
        //        headerText: "Tipos de Problema",
        //        footerText: "Digite 'menu' para cancelar"
        //    );
        //}

        private async Task ShowSubCategoriasAsync(string from, long categoriaId, int pagina = 1)
        {
            var subCategorias = await _helpDeskService.GetSubCategoriasByCategoriaAsync(categoriaId);

            if (!subCategorias.Any())
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "❌ Não há subcategorias disponíveis para esta categoria.\n\nDigite *menu* para voltar.");
                return;
            }

            var subCategoriasList = subCategorias.ToList();
            var totalSubCategorias = subCategoriasList.Count;

            const int itensPorPagina = 10;
            var totalPaginas = (int)Math.Ceiling(totalSubCategorias / (double)itensPorPagina);

            // Validar página
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            // Calcular índices
            var inicio = (pagina - 1) * itensPorPagina;
            var fim = Math.Min(inicio + itensPorPagina, totalSubCategorias);

            _logger.LogInformation("Exibindo subcategorias {Inicio}-{Fim} de {Total} da categoria {CatId} (página {Pagina}/{TotalPag}) para {Phone}",
                inicio + 1, fim, totalSubCategorias, categoriaId, pagina, totalPaginas, from);

            // Pegar subcategorias da página atual
            var subCategoriasPage = subCategoriasList
                .Skip(inicio)
                .Take(itensPorPagina)
                .ToList();

            // Criar seção com subcategorias da página
            var sections = new List<(string sectionTitle, List<(string id, string title)> items)>
    {
        (
            sectionTitle: $"🔧 Itens {inicio + 1}-{fim}",
            items: subCategoriasPage. Select(sc => (
                id: $"sub_{sc.SubCategoriaId}",
                title: sc.Descricao.Length > 24 ? sc.Descricao.Substring(0, 21) + "..." : sc.Descricao
            )).ToList()
        )
    };

            // Enviar lista
            await _whatsAppService.SendMultiSectionListMessageAsync(
                to: from,
                headerText: $"🔧 Subcategorias (Pág {pagina}/{totalPaginas})",
                bodyText: $"Mostrando {inicio + 1}-{fim} de {totalSubCategorias} tipos de problema.\n\nSelecione o tipo específico:",
                footerText: "Use os botões para navegar",
                buttonText: "🔧 Ver Problemas",
                sections: sections
            );

            // Aguardar um pouco antes de enviar botões
            await Task.Delay(500);

            // Criar botões de navegação
            var buttons = new List<(string id, string text)>();

            if (pagina > 1)
            {
                buttons.Add(($"sub_page_{categoriaId}_{pagina - 1}", "⬅️ Anterior"));
            }

            if (pagina < totalPaginas)
            {
                buttons.Add(($"sub_page_{categoriaId}_{pagina + 1}", "⏭️ Próxima"));
            }

            buttons.Add(("btn_voltar", "🔙 Voltar"));

            // Enviar botões de navegação
            if (buttons.Count <= 3)
            {
                await _whatsAppService.SendButtonMessageAsync(
                    to: from,
                    bodyText: $"📄 Página {pagina} de {totalPaginas}",
                    buttons: buttons
                );
            }

            _logger.LogInformation("✅ Lista de subcategorias (página {Pagina}) enviada com {Count} itens", pagina, subCategoriasPage.Count);
        }

        /// <summary>
        /// Processa descrição do problema e CRIA o chamado
        /// </summary>
        private async Task HandleDescriptionInputAsync(string from, string description, CustomerSessions session)
        {
            if (description.Length < 10)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "⚠️ Por favor, descreva o problema com mais detalhes (mínimo 10 caracteres):");
                return;
            }

            try
            {
                // Recuperar dados do fluxo
                var flowData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(session.FlowData ?? "{}");

                var contactName = flowData.ContainsKey("contactName")
                    ? flowData["contactName"].GetString()
                    : "Cliente";

                var subCategoriaId = flowData.ContainsKey("subCategoriaId")
                    ? flowData["subCategoriaId"].GetInt64()
                    : 0;

                var clienteId = flowData.ContainsKey("clienteId") && flowData["clienteId"].ValueKind != JsonValueKind.Null
                    ? (long?)flowData["clienteId"].GetInt64()
                    : null;

                var companyName = flowData.ContainsKey("companyName") && flowData["companyName"].ValueKind != JsonValueKind.Null
                    ? flowData["companyName"].GetString()
                    : null;

                var document = flowData.ContainsKey("document") && flowData["document"].ValueKind != JsonValueKind.Null
                    ? flowData["document"].GetString()
                    : null;

                if (subCategoriaId == 0)
                {
                    _logger.LogError("SubCategoriaId não encontrado no FlowData para {Phone}", from);
                    await CancelCurrentFlowAsync(from, session);
                    await _whatsAppService.SendTextMessageAsync(from,
                        "❌ Ocorreu um erro.  Por favor, tente novamente.");
                    return;
                }

                // Mostrar loading
                await _whatsAppService.SendTextMessageAsync(from,
                    "⏳ Criando seu chamado...");

                // CRIAR CHAMADO
                var chamado = await _helpDeskService.CreateTicketFromWhatsAppAsync(
                    phoneNumber: from,
                    contactName: contactName,
                    subCategoriaId: subCategoriaId,
                    description: description,
                    clienteId: clienteId,
                    companyName: companyName,
                    document: document,
                    setorId: 1 // Suporte Interno
                );

                if (chamado != null)
                {
                    // Vincular chamado à sessão
                    session.LinkedTicketId = chamado.ChamadoId;
                    session.CurrentFlow = null;
                    session.FlowData = null;
                    await _sessionManager.UpdateSessionAsync(session);

                    // Sincronizar mensagens do WhatsApp com o chamado
                    //await _helpDeskService.SyncWhatsAppMessagesToTicketAsync(chamado.ChamadoId, from);

                    // Notificar cliente
                    await _helpDeskService.NotifyTicketCreatedAsync(chamado.ChamadoId, from);

                    await Task.Delay(1500);

                    // Oferecer opções
                    var buttons = new List<(string id, string title)>
            {
                ("btn_my_tickets", "📋 Meus Chamados"),
                //("btn_agent", "👤 Falar com Atendente"),
                ("btn_menu", "⬅️ Menu Principal")
            };

                    await _whatsAppService.SendButtonMessageAsync(
                        to: from,
                        bodyText: "O fazer agora?",
                        buttons: buttons,
                        footerText: "Estamos à disposição!"
                    );

                    _logger.LogInformation("✅ Chamado #{ChamadoId} criado com sucesso via WhatsApp para {Phone} - Requisitante: {ContactName}",
                        chamado.ChamadoId, from, contactName);
                }
                else
                {
                    await CancelCurrentFlowAsync(from, session);
                    await _whatsAppService.SendTextMessageAsync(from,
                        "❌ Não foi possível criar o chamado.\n\nPor favor, tente novamente ou fale com um atendente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar chamado para {Phone}", from);
                await CancelCurrentFlowAsync(from, session);
                await _whatsAppService.SendTextMessageAsync(from,
                    "❌ Ocorreu um erro ao criar o chamado.\n\nPor favor, tente novamente.");
            }
        }

        /// <summary>
        /// Cancela o fluxo atual e limpa dados temporários
        /// </summary>
        private async Task CancelCurrentFlowAsync(string from, CustomerSessions session)
        {
            if (!string.IsNullOrEmpty(session.CurrentFlow))
            {
                _logger.LogInformation("❌ Cancelando fluxo {Flow} para {Phone}", session.CurrentFlow, from);

                session.CurrentFlow = null;
                session.FlowData = null;
                await _sessionManager.UpdateSessionAsync(session);
            }
        }

        #endregion

        #region Processamento de Botões

        /// <summary>
        /// Processa clique em botão
        /// </summary>
        private async Task ProcessButtonResponseAsync(string from, string buttonId, CustomerSessions session)
        {
            _logger.LogInformation("Botão clicado: {ButtonId}", buttonId);

            // ========================================
            // 🆕 PROCESSAR AVALIAÇÃO (rating_123_5)
            // ========================================
            if (buttonId.StartsWith("rating_"))
            {
                await HandleRatingButtonAsync(from, buttonId, session);
                return;
            }

            if (buttonId.StartsWith("comment_"))
            {
                await HandleCommentButtonAsync(from, buttonId, session);
                return;
            }

            if (buttonId.StartsWith("cat_page_"))
            {
                var paginaStr = buttonId.Replace("cat_page_", "");

                if (int.TryParse(paginaStr, out int numeroPagina))
                {
                    _logger.LogInformation("Cliente {Phone} navegou para página {Pagina} de categorias", from, numeroPagina);

                    await ShowCategoriasAsync(from, numeroPagina);
                    return;
                }
                else
                {
                    _logger.LogWarning("Página inválida no botão: {ButtonId}", buttonId);
                    await _whatsAppService.SendTextMessageAsync(from, "❌ Erro ao navegar.  Tente novamente.");
                    return;
                }
            }

            if (buttonId.StartsWith("sub_page_"))
            {
                var parts = buttonId.Replace("sub_page_", "").Split('_');

                if (parts.Length == 2 &&
                    long.TryParse(parts[0], out long categoriaId) &&
                    int.TryParse(parts[1], out int numeroPagina))
                {
                    _logger.LogInformation("Cliente {Phone} navegou para página {Pagina} de subcategorias da categoria {CatId}",
                        from, numeroPagina, categoriaId);

                    await ShowSubCategoriasAsync(from, categoriaId, numeroPagina);
                    return;
                }
                else
                {
                    _logger.LogWarning("Página de subcategoria inválida no botão: {ButtonId}", buttonId);
                    await _whatsAppService.SendTextMessageAsync(from, "❌ Erro ao navegar. Tente novamente.");
                    return;
                }
            }


            switch (buttonId)
            {
                case "btn_open_ticket":
                    await StartTicketCreationFlowAsync(from, session);
                    break;

                case "btn_my_tickets":
                    await ShowMyTicketsAsync(from);
                    break;

                case "btn_agent":
                    await RequestAgentAsync(from, session);
                    break;

                case "btn_financeiro":
                    await SendFinanceiroMenuAsync(from);
                    break;

                case "btn_info":
                    await SendInfoAsync(from);
                    break;

                case "btn_menu":
                case "btn_voltar":
                    await CancelCurrentFlowAsync(from, session);
                    await SendMainMenuAsync(from, "");
                    break;

                default:
                    await _whatsAppService.SendTextMessageAsync(from,
                        "Opção não reconhecida. Digite *menu* para ver as opções.");
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Processa clique em botão de avaliação (rating_123_5)
        /// Extrai atendimentoId e nota, salva no banco
        /// </summary>
        private async Task HandleRatingButtonAsync(string from, string buttonId, CustomerSessions session)
        {
            try
            {
                // Formato esperado: rating_123_5
                // rating = prefixo
                // 123 = atendimentoId
                // 5 = nota (1 a 5)

                var parts = buttonId.Split('_');

                if (parts.Length != 3)
                {
                    _logger.LogWarning("Formato inválido de buttonId de avaliação: {ButtonId}", buttonId);
                    await _whatsAppService.SendTextMessageAsync(from,
                        "Desculpe, ocorreu um erro ao processar sua avaliação. 😕");
                    return;
                }

                // Extrair dados
                var atendimentoIdStr = parts[1];
                var notaStr = parts[2];

                if (!long.TryParse(atendimentoIdStr, out long atendimentoId))
                {
                    _logger.LogError("AtendimentoId inválido: {AtendimentoId}", atendimentoIdStr);
                    return;
                }

                if (!int.TryParse(notaStr, out int nota) || nota < 1 || nota > 5)
                {
                    _logger.LogError("Nota inválida: {Nota}", notaStr);
                    return;
                }

                _logger.LogInformation("📊 Cliente {Phone} avaliou atendimento #{AtendimentoId} com nota {Nota}",
                    from, atendimentoId, nota);

                // Buscar atendimento no banco
                var atendimento = await _helpDeskService.GetAtendimentoByIdAsync(atendimentoId);

                if (atendimento == null)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} não encontrado", atendimentoId);
                    await _whatsAppService.SendTextMessageAsync(from,
                        "Desculpe, não consegui localizar este atendimento. 😕");
                    return;
                }

                // Verificar se já foi avaliado
                if (atendimento.AvaliacaoNota.HasValue)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} já foi avaliado anteriormente com nota {NotaAnterior}",
                        atendimentoId, atendimento.AvaliacaoNota.Value);

                    var estrelas = new string('⭐', atendimento.AvaliacaoNota.Value);
                    await _whatsAppService.SendTextMessageAsync(from,
                        $"Você já avaliou este atendimento anteriormente!\n\n" +
                        $"Nota: {estrelas} {atendimento.AvaliacaoNota.Value}/5\n\n" +
                        $"Obrigado pelo feedback! 😊");
                    return;
                }

                // Salvar avaliação
                await _helpDeskService.SaveRatingAsync(atendimentoId, nota);

                // Montar resposta com estrelas
                var estrelasResposta = new string('⭐', nota);

                await _whatsAppService.SendTextMessageAsync(from,
                    $"{estrelasResposta}\n\n*Obrigado pela avaliação!*");

                // Se nota for BAIXA (1 a 3), pedir comentário
                if (nota <= 3)
                {
                    await Task.Delay(800); // Delay para parecer mais natural

                    await _whatsAppService.SendTextMessageAsync(from,
                        "Sentimos muito que sua experiência não tenha sido ideal. 😔\n\n" +
                        "Poderia nos dizer o que podemos melhorar? 💬");

                    // Criar botões de opção
                    var buttons = new List<(string id, string title)>
            {
                ($"comment_{atendimentoId}_yes", "✍️ Deixar comentário"),
                ($"comment_{atendimentoId}_no", "❌ Não, obrigado")
            };

                    await _whatsAppService.SendButtonMessageAsync(
                        to: from,
                        bodyText: "Escolha uma opção:",
                        buttons: buttons,
                        footerText: "Sua opinião é importante!"
                    );

                    // Salvar no FlowData que está aguardando resposta de comentário
                    var flowData = new
                    {
                        atendimentoId = atendimentoId,
                        nota = nota
                    };

                    session.CurrentFlow = "awaiting_comment_choice";
                    session.FlowData = System.Text.Json.JsonSerializer.Serialize(flowData);
                    await _sessionManager.UpdateSessionAsync(session);
                }
                else
                {
                    // Nota alta (4-5) - apenas agradecer
                    await Task.Delay(500);

                    await _whatsAppService.SendTextMessageAsync(from,
                        "Ficamos felizes em ajudar! 😊\n\n" +
                        "Se precisar de algo mais, estamos à disposição!\n\n" +
                        "Digite *menu* para ver as opções.");
                }

                _logger.LogInformation("✅ Avaliação do atendimento #{AtendimentoId} processada com sucesso - Nota: {Nota}",
                    atendimentoId, nota);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar avaliação do cliente {Phone}", from);
                await _whatsAppService.SendTextMessageAsync(from,
                    "Desculpe, ocorreu um erro.  Por favor, tente novamente.");
            }
        }



        #region Processamento de Listas

        /// <summary>
        /// Processa seleção de item de lista
        /// </summary>
        private async Task ProcessListResponseAsync(string from, string listId, CustomerSessions session)
        {
            _logger.LogInformation("Item da lista selecionado: {ListId}", listId);

            // ========================================
            // 🆕 NAVEGAÇÃO DE PÁGINAS (BOTÕES)
            // ========================================
            // Nota: Paginação é processada via BOTÕES, não via LISTA
            // Mas adicionamos validação aqui por segurança
            if (listId.StartsWith("cat_page_"))
            {
                _logger.LogWarning("Paginação '{ListId}' recebida via lista (deveria ser botão).  Ignorando.", listId);
                return;
            }

            // ========================================
            // CATEGORIA SELECIONADA
            // ========================================
            if (listId.StartsWith("cat_") && !listId.StartsWith("cat_page_"))
            {
                var categoriaIdStr = listId.Replace("cat_", "");

                if (long.TryParse(categoriaIdStr, out long categoriaId))
                {
                    await HandleCategorySelectionAsync(from, categoriaId, session);
                    return;
                }
                else
                {
                    _logger.LogWarning("ID de categoria inválido: {ListId}", listId);
                    await _whatsAppService.SendTextMessageAsync(from, "❌ Categoria inválida.  Tente novamente.");
                    return;
                }
            }

            // ========================================
            // SUBCATEGORIA SELECIONADA
            // ========================================
            if (listId.StartsWith("sub_") && !listId.StartsWith("sub_page_"))
            {
                var subCategoriaIdStr = listId.Replace("sub_", "");

                if (long.TryParse(subCategoriaIdStr, out long subCategoriaId))
                {
                    await HandleSubCategorySelectionAsync(from, subCategoriaId, session);
                    return;
                }
                else
                {
                    _logger.LogWarning("ID de subcategoria inválido: {ListId}", listId);
                    await _whatsAppService.SendTextMessageAsync(from, "❌ Subcategoria inválida. Tente novamente.");
                    return;
                }
            }

            // ========================================
            // OUTROS (menu de serviços)
            // ========================================
            switch (listId)
            {
                case "srv_consultoria":
                    await _whatsAppService.SendTextMessageAsync(from,
                        "📊 *Consultoria*\n\n" +
                        "Oferecemos consultoria especializada em:\n" +
                        "• Desenvolvimento de Sistemas\n" +
                        "• Integração de APIs\n" +
                        "• Automação de Processos\n\n" +
                        "Entre em contato: contato@sysotecnologia.com");
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

            await Task.Delay(1000);
            var backButton = new List<(string id, string title)>
    {
        ("btn_voltar", "⬅️ Voltar ao Menu")
    };
            await _whatsAppService.SendButtonMessageAsync(from,
                "Precisa de mais alguma coisa? ",
                backButton);
        }

        /// <summary>
        /// Processa seleção de categoria
        /// </summary>
        private async Task HandleCategorySelectionAsync(string from, long categoriaId, CustomerSessions session)
        {
            if (session.CurrentFlow != "awaiting_category")
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "⚠️ Por favor, inicie um novo chamado digitando *menu*");
                return;
            }

            // Atualizar FlowData com categoria
            var flowData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(session.FlowData ?? "{}");
            flowData["categoriaId"] = JsonSerializer.SerializeToElement(categoriaId);

            session.CurrentFlow = "awaiting_subcategory";
            session.FlowData = JsonSerializer.Serialize(flowData);
            await _sessionManager.UpdateSessionAsync(session);

            await ShowSubCategoriasAsync(from, categoriaId);
        }

        /// <summary>
        /// Processa seleção de subcategoria
        /// </summary>
        private async Task HandleSubCategorySelectionAsync(string from, long subCategoriaId, CustomerSessions session)
        {
            if (session.CurrentFlow != "awaiting_subcategory")
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "⚠️ Por favor, inicie um novo chamado digitando *menu*");
                return;
            }

            // Buscar subcategoria para mostrar prioridade
            var subCategoria = await _helpDeskService.GetSubCategoriaByIdAsync(subCategoriaId);

            if (subCategoria == null)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "❌ Subcategoria não encontrada.  Digite *menu* para tentar novamente.");
                await CancelCurrentFlowAsync(from, session);
                return;
            }

            var prioridadeIcon = subCategoria.Prioridade switch
            {
                "Urgente" => "🔴",
                "Alta" => "🟠",
                "Media" => "🟡",
                _ => "🟢"
            };

            // Atualizar FlowData com subcategoria
            var flowData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(session.FlowData ?? "{}");
            flowData["subCategoriaId"] = JsonSerializer.SerializeToElement(subCategoriaId);

            session.CurrentFlow = "awaiting_description";
            session.FlowData = JsonSerializer.Serialize(flowData);
            await _sessionManager.UpdateSessionAsync(session);

            await _whatsAppService.SendTextMessageAsync(from,
                $"✅ Categoria selecionada: *{subCategoria.Descricao}*\n" +
                $"{prioridadeIcon} Prioridade: *{subCategoria.Prioridade ?? "Normal"}*\n\n" +
                $"📝 Agora descreva o problema com o máximo de detalhes possível:");
        }

        #endregion

        #region Menus e Mensagens

        /// <summary>
        /// Envia menu principal
        /// </summary>
        private async Task SendMainMenuAsync(string to, string senderName)
        {
            var greeting = string.IsNullOrEmpty(senderName) ? "Olá" : $"Olá *{senderName}*";

            var buttons = new List<(string id, string title)>
            {
                ("btn_open_ticket", "🎫 Abrir Chamado"),
                ("btn_my_tickets", "📋 Meus Chamados")
                //("btn_agent", "👤 Atendente")
            };

            await _whatsAppService.SendButtonMessageAsync(
                to: to,
                bodyText: $"{greeting}!  👋\n\nBem-vindo ao *HelpDesk SysoTecnologia*!\n\nComo posso ajudar você hoje?",
                buttons: buttons,
                headerText: "Menu Principal",
                footerText: "Selecione uma opção abaixo"
            );
        }

        /// <summary>
        /// Mostra chamados ativos do cliente
        /// </summary>
        private async Task ShowMyTicketsAsync(string from)
        {
            var chamados = await _helpDeskService.GetActiveTicketsByPhoneAsync(from);

            if (!chamados.Any())
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "📋 *Meus Chamados*\n\n" +
                    "Você não possui chamados em aberto no momento.\n\n" +
                    "Digite *menu* para voltar.");
                return;
            }

            var message = "📋 *Seus Chamados em Aberto*\n\n";

            foreach (var chamado in chamados.Take(5))
            {
                var prioridadeIcon = chamado.Prioridade switch
                {
                    "Urgente" => "🔴",
                    "Alta" => "🟠",
                    "Media" => "🟡",
                    _ => "🟢"
                };

                var statusIcon = chamado.FkSituacaoChamadoId switch
                {
                    1 => "🆕",
                    2 => "🔄",
                    _ => "✅"
                };

                message += $"{statusIcon} *Chamado #{chamado.ChamadoId}*\n";
                message += $"{prioridadeIcon} Prioridade: {chamado.Prioridade}\n";
                message += $"📅 Aberto em: {chamado.DataCriacao?.ToString("dd/MM/yyyy HH:mm")}\n";

                if (chamado.FkTecnico != null)
                {
                    message += $"👨‍💻 Técnico: {chamado.FkTecnico.NomeCompleto}\n";
                }

                message += "\n";
            }

            message += "_Digite *menu* para voltar_";

            await _whatsAppService.SendTextMessageAsync(from, message);
        }

        /// <summary>
        /// Solicita atendimento humano
        /// </summary>
        private async Task RequestAgentAsync(string from, CustomerSessions session)
        {
            await CancelCurrentFlowAsync(from, session);
            await _sessionManager.UpdateStateAsync(from, 1); // WaitingForAgent

            await _whatsAppService.SendTextMessageAsync(from,
                "👤 *Atendimento Humano Solicitado*\n\n" +
                "Você foi colocado na fila de atendimento.\n\n" +
                "Em breve um de nossos atendentes irá responder!  ⏳\n\n" +
                "_Aguarde, por favor.. ._");

            await _sessionManager.SaveMessageAsync(from, "outgoing", "text",
                "Cliente solicitou atendimento humano", "bot");

            _logger.LogWarning("🔔 Cliente {From} aguardando atendente!", from);
        }

        /// <summary>
        /// Menu de serviços (financeiro)
        /// </summary>
        private async Task SendFinanceiroMenuAsync(string to)
        {
            var services = new List<(string id, string title)>
            {
                ("srv_consultoria", "Consultoria"),
                ("srv_desenvolvimento", "Desenvolvimento"),
                ("srv_suporte", "Suporte Técnico")
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

        /// <summary>
        /// Informações sobre a empresa
        /// </summary>
        private async Task SendInfoAsync(string to)
        {
            await _whatsAppService.SendTextMessageAsync(to,
                "ℹ️ *Sobre a SysoTecnologia*\n\n" +
                "Somos especialistas em desenvolvimento de sistemas e automação com WhatsApp!\n\n" +
                "🚀 Soluções inovadoras\n" +
                "💡 Tecnologia de ponta\n" +
                "🤝 Atendimento personalizado\n\n" +
                "Visite: www.sysotecnologia. com. br");

            await Task.Delay(1000);
            var backButton = new List<(string id, string title)>
            {
                ("btn_voltar", "⬅️ Voltar ao Menu")
            };
            await _whatsAppService.SendButtonMessageAsync(to,
                "Precisa de mais alguma coisa?",
                backButton);
        }

        #endregion
        /// <summary>
        /// Processa nome da pessoa (requisitante do chamado)
        /// </summary>
        private async Task HandleContactNameInputAsync(string from, string name, CustomerSessions session)
        {
            if (name.Length < 3)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "⚠️ Por favor, informe seu nome completo (mínimo 3 caracteres):");
                return;
            }

            // Salvar nome no FlowData
            var flowData = new
            {
                contactName = name
            };

            session.CurrentFlow = "awaiting_document";
            session.FlowData = JsonSerializer.Serialize(flowData);
            await _sessionManager.UpdateSessionAsync(session);

            await _whatsAppService.SendTextMessageAsync(from,
                $"✅ Obrigado, *{name}*!\n\n" +
                $"Agora informe o *CPF* ou *CNPJ* da empresa:\n\n" +
                $"_Exemplo: 123.456.789-00 ou 12.345.678/0001-90_");
        }

        /// <summary>
        /// Processa CPF/CNPJ informado pelo cliente
        /// </summary>
        private async Task HandleDocumentInputAsync(string from, string document, CustomerSessions session)
        {
            // Validar formato básico
            if (!_helpDeskService.IsValidDocument(document))
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "⚠️ CPF/CNPJ inválido.\n\n" +
                    "Por favor, informe um *CPF* (11 dígitos) ou *CNPJ* (14 dígitos):\n\n" +
                    "_Exemplo: 123.456. 789-00 ou 12. 345.678/0001-90_");
                return;
            }

            // Recuperar nome da pessoa do FlowData
            var currentFlowData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(session.FlowData ?? "{}");
            var contactName = currentFlowData.ContainsKey("contactName")
                ? currentFlowData["contactName"].GetString()
                : "Cliente";

            // Buscar cliente no banco pelo documento
            var cliente = await _helpDeskService.FindClientByDocumentAsync(document);

            if (cliente != null)
            {
                // ✅ CLIENTE ENCONTRADO
                var companyName = cliente.Fantasia ?? cliente.NomeCliente;

                var flowData = new
                {
                    contactName = contactName,      // Nome da PESSOA
                    document = document,             // CPF/CNPJ informado
                    clienteId = cliente.ClienteId,  // ID do cliente
                    companyName = companyName        // Nome da empresa
                };

                session.CurrentFlow = "awaiting_category";
                session.FlowData = JsonSerializer.Serialize(flowData);
                await _sessionManager.UpdateSessionAsync(session);

                await _whatsAppService.SendTextMessageAsync(from,
                    $"✅ *Empresa identificada!*\n\n" +
                    $"📌 {companyName}\n" +
                    $"📄 {document}\n\n" +
                    $"Vamos abrir um chamado para você!");

                await Task.Delay(1500);
                await ShowCategoriasAsync(from);
            }
            else
            {
                // ❌ CLIENTE NÃO ENCONTRADO - BLOQUEAR E PEDIR NOVAMENTE

                _logger.LogWarning("Documento {Document} NÃO encontrado para {Phone}. Bloqueando abertura de chamado.", document, from);

                await _whatsAppService.SendTextMessageAsync(from,
                    $"❌ *CPF/CNPJ não encontrado!*\n\n" +
                    $"📄 Documento informado: *{document}*\n\n" +
                    $"⚠️ Somente *clientes cadastrados* podem abrir chamados.\n\n" +
                    $"Por favor, verifique se digitou corretamente e tente novamente.\n\n" +
                    $"Digite o *CPF* ou *CNPJ* da empresa:\n\n" +
                    $"Digite *Menu* para volta ao início !");


                // 🔥 MANTER NO ESTADO "aguardando_documento" para cliente tentar novamente
                session.CurrentFlow = "awaiting_document";
                session.FlowData = JsonSerializer.Serialize(new { contactName });
                await _sessionManager.UpdateSessionAsync(session);

                return; // 🔥 IMPORTANTE: Não continua o fluxo
            }
        }


        /// <summary>
        /// Processa clique em botão de comentário (comment_123_yes ou comment_123_no)
        /// </summary>
        private async Task HandleCommentButtonAsync(string from, string buttonId, CustomerSessions session)
        {
            try
            {
                // Formato esperado: comment_123_yes ou comment_123_no
                var parts = buttonId.Split('_');

                if (parts.Length != 3)
                {
                    _logger.LogWarning("Formato inválido de buttonId de comentário: {ButtonId}", buttonId);
                    return;
                }

                var atendimentoIdStr = parts[1];
                var escolha = parts[2]; // "yes" ou "no"

                if (!long.TryParse(atendimentoIdStr, out long atendimentoId))
                {
                    _logger.LogError("AtendimentoId inválido no botão de comentário: {AtendimentoId}", atendimentoIdStr);
                    return;
                }

                _logger.LogInformation("Cliente {Phone} clicou em '{Escolha}' para comentário do atendimento #{AtendimentoId}",
                    from, escolha, atendimentoId);

                if (escolha == "yes")
                {
                    // Cliente quer deixar comentário
                    await _whatsAppService.SendTextMessageAsync(from,
                        "📝 *Deixe seu comentário:*\n\n" +
                        "Escreva o que podemos melhorar ou o que você achou do atendimento.\n\n" +
                        "_Digite sua mensagem abaixo_ ⬇️");

                    // Salvar no FlowData qual atendimento está comentando
                    var flowData = new
                    {
                        atendimentoId = atendimentoId
                    };

                    session.CurrentFlow = "awaiting_rating_comment";
                    session.FlowData = System.Text.Json.JsonSerializer.Serialize(flowData);
                    await _sessionManager.UpdateSessionAsync(session);

                    _logger.LogInformation("Aguardando comentário do cliente {Phone} para atendimento #{AtendimentoId}",
                        from, atendimentoId);
                }
                else if (escolha == "no")
                {
                    // Cliente não quer deixar comentário
                    await _whatsAppService.SendTextMessageAsync(from,
                        "✅ *Obrigado pelo feedback!*\n\n" +
                        "Sua avaliação é muito importante para nós!  😊\n\n" +
                        "Se precisar de algo mais, estamos à disposição!\n\n" +
                        "Digite *menu* para ver as opções.");

                    // Limpar fluxo
                    session.CurrentFlow = null;
                    session.FlowData = null;
                    await _sessionManager.UpdateSessionAsync(session);

                    _logger.LogInformation("Cliente {Phone} optou por não deixar comentário para atendimento #{AtendimentoId}",
                        from, atendimentoId);
                }
                else
                {
                    _logger.LogWarning("Escolha inválida no botão de comentário: {Escolha}", escolha);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar botão de comentário para {Phone}", from);
                await _whatsAppService.SendTextMessageAsync(from,
                    "Desculpe, ocorreu um erro.  Digite *menu* para voltar.");
            }
        }

        /// <summary>
        /// Processa comentário digitado pelo cliente após avaliar com nota baixa
        /// </summary>
        private async Task HandleRatingCommentInputAsync(string from, string comentario, CustomerSessions session)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(comentario))
                {
                    await _whatsAppService.SendTextMessageAsync(from,
                        "⚠️ Por favor, escreva seu comentário:");
                    return;
                }

                if (comentario.Length < 3)
                {
                    await _whatsAppService.SendTextMessageAsync(from,
                        "⚠️ Comentário muito curto. Por favor, escreva um pouco mais (mínimo 3 caracteres):");
                    return;
                }

                // Recuperar atendimentoId do FlowData
                var flowData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(
                    session.FlowData ?? "{}");

                if (!flowData.ContainsKey("atendimentoId"))
                {
                    _logger.LogError("AtendimentoId não encontrado no FlowData para {Phone}", from);
                    await _whatsAppService.SendTextMessageAsync(from,
                        "❌ Ocorreu um erro.  Por favor, tente novamente.");
                    await CancelCurrentFlowAsync(from, session);
                    return;
                }

                var atendimentoId = flowData["atendimentoId"].GetInt64();

                _logger.LogInformation("💬 Cliente {Phone} enviou comentário para atendimento #{AtendimentoId}: {Comentario}",
                    from, atendimentoId, comentario.Substring(0, Math.Min(50, comentario.Length)) + "...");

                // Mostrar loading
                await _whatsAppService.SendTextMessageAsync(from, "💾 Salvando seu comentário...");

                // Salvar comentário no banco
                var sucesso = await _helpDeskService.SaveRatingCommentAsync(atendimentoId, comentario);

                if (sucesso)
                {
                    await Task.Delay(500);

                    await _whatsAppService.SendTextMessageAsync(from,
                        "✅ *Comentário registrado! *\n\n" +
                        "Muito obrigado pelo seu feedback detalhado! 🙏\n\n" +
                        "Vamos trabalhar para melhorar nosso atendimento!\n\n" +
                        "Se precisar de algo mais, estamos à disposição!\n\n" +
                        "Digite *menu* para ver as opções.");

                    // Limpar fluxo
                    session.CurrentFlow = null;
                    session.FlowData = null;
                    await _sessionManager.UpdateSessionAsync(session);

                    _logger.LogInformation("✅ Comentário salvo com sucesso - Atendimento #{AtendimentoId}, Cliente: {Phone}",
                        atendimentoId, from);
                }
                else
                {
                    await _whatsAppService.SendTextMessageAsync(from,
                        "❌ Não foi possível salvar o comentário.\n\n" +
                        "Por favor, tente novamente ou digite *menu* para voltar.");

                    _logger.LogError("Falha ao salvar comentário do atendimento #{AtendimentoId}", atendimentoId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar comentário da avaliação para {Phone}", from);
                await _whatsAppService.SendTextMessageAsync(from,
                    "❌ Ocorreu um erro ao salvar seu comentário.\n\nPor favor, tente novamente.");
                await CancelCurrentFlowAsync(from, session);
            }
        }


    }
}