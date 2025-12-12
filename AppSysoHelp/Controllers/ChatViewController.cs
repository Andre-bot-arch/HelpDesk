using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class ChatViewController : Controller
    {
        [Authorize(Policy = "AdminOrManager")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
