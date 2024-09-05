using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class LicencaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceLicenca _licenca;
        private readonly ServiceGenerico _generico;

        public LicencaController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _licenca = new ServiceLicenca(context);
            _generico = new ServiceGenerico(context);
        }
        [HttpPost]
        public IActionResult GravarLicenca(Licencas l)
        {
           if(_generico.GravarGenerico(l))
                return Json(new { success = true, data = l });

            return Json(new { success = false, message = "Erro ao buscar os Gravar: "});
        }
    }
}
