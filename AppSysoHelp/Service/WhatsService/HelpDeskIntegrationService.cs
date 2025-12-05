using AppSysoHelp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AppSysoHelp.Service.WhatsService
{
    /// <summary>
    /// Service responsável pela integração entre WhatsApp e HelpDesk
    /// Gerencia criação de chamados, vinculação com clientes e notificações
    /// </summary>
    public class HelpDeskIntegrationService
    {
        private readonly HelpdesksysoContext _context;
        private readonly WhatsAppService _whatsAppService;
        private readonly ILogger<HelpDeskIntegrationService> _logger;
        private readonly SessionManager _sessionManager;

        public HelpDeskIntegrationService(
            SessionManager sessionManager,
            HelpdesksysoContext context,
            WhatsAppService whatsAppService,
            ILogger<HelpDeskIntegrationService> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _logger = logger;
            _sessionManager = sessionManager;
        }

        #region Busca de Clientes

        /// <summary>
        /// Busca cliente por número de telefone/WhatsApp
        /// Procura primeiro no campo WhatsApp, depois em TelefoneCliente
        /// Normaliza números para formato padrão (55DD9NNNNNNNN)
        /// </summary>
        public async Task<Clientes?> FindClientByPhoneAsync(string phoneNumber)
        {
            try
            {
                // Normalizar número
                var normalizedPhone = NormalizePhoneNumber(phoneNumber);

                _logger.LogInformation("Buscando cliente por telefone: {Phone} (normalizado: {Normalized})",
                    phoneNumber, normalizedPhone);

                // Buscar no campo WhatsApp primeiro
                var cliente = await _context.Clientes
                    .Where(c => c.Situacao == true) // Apenas clientes ativos
                    .FirstOrDefaultAsync(c =>
                        c.WhatsApp != null && (
                            c.WhatsApp == phoneNumber ||
                            c.WhatsApp == normalizedPhone ||
                            c.WhatsApp.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "") == normalizedPhone
                        )
                    );

                if (cliente != null)
                {
                    _logger.LogInformation("Cliente encontrado pelo WhatsApp: {ClienteId} - {Nome}",
                        cliente.ClienteId, cliente.NomeCliente);
                    return cliente;
                }

                // Se não encontrou, buscar no TelefoneCliente
                cliente = await _context.Clientes
                    .Where(c => c.Situacao == true)
                    .FirstOrDefaultAsync(c =>
                        c.TelefoneCliente != null && (
                            c.TelefoneCliente == phoneNumber ||
                            c.TelefoneCliente == normalizedPhone ||
                            c.TelefoneCliente.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "") == normalizedPhone
                        )
                    );

                if (cliente != null)
                {
                    _logger.LogInformation("Cliente encontrado pelo TelefoneCliente: {ClienteId} - {Nome}",
                        cliente.ClienteId, cliente.NomeCliente);
                }
                else
                {
                    _logger.LogWarning("Cliente NÃO encontrado para o telefone: {Phone}", phoneNumber);
                }

                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cliente por telefone: {Phone}", phoneNumber);
                return null;
            }
        }

        /// <summary>
        /// Normaliza número de telefone para formato padrão brasileiro
        /// Formato final: 55DD9NNNNNNNN (13 dígitos)
        /// </summary>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return phoneNumber;

            // Remove todos os caracteres não numéricos
            var digitsOnly = Regex.Replace(phoneNumber, @"\D", "");

            // Se já está no formato correto (55DD9NNNNNNNN - 13 dígitos)
            if (digitsOnly.Length == 13 && digitsOnly.StartsWith("55"))
                return digitsOnly;

            // Se está sem o 9 (55DDNNNNNNNN - 12 dígitos)
            if (digitsOnly.Length == 12 && digitsOnly.StartsWith("55"))
            {
                var ddd = digitsOnly.Substring(2, 2);
                var numero = digitsOnly.Substring(4);
                return $"55{ddd}9{numero}";
            }

            // Se tem apenas DDD + número (DD9NNNNNNNN - 11 dígitos)
            if (digitsOnly.Length == 11)
            {
                return $"55{digitsOnly}";
            }

            // Se tem DDD + número sem o 9 (DDNNNNNNNN - 10 dígitos)
            if (digitsOnly.Length == 10)
            {
                var ddd = digitsOnly.Substring(0, 2);
                var numero = digitsOnly.Substring(2);
                return $"55{ddd}9{numero}";
            }

            // Retorna como está se não conseguir normalizar
            return digitsOnly;
        }

        #endregion

        #region Categorias e SubCategorias

        /// <summary>
        /// Busca todas as categorias ativas ordenadas por descrição
        /// </summary>
        public async Task<List<ChamadosCategoria>> GetCategoriasAsync()
        {
            try
            {
                return await _context.ChamadosCategoria
                    .OrderBy(c => c.Descricao)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categorias");
                return new List<ChamadosCategoria>();
            }
        }

        /// <summary>
        /// Busca subcategorias de uma categoria específica
        /// </summary>
        public async Task<List<ChamadosSubCategoria>> GetSubCategoriasByCategoriaAsync(long categoriaId)
        {
            try
            {
                return await _context.ChamadosSubCategoria
                    .Where(sc => sc.FkCategoria == categoriaId && sc.Situacao == true)
                    .OrderBy(sc => sc.Descricao)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar subcategorias da categoria {CategoriaId}", categoriaId);
                return new List<ChamadosSubCategoria>();
            }
        }

        /// <summary>
        /// Busca uma subcategoria específica por ID
        /// </summary>
        public async Task<ChamadosSubCategoria?> GetSubCategoriaByIdAsync(long subCategoriaId)
        {
            try
            {
                return await _context.ChamadosSubCategoria
                    .FirstOrDefaultAsync(sc => sc.SubCategoriaId == subCategoriaId && sc.Situacao == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar subcategoria {SubCategoriaId}", subCategoriaId);
                return null;
            }
        }

        #endregion

        #region Criação de Chamados

        /// <summary>
        /// Cria um novo chamado originado do WhatsApp
        /// </summary>
        /// <param name="phoneNumber">Número de telefone do cliente (formato WhatsApp)</param>
        /// <param name="contactName">Nome da PESSOA que está solicitando (requisitante)</param>
        /// <param name="subCategoriaId">ID da subcategoria selecionada</param>
        /// <param name="description">Descrição do problema</param>
        /// <param name="clienteId">ID do cliente (se encontrado) ou null</param>
        /// <param name="companyName">Nome da empresa (se encontrado)</param>
        /// <param name="document">CPF/CNPJ informado</param>
        /// <param name="setorId">ID do setor (padrão: 1 - Suporte Interno)</param>
        /// <returns>Chamado criado ou null se houver erro</returns>
        public async Task<Chamados?> CreateTicketFromWhatsAppAsync(
            string phoneNumber,
            string contactName,
            long subCategoriaId,
            string description,
            long? clienteId = null,
            string? companyName = null,
            string? document = null,
            long setorId = 1)
        {
            try
            {
                _logger.LogInformation("Criando chamado via WhatsApp para {Phone} - Cliente: {ContactName}",
                    phoneNumber, contactName);

                // Buscar subcategoria para obter prioridade
                var subCategoria = await GetSubCategoriaByIdAsync(subCategoriaId);
                if (subCategoria == null)
                {
                    _logger.LogError("SubCategoria {SubCategoriaId} não encontrada!", subCategoriaId);
                    return null;
                }

                // Determinar prioridade (da subcategoria ou padrão)
                string prioridade = subCategoria.Prioridade ?? "Normal";

                // Montar descrição completa com todas as informações
                var descricaoCompleta = "[WHATSAPP]\n\n";
                descricaoCompleta += $"Requisitante: {contactName}\n";

                if (!string.IsNullOrEmpty(companyName))
                {
                    descricaoCompleta += $"Empresa: {companyName}\n";
                }

                if (!string.IsNullOrEmpty(document))
                {
                    descricaoCompleta += $"CPF/CNPJ: {document}\n";
                }

                descricaoCompleta += $"Telefone: {phoneNumber}\n\n";
                descricaoCompleta += "Descrição do Problema:\n";
                descricaoCompleta += description;

                // Criar chamado
                var chamado = new Chamados
                {
                    // Dados do cliente
                    FkClienteId = clienteId,  // Pode ser null
                    Contato = contactName,    // Nome da PESSOA (requisitante)
                    TelefoneContato = phoneNumber,

                    // Categorização
                    FkSubCategoria = subCategoriaId,
                    FkSetores = setorId,
                    Prioridade = prioridade,

                    // Descrição
                    DescricaoCompleta = descricaoCompleta,

                    // Status e datas
                    FkSituacaoChamadoId = 1, // Aberto
                    DataCriacao = DateTime.UtcNow.AddHours(-4),
                    DataAgendamento = DateTime.UtcNow.AddHours(-4),

                    // Técnico (0 = não atribuído ainda)
                    FkTecnicoId = 0,

                    // Atendente (0 = bot do WhatsApp)
                    FkAtendente = 8,

                    // Tipo de chamado (1 = padrão)
                    FkTipoChamadoId = 1,

                    // Plataforma (1 = Solution - Vector)
                    FkPlataforma = 1,

                    // Marcar como chamado do painel
                    ChamadoPainel = null
                };

                _context.Chamados.Add(chamado);
                await _context.SaveChangesAsync();

                if (clienteId.HasValue)
                {
                    _logger.LogInformation("✅ Chamado #{ChamadoId} criado e VINCULADO ao cliente {ClienteId} ({Company})",
                        chamado.ChamadoId, clienteId.Value, companyName);
                }
                else
                {
                    _logger.LogInformation("✅ Chamado #{ChamadoId} criado SEM vínculo de cliente (apenas dados de contato)",
                        chamado.ChamadoId);
                }

                // Criar primeiro atendimento (registro de abertura)
                //await CreateInitialAtendimentoAsync(chamado.ChamadoId, phoneNumber, contactName, companyName, document, description);

                return chamado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar chamado via WhatsApp para {Phone}", phoneNumber);
                return null;
            }
        }

        /// <summary>
        /// Cria o atendimento inicial do chamado (registro de abertura via WhatsApp)
        /// </summary>
        private async Task CreateInitialAtendimentoAsync(
            long chamadoId,
            string phoneNumber,
            string contactName,
            string? companyName,
            string? document,
            string description)
        {
            try
            {
                var procedimentos = "[ABERTURA VIA WHATSAPP]\n\n";
                procedimentos += $"Requisitante: {contactName}\n";

                if (!string.IsNullOrEmpty(companyName))
                {
                    procedimentos += $"Empresa: {companyName}\n";
                }

                if (!string.IsNullOrEmpty(document))
                {
                    procedimentos += $"CPF/CNPJ: {document}\n";
                }

                procedimentos += $"Telefone: {phoneNumber}\n\n";
                procedimentos += "Descrição:\n";
                procedimentos += description;

                var atendimento = new Atendimentos
                {
                    FkChamadoId = chamadoId,
                    DataAtendimento = DateTime.UtcNow.AddHours(-4),
                    ProcedimentosAplicados = procedimentos,
                    FkTecnicoId = 8, // Bot do WhatsApp (sem técnico)
                    AtendimentoEncerrado = false,
                    NovaDataAtendimento = null,
                    DataFechamento = null
                };

                _context.Atendimentos.Add(atendimento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Atendimento inicial criado para chamado #{ChamadoId}", chamadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar atendimento inicial para chamado #{ChamadoId}", chamadoId);
            }
        }
        #endregion

        #region Sincronização de Mensagens

        /// <summary>
        /// Vincula mensagens do WhatsApp com atendimentos do chamado
        /// Pega todas as mensagens desde a criação da sessão e cria um atendimento
        /// </summary>
        public async Task SyncWhatsAppMessagesToTicketAsync(long chamadoId, string phoneNumber)
        {
            try
            {
                _logger.LogInformation("Sincronizando mensagens WhatsApp com chamado #{ChamadoId}", chamadoId);

                // Buscar mensagens do WhatsApp
                var messages = await _context.MessageHistories
                    .Where(m => m.PhoneNumber == phoneNumber)
                    .OrderBy(m => m.Timestamp)
                    .Take(50) // Limitar a 50 mensagens mais recentes
                    .ToListAsync();

                if (!messages.Any())
                {
                    _logger.LogWarning("Nenhuma mensagem encontrada para {Phone}", phoneNumber);
                    return;
                }

                // Montar histórico de conversa
                var conversaCompleta = "=== HISTÓRICO DA CONVERSA WHATSAPP ===\n\n";

                foreach (var msg in messages)
                {
                    var direcao = msg.Direction == "incoming" ? "👤 Cliente" : "🤖 Bot/Atendente";
                    var timestamp = msg.Timestamp.ToString("dd/MM/yyyy HH:mm:ss");
                    conversaCompleta += $"[{timestamp}] {direcao}:\n{msg.MessageContent}\n\n";
                }

                // Criar atendimento com histórico
                var atendimento = new Atendimentos
                {
                    FkChamadoId = chamadoId,
                    DataAtendimento = DateTime.UtcNow.AddHours(-4),
                    ProcedimentosAplicados = conversaCompleta,
                    FkTecnicoId =  8, // Bot
                    AtendimentoEncerrado = false
                };

                _context.Atendimentos.Add(atendimento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ {Count} mensagens sincronizadas com chamado #{ChamadoId}",
                    messages.Count, chamadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar mensagens para chamado #{ChamadoId}", chamadoId);
            }
        }

        #endregion

        #region Notificações ao Cliente

        /// <summary>
        /// Notifica cliente via WhatsApp que o chamado foi criado
        /// </summary>
        public async Task NotifyTicketCreatedAsync(long chamadoId, string phoneNumber)
        {
            try
            {
                var chamado = await _context.Chamados
                    .Include(c => c.FkSituacaoChamado)
                    .FirstOrDefaultAsync(c => c.ChamadoId == chamadoId);

                if (chamado == null)
                {
                    _logger.LogWarning("Chamado #{ChamadoId} não encontrado para notificação", chamadoId);
                    return;
                }

                var prioridadeIcon = chamado.Prioridade switch
                {
                    "Urgente" => "🔴",
                    "Alta" => "🟠",
                    "Media" => "🟡",
                    _ => "🟢"
                };

                var message = $"✅ *Chamado Aberto com Sucesso!*\n\n" +
                              $"📋 *Protocolo:* #{chamado.ChamadoId}\n" +
                              $"{prioridadeIcon} *Prioridade:* {chamado.Prioridade}\n" +
                              $"📅 *Data:* {chamado.DataCriacao?.ToString("dd/MM/yyyy HH:mm")}\n\n" +
                              $"Em breve um técnico irá atender seu chamado!\n\n" +
                              $"_Você pode acompanhar o status pelo nosso sistema._";

                await _whatsAppService.SendTextMessageAsync(phoneNumber, message);

                _logger.LogInformation("✅ Cliente {Phone} notificado sobre criação do chamado #{ChamadoId}",
                    phoneNumber, chamadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar cliente sobre chamado #{ChamadoId}", chamadoId);
            }
        }

        /// <summary>
        /// Notifica cliente quando técnico assume o chamado
        /// </summary>
        public async Task NotifyTicketAssignedAsync(long chamadoId, string phoneNumber, string technicianName)
        {
            try
            {
                var message = $"👨‍💻 *Chamado #{chamadoId} - Técnico Atribuído*\n\n" +
                              $"O técnico *{technicianName}* foi atribuído ao seu chamado!\n\n" +
                              $"Em breve você receberá um retorno.  🚀";

                await _whatsAppService.SendTextMessageAsync(phoneNumber, message);

                _logger.LogInformation("✅ Cliente {Phone} notificado sobre atribuição do chamado #{ChamadoId} para {Tecnico}",
                    phoneNumber, chamadoId, technicianName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar atribuição do chamado #{ChamadoId}", chamadoId);
            }
        }

        /// <summary>
        /// Notifica cliente quando chamado é finalizado
        /// </summary>
        public async Task NotifyTicketClosedAsync(long chamadoId, string phoneNumber)
        {
            try
            {
                var chamado = await _context.Chamados
                    .Include(c => c.FkTecnico)
                    .FirstOrDefaultAsync(c => c.ChamadoId == chamadoId);

                if (chamado == null)
                {
                    _logger.LogWarning("Chamado #{ChamadoId} não encontrado", chamadoId);
                    return;
                }

                var message = $"✅ *Chamado #{chamadoId} Finalizado*\n\n" +
                              $"Seu chamado foi resolvido e finalizado!\n\n" +
                              $"📅 *Fechamento:* {chamado.DataFechamento?.ToString("dd/MM/yyyy HH:mm")}\n\n" +
                              $"Obrigado por entrar em contato!  😊\n\n" +
                              $"_Se precisar de algo mais, é só chamar!_";

                await _whatsAppService.SendTextMessageAsync(phoneNumber, message);

                _logger.LogInformation("✅ Cliente {Phone} notificado sobre fechamento do chamado #{ChamadoId}",
                    phoneNumber, chamadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar fechamento do chamado #{ChamadoId}", chamadoId);
            }
        }

        #endregion

        #region Busca de Chamados

        /// <summary>
        /// Busca chamados ativos de um cliente por telefone
        /// </summary>
        public async Task<List<Chamados>> GetActiveTicketsByPhoneAsync(string phoneNumber)
        {
            try
            {
                return await _context.Chamados
                    .Include(c => c.FkSituacaoChamado)
                    .Include(c => c.FkTecnico)
                    .Where(c => c.TelefoneContato == phoneNumber &&
                               (c.FkSituacaoChamadoId == 1 || c.FkSituacaoChamadoId == 2)) // Aberto ou Reagendado
                    .OrderByDescending(c => c.DataCriacao)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar chamados ativos para {Phone}", phoneNumber);
                return new List<Chamados>();
            }
        }

        #endregion


        #region Busca de Clientes por Documento

        /// <summary>
        /// Busca cliente por CPF ou CNPJ
        /// Remove formatação e busca apenas números
        /// </summary>
        public async Task<Clientes?> FindClientByDocumentAsync(string document)
        {
            try
            {
                // Remover formatação (.  - / espaços)
                var documentoLimpo = Regex.Replace(document, @"\D", "");

                _logger.LogInformation("Buscando cliente por documento: {Document} (limpo: {Clean})",
                    document, documentoLimpo);

                // Buscar no banco
                var cliente = await _context.Clientes
                    .Where(c => c.Situacao == true) // Apenas ativos
                    .FirstOrDefaultAsync(c =>
                        c.Documento != null &&
                        c.Documento.Replace(".", "").Replace("-", "").Replace("/", "").Replace(" ", "") == documentoLimpo
                    );

                if (cliente != null)
                {
                    _logger.LogInformation("✅ Cliente encontrado por documento: {ClienteId} - {Nome}",
                        cliente.ClienteId, cliente.NomeCliente);
                }
                else
                {
                    _logger.LogWarning("⚠️ Cliente NÃO encontrado para documento: {Document}", documentoLimpo);
                }

                return cliente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cliente por documento: {Document}", document);
                return null;
            }
        }

        /// <summary>
        /// Valida se é CPF ou CNPJ válido (formato básico)
        /// CPF: 11 dígitos, CNPJ: 14 dígitos
        /// </summary>
        public bool IsValidDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return false;

            var documentoLimpo = Regex.Replace(document, @"\D", "");

            // CPF: 11 dígitos, CNPJ: 14 dígitos
            return documentoLimpo.Length == 11 || documentoLimpo.Length == 14;
        }

        #endregion

        #region Notificações WhatsApp para Cliente

        /// <summary>
        /// Verifica se chamado foi criado via WhatsApp
        /// </summary>
        private async Task<bool> IsWhatsAppTicketAsync(long chamadoId, string? telefoneContato)
        {
            try
            {
                // Verificação 1: Telefone deve estar preenchido
                if (string.IsNullOrEmpty(telefoneContato))
                {
                    _logger.LogInformation("Telefone não preenchido para chamado #{ChamadoId}", chamadoId);
                    return false;
                }

                // Verificação 2: Telefone deve estar no formato WhatsApp (55...)
                if (!telefoneContato.StartsWith("55") || telefoneContato.Length < 12)
                {
                    _logger.LogInformation("Telefone {Phone} não está no formato WhatsApp", telefoneContato);
                    return false;
                }

                // Verificação 3: Buscar chamado e verificar se tem marcador [WHATSAPP]
                var chamado = await _context.Chamados
                    .FirstOrDefaultAsync(c => c.ChamadoId == chamadoId);

                if (chamado == null)
                {
                    _logger.LogWarning("Chamado #{ChamadoId} não encontrado", chamadoId);
                    return false;
                }

                if (!chamado.DescricaoCompleta?.Contains("[WHATSAPP") ?? true)
                {
                    _logger.LogInformation("Chamado #{ChamadoId} não tem marcador [WHATSAPP]", chamadoId);
                    return false;
                }

                _logger.LogInformation("✅ Chamado #{ChamadoId} é do WhatsApp", chamadoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se chamado {ChamadoId} é do WhatsApp", chamadoId);
                return false;
            }
        }

        /// <summary>
        /// Notifica cliente via WhatsApp que chamado está sendo atendido
        /// </summary>
        /// <summary>
        /// Notifica cliente via WhatsApp que chamado está sendo atendido
        /// </summary>
        public async Task NotifyTicketInProgressAsync(long chamadoId, string tecnicoNome, string? telefoneContato)
        {
            try
            {
                _logger.LogInformation("🔔 Iniciando notificação de atendimento para chamado #{ChamadoId}", chamadoId);

                // Verificar se é chamado do WhatsApp
                if (!await IsWhatsAppTicketAsync(chamadoId, telefoneContato))
                {
                    _logger.LogInformation("Chamado #{ChamadoId} não é do WhatsApp. Notificação não enviada.", chamadoId);
                    return;
                }

                // Buscar dados do chamado (SEM Include de subcategoria)
                var chamado = await _context.Chamados
                    .FirstOrDefaultAsync(c => c.ChamadoId == chamadoId);

                if (chamado == null)
                {
                    _logger.LogWarning("Chamado #{ChamadoId} não encontrado ao tentar notificar", chamadoId);
                    return;
                }

                var dataHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                // Montar mensagem (SEM categoria/subcategoria)
                var mensagem = $"🔔 *Atendimento Iniciado! *\n\n";
                mensagem += $"Olá *{chamado.Contato}*!\n\n";
                mensagem += $"Seu chamado *#{chamadoId}* está sendo atendido!\n\n";
                mensagem += $"👤 *Técnico:* {tecnicoNome}\n";
                mensagem += $"🕐 *Início:* {dataHora}\n\n";
                mensagem += $"Em breve entraremos em contato para resolver seu problema!\n\n";
                mensagem += $"Agradecemos a compreensão.  😊";

                await _whatsAppService.SendTextMessageAsync(telefoneContato, mensagem);

                _logger.LogInformation("✅ Notificação de ATENDIMENTO enviada para {Phone} - Chamado #{ChamadoId}",
                    telefoneContato, chamadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao notificar cliente sobre atendimento do chamado #{ChamadoId}", chamadoId);
            }
        }
        #endregion

        /// <summary>
        /// Ativa modo atendente (muda State para AgentActive)
        /// Bot para de responder automaticamente
        /// </summary>
        public async Task SetAgentActiveAsync(string phoneNumber)
        {
            try
            {
                var session = await _sessionManager.GetOrCreateSessionAsync(phoneNumber);

                // Mudar para State 2 (AgentActive)
                session.State = 2;
                session.CurrentFlow = null; // Limpar fluxo do bot
                session.FlowData = null;

                await _sessionManager.UpdateSessionAsync(session);

                _logger.LogInformation("✅ Sessão {Phone} alterada para AgentActive (State=2).  Bot não responderá mais.",
                    phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar modo atendente para {Phone}", phoneNumber);
            }
        }

        /// <summary>
        /// Notifica cliente via WhatsApp que chamado foi FINALIZADO
        /// Envia mensagem com informações + botões de avaliação
        /// </summary>
        public async Task NotifyTicketClosedWithFeedbackAsync(long chamadoId, long atendimentoId, string? telefoneContato)
        {
            try
            {
                _logger.LogInformation("🔔 Iniciando notificação de FINALIZAÇÃO para chamado #{ChamadoId}", chamadoId);

                // Verificar se é chamado do WhatsApp
                if (!await IsWhatsAppTicketAsync(chamadoId, telefoneContato))
                {
                    _logger.LogInformation("Chamado #{ChamadoId} não é do WhatsApp.  Notificação não enviada.", chamadoId);
                    return;
                }

                // Buscar dados do chamado e técnico
                var chamado = await _context.Chamados
                    .Include(c => c.FkTecnico)
                    .FirstOrDefaultAsync(c => c.ChamadoId == chamadoId);

                if (chamado == null)
                {
                    _logger.LogWarning("Chamado #{ChamadoId} não encontrado ao tentar notificar finalização", chamadoId);
                    return;
                }

                // Buscar atendimento para pegar informações
                var atendimento = await _context.Atendimentos
                    .Include(a => a.FkTecnico)
                    .FirstOrDefaultAsync(a => a.AtendimentoId == atendimentoId);

                if (atendimento == null)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} não encontrado", atendimentoId);
                    return;
                }

                // Calcular tempo de atendimento
                var tempoAtendimento = "";
                if (atendimento.DataAtendimento.HasValue && atendimento.DataFechamento.HasValue)
                {
                    var duracao = atendimento.DataFechamento.Value - atendimento.DataAtendimento.Value;

                    if (duracao.TotalDays >= 1)
                    {
                        tempoAtendimento = $"{(int)duracao.TotalDays}d {duracao.Hours}h";
                    }
                    else if (duracao.TotalHours >= 1)
                    {
                        tempoAtendimento = $"{(int)duracao.TotalHours}h {duracao.Minutes}min";
                    }
                    else
                    {
                        tempoAtendimento = $"{duracao.Minutes}min";
                    }
                }

                var tecnicoNome = atendimento.FkTecnico?.NomeCompleto ?? "Equipe de Suporte";
                var dataFechamento = chamado.DataFechamento?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                // Montar mensagem
                var mensagem = $"✅ *Chamado #{chamadoId} Finalizado! *\n\n";
                mensagem += $"Seu problema foi resolvido com sucesso!\n\n";
                mensagem += $"👨‍💻 *Técnico:* {tecnicoNome}\n";
                mensagem += $"📅 *Fechamento:* {dataFechamento}\n";

                if (!string.IsNullOrEmpty(tempoAtendimento))
                {
                    mensagem += $"⏱️ *Tempo:* {tempoAtendimento}\n";
                }

                mensagem += $"\n━━━━━━━━━━━━━━━━━━━━\n";
                mensagem += $"⭐ *Avalie nosso atendimento:*\n\n";
                mensagem += $"Sua opinião nos ajuda a melhorar! 😊";

                // Criar botões de avaliação (1 a 5 estrelas)
                var buttons = new List<(string id, string title)>
        {
            ($"rating_{atendimentoId}_1", "⭐ 1"),
            ($"rating_{atendimentoId}_2", "⭐⭐ 2"),
            ($"rating_{atendimentoId}_3", "⭐⭐⭐ 3")
        };

                // Enviar mensagem com primeiros 3 botões
                await _whatsAppService.SendButtonMessageAsync(
                    to: telefoneContato,
                    bodyText: mensagem,
                    buttons: buttons,
                    footerText: "HelpDesk SysoTecnologia"
                );

                // WhatsApp só permite 3 botões por mensagem, então enviamos os outros 2 separadamente
                await Task.Delay(500); // Pequeno delay entre mensagens

                var buttonsExtra = new List<(string id, string title)>
        {
            ($"rating_{atendimentoId}_4", "⭐⭐⭐⭐ 4"),
            ($"rating_{atendimentoId}_5", "⭐⭐⭐⭐⭐ 5")
        };

                await _whatsAppService.SendButtonMessageAsync(
                    to: telefoneContato,
                    bodyText: "Ou escolha uma dessas opções:",
                    buttons: buttonsExtra,
                    footerText: "Clique em uma das opções acima"
                );

                // Salvar mensagem no histórico
                await _sessionManager.SaveMessageAsync(
                    phoneNumber: telefoneContato,
                    direction: "outgoing",
                    messageType: "interactive",
                    content: $"Notificação de finalização do chamado #{chamadoId} com pedido de avaliação",
                    sentBy: "bot"
                );

                _logger.LogInformation("✅ Notificação de FINALIZAÇÃO enviada para {Phone} - Chamado #{ChamadoId}",
                    telefoneContato, chamadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao notificar cliente sobre finalização do chamado #{ChamadoId}", chamadoId);
            }
        }

        #region Avaliação de Atendimento

        /// <summary>
        /// Busca um atendimento específico por ID
        /// </summary>
        public async Task<Atendimentos?> GetAtendimentoByIdAsync(long atendimentoId)
        {
            try
            {
                var atendimento = await _context.Atendimentos
                    .Include(a => a.FkTecnico)
                    .Include(a => a.FkChamado)
                    .FirstOrDefaultAsync(a => a.AtendimentoId == atendimentoId);

                if (atendimento == null)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} não encontrado", atendimentoId);
                }

                return atendimento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atendimento #{AtendimentoId}", atendimentoId);
                return null;
            }
        }

        /// <summary>
        /// Salva avaliação (nota) no atendimento
        /// </summary>
        public async Task<bool> SaveRatingAsync(long atendimentoId, int nota)
        {
            try
            {
                if (nota < 1 || nota > 5)
                {
                    _logger.LogError("Nota inválida: {Nota}. Deve ser entre 1 e 5", nota);
                    return false;
                }

                var atendimento = await _context.Atendimentos
                    .FirstOrDefaultAsync(a => a.AtendimentoId == atendimentoId);

                if (atendimento == null)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} não encontrado para salvar avaliação", atendimentoId);
                    return false;
                }

                // Verificar se já foi avaliado
                if (atendimento.AvaliacaoNota.HasValue)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} já possui avaliação: {NotaExistente}",
                        atendimentoId, atendimento.AvaliacaoNota.Value);
                    return false;
                }

                // Salvar nota e data
                atendimento.AvaliacaoNota = nota;
                atendimento.AvaliacaoData = DateTime.UtcNow.AddHours(-4);

                _context.Atendimentos.Update(atendimento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Avaliação salva com sucesso - Atendimento #{AtendimentoId}, Nota: {Nota}",
                    atendimentoId, nota);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar avaliação do atendimento #{AtendimentoId}", atendimentoId);
                return false;
            }
        }


        /// <summary>
        /// Salva comentário da avaliação no atendimento
        /// </summary>
        public async Task<bool> SaveRatingCommentAsync(long atendimentoId, string comentario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(comentario))
                {
                    _logger.LogWarning("Comentário vazio para atendimento #{AtendimentoId}", atendimentoId);
                    return false;
                }

                var atendimento = await _context.Atendimentos
                    .FirstOrDefaultAsync(a => a.AtendimentoId == atendimentoId);

                if (atendimento == null)
                {
                    _logger.LogWarning("Atendimento #{AtendimentoId} não encontrado para salvar comentário", atendimentoId);
                    return false;
                }

                // Limitar tamanho do comentário (máximo 500 caracteres)
                var comentarioLimitado = comentario.Length > 500
                    ? comentario.Substring(0, 500)
                    : comentario;

                // Salvar comentário
                atendimento.AvaliacaoComentario = comentarioLimitado;

                // Se ainda não tem data de avaliação, adiciona agora
                if (!atendimento.AvaliacaoData.HasValue)
                {
                    atendimento.AvaliacaoData = DateTime.UtcNow.AddHours(-4);
                }

                _context.Atendimentos.Update(atendimento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Comentário da avaliação salvo - Atendimento #{AtendimentoId}, Tamanho: {Tamanho} caracteres",
                    atendimentoId, comentarioLimitado.Length);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar comentário da avaliação #{AtendimentoId}", atendimentoId);
                return false;
            }
        }

        #endregion


    }
}
