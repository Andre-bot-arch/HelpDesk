using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace AppSysoHelp.Controllers
{
    public class TimeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;

        public TimeController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;

            var usuario = _context.TecnicosSupervisores.Find(Convert.ToInt64(userId));



            if (usuario.CargoResponsabilidade != "Admin")
            {
                TempData["ErrorMessage"] = "Acesso negado! Apenas administradores podem acessar.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.usuario = usuario;
            ViewBag.tecnicos = _context.TecnicosSupervisores.OrderBy(a=> a.NomeCompleto).ToList();
            return View(_context.Time.ToList());
        }

        [HttpPost]
        public async Task<JsonResult> Create(ViewModelTime form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var timeAdd = new Models.Time();
                    timeAdd.Nome = form.Nome;
                    timeAdd.Funcao = form.Funcao;
                    _context.Add(timeAdd);
                    _context.SaveChanges();

                    if (form.imagemBase64 != null)
                    {
                        byte[] barr = Convert.FromBase64String(form.imagemBase64);
                        using (var image = Image.Load(barr))
                        {
                            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "landing-page", "time", "time-" + timeAdd.PkId + ".webp");
                            await image.SaveAsync(savePath, new WebpEncoder());
                            timeAdd.CaminhoImagem = $"/landing-page/time/time-{timeAdd.PkId}.webp";
                        }
                        _context.Update(timeAdd);
                        _context.SaveChanges();
                    }
                    return Json(new { success = true, message = "Colaborador cadastrado com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Dados inválidos!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao cadastrar o colaborador: " + ex.Message });
            }
        }

        public IActionResult Detalhes(long id)
        {
            try
            {
                // Supondo que você tenha um método para buscar a plataforma por ID
                var time = _context.Time.FirstOrDefault(a => a.PkId == id);

                if (time != null)
                {
                    return Json(new { success = true, data = time });
                }
                else
                {
                    return Json(new { success = false, message = "Plataforma não encontrada." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao buscar os detalhes: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Edit(ViewModelTime form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verifica se a plataforma existe no banco de dados
                    var existingTime = _context.Time.FirstOrDefault(a => a.PkId == form.PkId);
                    if (existingTime == null)
                    {
                        return Json(new { success = false, message = "Plataforma não encontrada." });
                    }

                    // Atualiza os dados da plataforma
                    existingTime.Nome = form.Nome;
                    existingTime.Funcao = form.Funcao;
                    existingTime.FkTecnico = form.FkTecnico;
                    if (form.imagemBase64 != null)
                    {
                        byte[] barr = Convert.FromBase64String(form.imagemBase64);
                        using (var image = Image.Load(barr))
                        {
                            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "landing-page", "time", "time-" + existingTime.PkId + ".webp");
                            await image.SaveAsync(savePath, new WebpEncoder());
                            existingTime.CaminhoImagem = $"/landing-page/time/time-{existingTime.PkId}.webp";
                        }
                    }
                    _context.Time.Update(existingTime);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Colaborador atualizado com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Dados inválidos fornecidos." });
                }
            }
            catch (Exception ex)
            {
                // Log da exceção (opcional)
                // _logger.LogError(ex, "Erro ao atualizar a plataforma.");

                return Json(new { success = false, message = $"Ocorreu um erro ao atualizar o colaborador: {ex.Message}" });
            }
        }
    }
}
