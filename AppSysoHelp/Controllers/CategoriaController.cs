using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly HelpdesksysoContext _context;

        public CategoriaController(HelpdesksysoContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.ChamadosCategoria.ToList());
        }
    }
}
