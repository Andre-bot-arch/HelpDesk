using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class ErroController : Controller
    {
        [Route("Erro/404")]
        public IActionResult NotFoundPage()
        {
            return View("NotFound");
        }
    }
}
