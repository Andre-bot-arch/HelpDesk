using AppSysoHelp.Models;
using AppSysoHelp.Service;
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

        public ChamadoController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _generico = new ServiceGenerico(context);
        }

        public async Task<IActionResult> Atendimento(int id)
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
            var chamado = _context.Chamados.Include(a => a.FkAtendenteNavigation)
                                         .Include(a => a.FkCliente)
                                         .Include(a => a.FkTecnico)
                                         .Include(a => a.Atendimentos)
                                         .ThenInclude(a => a.FkTecnico)
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
        public IActionResult FinalizarChamado(Chamados d, IFormFile imagem, bool finalizar = true, TimeOnly? Inicio = null, TimeOnly? Fim = null)
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

            // Carregar o chamado e atendimentos de uma vez (sem await para evitar concorrência)
            var chamado = _context.Chamados
                .Include(a => a.FkSituacaoChamado)
                .Include(a => a.Atendimentos) // Sem filtro no Include para evitar problemas assíncronos
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
                    : d.DescricaoCompleta + "  ATENÇÃO!! FECHAMENTO FORÇADO!!!";
                atendimento.FkTecnicoId = userId;
                atendimento.NovaDataAtendimento = d.DataAgendamento;
                atendimento.DataFechamento = DateTime.UtcNow.AddHours(-4);
                atendimento.AtendimentoEncerrado = true;
                atendimento.CaminhoDaImagem = caminhoImagem;
                atendimento.Inicio = Inicio;
                atendimento.Fim = Fim;
            }

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
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
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
    }
}
