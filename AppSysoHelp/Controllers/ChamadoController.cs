using AppSysoHelp.Models;
using AppSysoHelp.Service;
using AppSysoHelp.Service.WhatsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class ChamadoController : Controller
    {
        private readonly ServiceGenerico _generico;
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly HelpDeskIntegrationService _helpDeskService;
        private readonly ILogger<ChamadoController> _logger;

        public ChamadoController(IConfiguration configuration, HelpdesksysoContext context, HelpDeskIntegrationService helpDeskService, ILogger<ChamadoController> logger)
        {
            _configuration = configuration;
            _context = context;
            _generico = new ServiceGenerico(context);
            _helpDeskService = helpDeskService;
            _logger = logger;
        }

        public async Task<IActionResult> Atendimento(int id)
        {
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var atendimento = _context.Atendimentos
                .Include(a => a.FkChamado)
                .FirstOrDefault(a => a.FkChamadoId == id &&
                                     a.AtendimentoEncerrado == false &&
                                     a.FkChamado.FkSituacaoChamadoId != 3);

            if (atendimento == null)
            {
                ViewBag.atendimento = false;
                atendimento = new Atendimentos
                {
                    DataAtendimento = DateTime.UtcNow.AddHours(-4),
                    FkChamadoId = id,
                    ProcedimentosAplicados = "EM ATENDIMENTO",
                    FkTecnicoId = Convert.ToInt32(userId),
                    AtendimentoEncerrado = false,
                };
                await _generico.GravarGenericoAsync(atendimento);

                // 🔔 NOTIFICAR CLIENTE VIA WHATSAPP
                var idChamado = Convert.ToInt64(id);
                var chamado = _context.Chamados.Find(idChamado);
                var tecnico = _context.TecnicosSupervisores.Find(Convert.ToInt64(userId));

                if (chamado != null && tecnico != null)
                {
                    _logger.LogInformation("Tentando notificar cliente do chamado #{ChamadoId}", idChamado);

                    await _helpDeskService.NotifyTicketInProgressAsync(
                        chamadoId: idChamado,
                        tecnicoNome: tecnico.NomeCompleto,
                        telefoneContato: chamado.TelefoneContato
                    );

                    // 🤖 MUDAR STATE PARA AgentActive (bot para de responder)
                    if (!string.IsNullOrEmpty(chamado.TelefoneContato))
                    {
                        await _helpDeskService.SetAgentActiveAsync(chamado.TelefoneContato);
                    }

                }
            }
            else if (atendimento != null && atendimento.FkTecnicoId == Convert.ToInt32(userId))
            {
                ViewBag.atendimento = false;
            }
            else
            {
                ViewBag.atendimento = true;
            }

            var chamadoCompleto = _context.Chamados
                .Include(a => a.FkAtendenteNavigation)
                .Include(a => a.FkCliente)
                .Include(a => a.FkTecnico)
                .Include(a => a.Atendimentos)
                .ThenInclude(a => a.FkTecnico)
                .FirstOrDefault(a => a.ChamadoId == id);

            chamadoCompleto.DataAgendamento = DateTime.UtcNow.AddHours(-4);
            _context.Update(chamadoCompleto);
            _context.SaveChanges();

            return View(chamadoCompleto);
        }

        public async Task<IActionResult> AtendimentoTreinamento(int id)
        {
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var atendimento = _context.Atendimentos.FirstOrDefault(a => a.FkChamadoId == id && a.AtendimentoEncerrado == false);

            if (atendimento == null)
            {
                ViewBag.atendimento = false;
                atendimento = new Atendimentos
                {
                    DataAtendimento = DateTime.UtcNow.AddHours(-4),
                    FkChamadoId = id,
                    ProcedimentosAplicados = "EM ATENDIMENTO",
                    FkTecnicoId = Convert.ToInt32(userId),
                    AtendimentoEncerrado = false,
                };
                await _generico.GravarGenericoAsync(atendimento);
            }
            else if (atendimento != null && atendimento.FkTecnicoId == Convert.ToInt32(userId))
            {
                ViewBag.atendimento = false;
            }
            else
            {
                ViewBag.atendimento = true;
            }
            var chamado = _context.Chamados.Include(a => a.FkAtendenteNavigation)
                                         .Include(a => a.FkCliente)
                                         .Include(a => a.FkTecnico)
                                         .Include(a => a.Atendimentos)
                                         .ThenInclude(a => a.FkTecnico)
                                         .FirstOrDefault(a => a.ChamadoId == id);
            chamado.DataAgendamento = DateTime.UtcNow.AddHours(-4);
            _context.Update(chamado);
            _context.SaveChanges();
            return View(chamado);
        }


        public IActionResult Aberto()
        {
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var atendimento = _context.Atendimentos.Where(a => a.FkTecnicoId == Convert.ToInt32(userId) && a.AtendimentoEncerrado == false).ToList();

            if (atendimento.Count > 0)
            {
                ViewBag.emAtendimento = true;
            }
            else
            {
                ViewBag.emAtendimento = false;
            }

            ViewBag.categoria = _context.ChamadosCategoria.OrderBy(a => a.Descricao).ToList();
            return View(_context.Chamados.Include(a => a.FkAtendenteNavigation)
                                         .Include(a => a.FkCliente)
                                         .Include(a => a.FkTecnico)
                                         .Include(a => a.Atendimentos)
                                         .ThenInclude(a => a.FkTecnico)
                                         .Where(a => a.DataFechamento == null || a.DataFechamento.Value.Date >= DateTime.Now.Date)
                                         .ToList());
        }

        [HttpPost]
        public IActionResult GravarChamado(Chamados m)
        {

            if (m.FkSubCategoria > 0 )
            {
                var userIdClaim = User.FindFirst("Id");
                var userId = userIdClaim?.Value;

                m.FkSituacaoChamadoId = 1;
                m.DataCriacao = DateTime.UtcNow.AddHours(-4);
                m.FkAtendente = Convert.ToInt32(userId);
                if (m.DataAgendamento == null)
                {
                    m.DataAgendamento = DateTime.UtcNow.AddHours(-4);
                }

                _generico.GravarGenerico(m);

                return Ok();
            }
            return BadRequest(new { message = "Subcategoria inválida." });

        }

        [HttpPost]
        public IActionResult RemarcarChamado(Chamados dados)
        {
            var chamado = _context.Chamados.Include(a=> a.FkSituacaoChamado).FirstOrDefault(a => a.ChamadoId == dados.ChamadoId);
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var atendimentoExistente = _context.Atendimentos.FirstOrDefault(a => a.FkChamadoId == dados.ChamadoId && a.AtendimentoEncerrado != true);
            atendimentoExistente.ProcedimentosAplicados = dados.DescricaoCompleta;
            atendimentoExistente.FkTecnicoId = Convert.ToInt32(userId);
            atendimentoExistente.NovaDataAtendimento = dados.DataAgendamento;
            atendimentoExistente.DataFechamento = DateTime.UtcNow.AddHours(-4);
            atendimentoExistente.AtendimentoEncerrado = true;

            if (chamado!.FkSituacaoChamado.SituacaoChamadoId != 3)
            {
                _generico.UpdateGenerico(atendimentoExistente);

                chamado.FkSituacaoChamadoId = 2;
                chamado.DataAgendamento = dados.DataAgendamento;
                chamado.FkTecnicoId = dados.FkTecnicoId;
                chamado.Prioridade = dados.Prioridade;

                _generico.UpdateGenerico(chamado);
            }
            return RedirectToAction("Aberto");
        }

        [HttpPost]
        public IActionResult CancelarChamado(Chamados dados)
        {
            var chamado = _context.Chamados.FirstOrDefault(a => a.ChamadoId == dados.ChamadoId);
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var atendimento = new Atendimentos
            {
                DataAtendimento = DateTime.UtcNow.AddHours(-4),
                FkChamadoId = dados.ChamadoId,
                ProcedimentosAplicados = dados.DescricaoCompleta,
                FkTecnicoId = Convert.ToInt32(userId),
                NovaDataAtendimento = DateTime.UtcNow.AddHours(-4),
                AtendimentoEncerrado = true,
                DataFechamento = DateTime.UtcNow.AddHours(-4)
            };
            _generico.GravarGenerico(atendimento);

            chamado.FkSituacaoChamadoId = 4;
            chamado.DataFechamento = DateTime.UtcNow.AddHours(-4);

            _generico.UpdateGenerico(chamado);

            return RedirectToAction("Aberto");
        }

        [HttpPost]
        public IActionResult BuscarDetalhes(long id)
        {
             var chamado = _context.Chamados
                .Include(a => a.FkAtendenteNavigation)
                .Include(a => a.FkCliente)
                .Include(a => a.FkTecnico)
                .Include(a => a.FkSubCategoriaNavigation)
                    .ThenInclude(sc => sc!.FkCategoriaNavigation)
                .Include(a => a.Atendimentos)
                    .ThenInclude(at => at.FkTecnico)
                .FirstOrDefault(a => a.ChamadoId == id);

               return PartialView("_DetalhesDoAtendimento", chamado);
        }

        [HttpPost]
        public IActionResult BuscarDetalhes2(long id)
        {
            var chamado = _context.Chamados.Include(a => a.FkAtendenteNavigation)
                                         .Include(a => a.FkCliente)
                                         .Include(a => a.FkTecnico)
                                         .Include(a => a.Atendimentos)
                                         .ThenInclude(a => a.FkTecnico)
                                         .FirstOrDefault(a => a.ChamadoId == id);
            return PartialView("_DetalhesDoAtendimento2", chamado);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarChamado(Chamados d, IFormFile imagem, bool finalizar = true, TimeOnly? Inicio = null, TimeOnly? Fim = null)
        {
            var caminhoImagem = string.Empty;
            if (imagem is { Length: > 0 })
            {
                string fileExtension = Path.GetExtension(imagem.FileName).ToLower();
                string fileName = $"chamado-{d.ChamadoId}{fileExtension}";
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens_chamado", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                using (var fileStream = new FileStream(savePath, FileMode.Create))
                {
                    imagem.CopyTo(fileStream);
                }
                caminhoImagem = $"imagens_chamado/{fileName}";
            }

            var chamado = _context.Chamados
                .Include(a => a.FkSituacaoChamado)
                .Include(a => a.Atendimentos)
                .FirstOrDefault(a => a.ChamadoId == d.ChamadoId && a.FkSituacaoChamadoId != 3);

            if (chamado == null)
                return RedirectToAction("Aberto");

            var userId = Convert.ToInt64(User.FindFirst("Id")?.Value);

            // Filtra manualmente apenas os atendimentos não encerrados para evitar múltiplos acessos ao contexto
            var atendimentos = chamado.Atendimentos.Where(at => !at.AtendimentoEncerrado).ToList();

            foreach (var atendimento in atendimentos)
            {
                atendimento.ProcedimentosAplicados = (atendimento.FkTecnicoId == userId)
                    ? d.DescricaoCompleta
                    : d.DescricaoCompleta + "  ATENÇÃO!!  FECHAMENTO FORÇADO!!! ";
                atendimento.FkTecnicoId = userId;
                atendimento.NovaDataAtendimento = d.DataAgendamento;
                atendimento.DataFechamento = DateTime.UtcNow.AddHours(-4);
                atendimento.AtendimentoEncerrado = true;
                atendimento.CaminhoDaImagem = caminhoImagem;
                atendimento.Inicio = Inicio;
                atendimento.Fim = Fim;
            }

            // ========================================
            // 🆕 VARIÁVEL PARA ARMAZENAR ID DO ÚLTIMO ATENDIMENTO
            // ========================================
            long? ultimoAtendimentoId = null;

            // Atualizando em uma única transação
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Atendimentos.UpdateRange(atendimentos);
                    chamado.FkSituacaoChamadoId = 3;
                    chamado.DataFechamento = DateTime.UtcNow.AddHours(-4);
                    _context.Chamados.Update(chamado);
                    _context.SaveChanges();
                    transaction.Commit();

                    // ========================================
                    // 🆕 PEGAR ID DO ÚLTIMO ATENDIMENTO ENCERRADO
                    // ========================================
                    ultimoAtendimentoId = atendimentos.LastOrDefault()?.AtendimentoId;

                    _logger.LogInformation("✅ Chamado #{ChamadoId} finalizado com sucesso.  Último atendimento: #{AtendimentoId}",
                        chamado.ChamadoId, ultimoAtendimentoId);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            // ========================================
            // 🆕 NOTIFICAR CLIENTE VIA WHATSAPP
            // ========================================
            if (ultimoAtendimentoId.HasValue && !string.IsNullOrEmpty(chamado.TelefoneContato))
            {
                try
                {
                    _logger.LogInformation("🔔 Enviando notificação de finalização para chamado #{ChamadoId}", chamado.ChamadoId);

                    // Enviar notificação com pedido de avaliação
                    await _helpDeskService.NotifyTicketClosedWithFeedbackAsync(
                        chamadoId: chamado.ChamadoId,
                        atendimentoId: ultimoAtendimentoId.Value,
                        telefoneContato: chamado.TelefoneContato
                    );

                    // Reativar bot (volta State para 0 - BotActive)
                    var session = await _context.CustomerSessions
                        .FirstOrDefaultAsync(s => s.PhoneNumber == chamado.TelefoneContato);

                    if (session != null)
                    {
                        session.State = 0; // BotActive
                        session.CurrentFlow = null; // Limpa fluxo
                        session.FlowData = null; // Limpa dados temporários
                        session.LinkedTicketId = null; // Desvincula chamado
                        session.LastInteraction = DateTime.UtcNow;

                        _context.CustomerSessions.Update(session);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("✅ Bot reativado para {Phone}. State = 0 (BotActive)", chamado.TelefoneContato);
                    }
                }
                catch (Exception ex)
                {
                    // Mesmo se falhar a notificação, chamado já foi finalizado
                    _logger.LogError(ex, "❌ Erro ao notificar cliente do chamado #{ChamadoId}.  Chamado finalizado, mas notificação falhou.",
                        chamado.ChamadoId);
                }
            }
            else
            {
                _logger.LogInformation("ℹ️ Chamado #{ChamadoId} finalizado, mas não enviará notificação WhatsApp (sem telefone ou atendimento)",
                    chamado.ChamadoId);
            }

            return RedirectToAction("Aberto");
        }
        public IActionResult FinalizarChamadoTreinamento(Chamados d, IFormFile imagem, bool finalizar = false, TimeOnly? Inicio = null, TimeOnly? Fim = null)
        {
            string caminhoImagem = SalvarImagem(imagem, d.ChamadoId);

            Inicio ??= TimeOnly.Parse("00:00:00");
            Fim ??= TimeOnly.Parse("00:00:00");

            var chamado = _context.Chamados
                .Include(a => a.FkSituacaoChamado)
                .FirstOrDefault(a => a.ChamadoId == d.ChamadoId && a.FkSituacaoChamadoId != 3);

            if (chamado == null) return RedirectToAction("Aberto");

            var userId = Convert.ToInt64(User.FindFirst("Id")?.Value);
            var atendimentos = _context.Atendimentos
                .Where(a => a.FkChamadoId == d.ChamadoId && !a.AtendimentoEncerrado)
                .ToList();

            AtualizarAtendimentos(atendimentos, d, userId, caminhoImagem, Inicio, Fim);
            _generico.UpdateGenericoRanger(atendimentos);

            if (!finalizar)
            {
                chamado.FkSituacaoChamadoId = 2;
                chamado.DataAgendamento = d.DataAgendamento;
            }
            else
            {
                chamado.FkSituacaoChamadoId = 3;
                chamado.DataFechamento = DateTime.UtcNow.AddHours(-4);
            }

            _generico.UpdateGenerico(chamado);
            return RedirectToAction("Aberto");
        }

        private string SalvarImagem(IFormFile imagem, long chamadoId)
        {
            if (imagem == null || imagem.Length == 0) return string.Empty;

            string fileExtension = Path.GetExtension(imagem.FileName).ToLower();
            string fileName = $"chamado-{chamadoId}{fileExtension}";
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens_chamado", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                imagem.CopyTo(fileStream);
            }

            return $"imagens_chamado/{fileName}";
        }

        private void AtualizarAtendimentos(List<Atendimentos> atendimentos, Chamados d, long userId, string caminhoImagem, TimeOnly? Inicio, TimeOnly? Fim)
        {
            foreach (var item in atendimentos)
            {
                item.ProcedimentosAplicados = (item.FkTecnicoId == userId) ? d.DescricaoCompleta : d.DescricaoCompleta + "  ATENÇÃO!! FECHAMENTO FORÇADO!!!";
                item.FkTecnicoId = userId;
                item.NovaDataAtendimento = d.DataAgendamento;
                item.DataFechamento = DateTime.UtcNow.AddHours(-4);
                item.AtendimentoEncerrado = true;
                item.CaminhoDaImagem = caminhoImagem;
                item.Inicio = Inicio;
                item.Fim = Fim;
            }
        }

        public async Task<IActionResult> AdminChamados()
        {
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var usuario = _context.TecnicosSupervisores.Find(Convert.ToInt64(userId));

            if (usuario.CargoResponsabilidade != "Admin")
            {
                TempData["ErrorMessage"] = "Acesso negado! Apenas administradores podem acessar.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.usuario = usuario;

            return View();
        }

        [HttpPost]
        public IActionResult Finalizar(long id)
        {

            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var usuarioTecnico = _context.TecnicosSupervisores.Find(Convert.ToInt64(userId));

            var Atendimento = _context.Atendimentos.Find(id);

            if (Atendimento != null)
            {
                Atendimento.AtendimentoEncerrado = true;
                Atendimento.ProcedimentosAplicados = $"FINALIZADO PELO ADMINISTRADOR {usuarioTecnico.NomeCompleto}";
                Atendimento.DataFechamento = DateTime.Now;
                _context.SaveChanges();
            }

            return Ok();
        }
    }
}
