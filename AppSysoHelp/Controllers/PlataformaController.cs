using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class PlataformaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;

        public PlataformaController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(PlataformasContratos plataformas)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.PlataformasContratos.Add(plataformas);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Plataforma cadastrada com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Dados inválidos!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao cadastrar a plataforma: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(PlataformasContratos plataformas)
        {
            _context.PlataformasContratos.Update(plataformas);
            _context.SaveChanges();
            return View();
        }
    }
}
