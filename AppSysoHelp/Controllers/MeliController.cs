using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    [AllowAnonymous]
    [Route("meli")]
    public class MeliController : Controller
    {
        [HttpGet("callback")]
        public IActionResult Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Código de autorização ausente.");

            // Aqui você pode apenas exibir uma mensagem simples
            // ou logar o código para uso posterior
            return Content($"Autenticação Mercado Livre concluída com sucesso. Código: {code}");
        }
    }
}
