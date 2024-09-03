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
    }
}
