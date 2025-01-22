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
        public IActionResult FinalizarChamado(Chamados d, IFormFile imagem, bool finalizar = false, TimeOnly? Inicio = null, TimeOnly? Fim = null)
        {
            if (finalizar == false)
            {
                Inicio ??= TimeOnly.Parse("00:00:00");
                Fim ??= TimeOnly.Parse("00:00:00");

                var _chamado = _context.Chamados.Include(a => a.FkSituacaoChamado).FirstOrDefault(a => a.ChamadoId == d.ChamadoId);
                var _userIdClaim = User.FindFirst("Id");
                var _userId = _userIdClaim?.Value;
                var _atendimentoExistente = _context.Atendimentos.Where(a => a.FkChamadoId == d.ChamadoId && a.AtendimentoEncerrado != true).ToList();
                foreach (var item in _atendimentoExistente)
                {
                    item.ProcedimentosAplicados = (item.FkTecnicoId == Convert.ToInt64(_userId))? d.DescricaoCompleta : "FECHAMENTO FORÇADO!!!";
                    item.FkTecnicoId = Convert.ToInt32(_userId);
                    item.NovaDataAtendimento = d.DataAgendamento;
                    item.DataFechamento = DateTime.UtcNow.AddHours(-4);
                    item.AtendimentoEncerrado = true;
                    item.Inicio = (item.FkTecnicoId == Convert.ToInt64(_userId)) ? Inicio :TimeOnly.Parse("00:00:00");
                    item.Fim = (item.FkTecnicoId == Convert.ToInt64(_userId)) ? Fim : TimeOnly.Parse("00:00:00");
                }

                if (_chamado!.FkSituacaoChamado.SituacaoChamadoId != 3)
                {
                    _generico.UpdateGenericoRanger(_atendimentoExistente);

                    _chamado.FkSituacaoChamadoId = 2;
                    _chamado.DataAgendamento = d.DataAgendamento;

                    _generico.UpdateGenerico(_chamado);
                }
                return RedirectToAction("Aberto");
            }

            var chamado = _context.Chamados.FirstOrDefault(a => a.ChamadoId == d.ChamadoId);
            var userIdClaim = User.FindFirst("Id").Value;
            var caminhoImagem = "";
            if (imagem != null && imagem.Length > 0)
            {
                string fileExtension = Path.GetExtension(imagem.FileName);
                string fileName = $"chamado-{d.ChamadoId}{fileExtension.ToLower()}";
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens_chamado", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                using (var fileStream = new FileStream(savePath, FileMode.Create))
                {
                    imagem.CopyTo(fileStream);
                }
                caminhoImagem = $"imagens_chamado/{fileName}";
            }

            var atendimentoExistente = _context.Atendimentos.FirstOrDefault(a => a.FkChamadoId == d.ChamadoId && a.AtendimentoEncerrado != true);
            atendimentoExistente.ProcedimentosAplicados = d.DescricaoCompleta;
            atendimentoExistente.FkTecnicoId = Convert.ToInt32(userIdClaim);
            atendimentoExistente.DataFechamento = DateTime.UtcNow.AddHours(-4);
            atendimentoExistente.AtendimentoEncerrado = true;
            atendimentoExistente.CaminhoDaImagem = caminhoImagem;

            _generico.UpdateGenerico(atendimentoExistente);

            chamado.FkSituacaoChamadoId = 3;
            chamado.DataFechamento = DateTime.UtcNow.AddHours(-4);

            _generico.UpdateGenerico(chamado);
            return RedirectToAction("Aberto");
        }
    }
}
