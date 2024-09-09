using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceGenerico _generico;

        public UsuarioController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _generico = new ServiceGenerico(context);
        }
        public IActionResult Index()
        {
            return View(_context.TecnicosSupervisores.ToList());
        }

        [HttpPost]
        public JsonResult Create(TecnicosSupervisores form)
        {
            try
            {

                if (form.PkId == 0 && _generico.GravarGenerico(form))
                    return Json(new { success = true, message = "Usuário cadastrada com sucesso!" });
                else if (_generico.UpdateGenerico(form))
                    return Json(new { success = true, message = "Alteração registrada com sucesso!" });
                else
                    return Json(new { success = false, message = "Erro no processamento!" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao cadastrar a usuário: " + ex.Message });
            }
        }

    }
}
