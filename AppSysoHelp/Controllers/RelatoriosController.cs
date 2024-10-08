using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly HelpdesksysoContext _context;

        public RelatoriosController(HelpdesksysoContext context)
        {
            _context = context;
        }
        public IActionResult Index(int id)
        {
            ViewBag.tipo = id;
            return View();
        } 

        public IActionResult ChamadosPorPeriodo(string? tecnico, string? cliente, DateTime dataInicio, DateTime dataFinal)
        {
            var chamadosFiltrados = _context.Chamados.Include(a => a.FkSituacaoChamado).Include(a => a.FkCliente).AsQueryable();

            chamadosFiltrados = chamadosFiltrados.Where(a => a.DataCriacao >= dataInicio && a.DataFechamento <= dataFinal);

            if (!string.IsNullOrEmpty(tecnico))
                chamadosFiltrados = chamadosFiltrados.Where(a => a.FkTecnico.PkId == Convert.ToInt64(tecnico));

            if (!string.IsNullOrEmpty(cliente))
                chamadosFiltrados = chamadosFiltrados.Where(a => a.FkCliente.ClienteId == Convert.ToInt64(cliente));

            var resultados = chamadosFiltrados.ToList();
            ViewBag.DataInicio = dataInicio.ToShortDateString(); ViewBag.DataFinal = dataFinal.ToShortDateString();
            ViewBag.TotalChamados = resultados.Count();
            return View(resultados);
        }
    }
}
