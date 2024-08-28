using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppSysoHelp.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Almir"),
                new Claim(ClaimTypes.Email, "almir.matos.dev@gmail.com"),
                new Claim("Role", "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Faz com que o cookie de autenticação seja persistente
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1), // Define a expiração do cookie para uma hora a partir de agora
                RedirectUri = "/Home/Index" // Redireciona o usuário para a página inicial após o login
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }
    }
}
