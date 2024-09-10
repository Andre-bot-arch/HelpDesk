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
        public IActionResult Aberto()
        {
            return View(_context.Chamados.Include(a => a.FkAtendenteNavigation)
                                         .Include(a => a.FkCliente)
                                         .Include(a => a.FkTecnico)
                                         .Include(a => a.Atendimentos)
                                         .ThenInclude(a=> a.FkTecnico)
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
            var atendimento = new Atendimentos
            {
                DataAtendimento = DateTime.Now,
                FkChamadoId = dados.ChamadoId,
                ProcedimentosAplicados = dados.DescricaoCompleta,
                FkTecnicoId = Convert.ToInt32(userId),
                NovaDataAtendimento = dados.DataAgendamento,
                AtendimentoEncerrado = false,                
            };
            _generico.GravarGenerico(atendimento);

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
            };
            _generico.GravarGenerico(atendimento);

            chamado.FkSituacaoChamadoId = 4;           

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
            return PartialView("_ModalPrintChamado", chamado);
        }
    }
}
