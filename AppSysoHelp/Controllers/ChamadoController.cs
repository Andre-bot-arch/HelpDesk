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
                    DataAtendimento = DateTime.Now,
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
            return View(chamado);
        }

        public IActionResult Aberto()
        {
            return View(_context.Chamados.Include(a => a.FkAtendenteNavigation)
                                         .Include(a => a.FkCliente)
                                         .Include(a => a.FkTecnico)
                                         .Include(a => a.Atendimentos)
                                         .ThenInclude(a => a.FkTecnico)
                                         .ToList());
        }

        [HttpPost]
        public IActionResult GravarChamado(Chamados m)
        {
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;

            m.FkSituacaoChamadoId = 1;
            m.DataCriacao = DateTime.Now;
            m.FkAtendente = Convert.ToInt32(userId);
            if (m.DataAgendamento == null)
            {
                m.DataAgendamento = DateTime.Now;
            }

            _generico.GravarGenerico(m);

            return RedirectToAction("Aberto");
        }

        [HttpPost]
        public IActionResult RemarcarChamado(Chamados dados)
        {
            var chamado = _context.Chamados.FirstOrDefault(a => a.ChamadoId == dados.ChamadoId);
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;
            var atendimentoExistente = _context.Atendimentos.FirstOrDefault(a => a.FkChamadoId == dados.ChamadoId && a.AtendimentoEncerrado != true);
            atendimentoExistente.ProcedimentosAplicados = dados.DescricaoCompleta;
            atendimentoExistente.FkTecnicoId = Convert.ToInt32(userId);
            atendimentoExistente.NovaDataAtendimento = dados.DataAgendamento;
            atendimentoExistente.DataFechamento = DateTime.Now;
            atendimentoExistente.AtendimentoEncerrado = true;

            _generico.UpdateGenerico(atendimentoExistente);

            chamado.FkSituacaoChamadoId = 2;
            chamado.DataAgendamento = dados.DataAgendamento;
            chamado.FkTecnicoId = dados.FkTecnicoId;
            chamado.Prioridade = dados.Prioridade;

            _generico.UpdateGenerico(chamado);

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
                DataAtendimento = DateTime.Now,
                FkChamadoId = dados.ChamadoId,
                ProcedimentosAplicados = dados.DescricaoCompleta,
                FkTecnicoId = Convert.ToInt32(userId),
                NovaDataAtendimento = DateTime.Now,
                AtendimentoEncerrado = true,
                DataFechamento = DateTime.Now
            };
            _generico.GravarGenerico(atendimento);

            chamado.FkSituacaoChamadoId = 4;
            chamado.DataFechamento = DateTime.Now;

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
        public IActionResult FinalizarChamado(Chamados d, IFormFile imagem)
        {
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
                caminhoImagem = $"/imagens_chamado/{fileName}";
            }

            var atendimentoExistente = _context.Atendimentos.FirstOrDefault(a => a.FkChamadoId == d.ChamadoId && a.AtendimentoEncerrado != true);
            atendimentoExistente.ProcedimentosAplicados = d.DescricaoCompleta;
            atendimentoExistente.FkTecnicoId = Convert.ToInt32(userIdClaim);
            atendimentoExistente.DataFechamento = DateTime.Now;
            atendimentoExistente.AtendimentoEncerrado = true;
            atendimentoExistente.CaminhoDaImagem = caminhoImagem;

            _generico.UpdateGenerico(atendimentoExistente);

            chamado.FkSituacaoChamadoId = 3;
            chamado.DataFechamento = DateTime.Now;

            _generico.UpdateGenerico(chamado);
            return RedirectToAction("Aberto");
        }
    }
}
