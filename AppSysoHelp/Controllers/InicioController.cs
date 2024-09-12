using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class InicioController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;

        public InicioController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.PlataformasContratos.ToList());
        }
    }
}
