using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class ChamadoController : Controller
    {
        public IActionResult Aberto()
        {
            return View();
        }

        public IActionResult GravarChamado(Chamados m)
        {
            return RedirectToAction("Aberto");
        }
    }
}
