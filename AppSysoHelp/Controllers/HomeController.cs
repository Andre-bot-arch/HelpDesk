using AppSysoHelp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;

        public HomeController(ILogger<HomeController> logger, HelpdesksysoContext context, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {

            // Obtém a lista de chamados nos últimos 6 meses
            var lista = _context.Chamados
                .Where(a => a.DataCriacao > DateTime.Now.AddMonths(-6).Date)
                .ToList();
            
            var chamados = _context.TotalizadorChamadosPorTecnico.ToList();

            // Passa as informações para a View
            ViewBag.tecnicosAtendimento = chamados;

            return View(lista);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult AtendimentoRanking()
        {
            var seisMesesAtras = DateTime.Now.AddMonths(-6);

            // Busca os chamados com a lista de atendimentos
            var chamados = _context.Chamados
                .Include(c => c.Atendimentos)
                .ThenInclude(c=> c.FkTecnico)
                .Where(c => c.DataFechamento != null && c.Atendimentos.Any(a => a.DataFechamento.HasValue && a.DataFechamento.Value >= seisMesesAtras))
                .ToList();

            // Obtemos o último atendimento para cada chamado
            var ultimoAtendimentoPorChamado = chamados
                .Select(c => c.Atendimentos
                    .Where(a => a.DataFechamento.HasValue && a.DataFechamento.Value >= seisMesesAtras && a.AtendimentoEncerrado == true)
                    .OrderByDescending(a => a.DataFechamento)
                    .FirstOrDefault()
                )
                .Where(a => a != null)
                .ToList();

            // Contamos os atendimentos por técnico e mês
            var ranking = ultimoAtendimentoPorChamado
                .GroupBy(a => new { a.FkTecnico.NomeCompleto, MêsAno = a.DataFechamento.Value.ToString("yyyy-MM") })
                .Select(g => new
                {
                    Tecnico = g.Key.NomeCompleto ?? "Desconhecido",
                    MêsAno = g.Key.MêsAno,
                    Quantidade = g.Count()
                })
                .ToList();

            return Json(ranking);
        }


    }
}
