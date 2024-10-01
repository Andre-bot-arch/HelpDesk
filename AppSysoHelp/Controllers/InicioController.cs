using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace AppSysoHelp.Controllers
{
    public class InicioController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContato _contato;

        public InicioController(IConfiguration configuration, HelpdesksysoContext context, ServiceContato contato)
        {
            _configuration = configuration;
            _context = context;
            _contato = contato;
        }

        public IActionResult Index()
        {
            return View(_context.PlataformasContratos.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> SendContactForm(string name, string email, string phone, string message)
        {

            if (name != null || email != null || phone != null || message != null)
            {
                _contato.EnviarEmail(name, email, phone, message);
                return Json(new { success = true, message = "Enviado com sucesso! " });

            }

            return Json(new { success = false, message = "Erro ao enviar o formulário: " });
        }
    }
}
