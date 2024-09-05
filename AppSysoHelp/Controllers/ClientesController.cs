using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        public ClientesController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
