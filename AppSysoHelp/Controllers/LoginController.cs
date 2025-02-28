using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.Extensions.Configuration;

namespace AppSysoHelp.Controllers
{
    public class LoginController : Controller
    {
        private readonly HelpdesksysoContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            var usuario = _context.TecnicosSupervisores.FirstOrDefault(a => a.EmailContato.ToUpper() == email.ToUpper() && a.Cpf == password);
            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                    new Claim(ClaimTypes.Email, usuario.EmailContato),
                    new Claim("Id", Convert.ToString(usuario.PkId)),
                    new Claim("Role", usuario.CargoResponsabilidade)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Faz com que o cookie de autenticação seja persistente
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1), // Define a expiração do cookie para uma hora a partir de agora
                    RedirectUri = "/Home/Index" // Redireciona o usuário para a página inicial após o login
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Painel", "Home");
            }

            return View();
        }
    }
}
