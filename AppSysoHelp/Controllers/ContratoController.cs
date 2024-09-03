using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class ContratoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContrato _contrato;

        public ContratoController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _contrato = new ServiceContrato(context);
        }

        public IActionResult Index()
        {
            var lista = _contrato.BuscarContratos();
            return View(lista);
        }

      
        public JsonResult GetSuggestions(string query)
        {
            var suggestions = _context.Clientes
                                .Where(e => e.NomeCliente.Contains(query))
                                .Select(e => new
                                {
                                    value = e.ClienteId, 
                                    label = e.NomeCliente
                                })
                                .ToList();

            return Json(suggestions);
        }

      
        public JsonResult GetDetailedResults(long query)
        {
            if (query == 0)
            {
                return Json(new { error = "Nenhum resultado encontrado." });
            }

            var results = _context.Clientes
                            .Where(e => e.ClienteId == query)
                            .ToList();
            
            var htmlResults = results.Select(e => $"<p>{e.NomeCliente}</p>").ToArray();

            return Json(new { html = string.Join("", htmlResults) });
        }
    }
}
