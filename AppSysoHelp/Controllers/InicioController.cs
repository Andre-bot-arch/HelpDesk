using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class InicioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
