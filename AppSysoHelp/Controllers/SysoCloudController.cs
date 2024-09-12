using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers
{
    public class SysoCloudController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContrato _contrato;
        private readonly ServiceGenerico _generico;

        public SysoCloudController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _contrato = new ServiceContrato(context);
            _generico = new ServiceGenerico(context);
        }
        public IActionResult Index()
        {
            return View(_contrato.BuscarContratos().Where(a => a.FkPlataforma.NomePlataforma.Contains("Syso Cloud")));
        }

        public IActionResult Detalhar(long id)
        {
            var contrato = _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .Include(a => a.Licencas)
                                     .ThenInclude(a => a.LicencasDispositivos)
                                     .FirstOrDefault(a => a.ContratoId == id) ?? new Contratos();
            return View(contrato);
        }
    }
}
