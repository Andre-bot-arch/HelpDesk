using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
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
            var userIdClaim = User.FindFirst("Id");
            var userId = userIdClaim?.Value;

            var usuario = _context.TecnicosSupervisores.Find(Convert.ToInt64(userId));



            if (usuario.CargoResponsabilidade != "Admin")
            {
                TempData["ErrorMessage"] = "Acesso negado! Apenas administradores podem acessar.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.usuario = usuario;
            return View(_context.PlataformasContratos.ToList());
        }

        [HttpPost]
        public JsonResult Create(ViewModelPlataforma form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var plataformaAdd = new PlataformasContratos();
                    plataformaAdd.NomePlataforma = form.NomePlataforma;
                    plataformaAdd.Descricao = form.Descricao;
                    _context.Add(plataformaAdd);
                    _context.SaveChanges();

                    if (form.imagemBase64 != null)
                    {
                        byte[] barr = Convert.FromBase64String(form.imagemBase64);
                        string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "landing-page", "images", "plataforma-"+plataformaAdd.PlataformaId + "." + form.extensaoArquivo);
                        System.IO.File.WriteAllBytes(savePath, barr);
                        plataformaAdd.CaminhoImagem = $"/landing-page/images/plataforma-{plataformaAdd.PlataformaId}.{form.extensaoArquivo}";
                        _context.Update(plataformaAdd);
                        _context.SaveChanges();
                    }
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

        public IActionResult Detalhes(long id)
        {
            try
            {
                // Supondo que você tenha um método para buscar a plataforma por ID
                var plataforma = _context.PlataformasContratos.FirstOrDefault(a => a.PlataformaId == id);

                if (plataforma != null)
                {
                    return Json(new { success = true, data = plataforma });
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
        public JsonResult Edit(ViewModelPlataforma form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verifica se a plataforma existe no banco de dados
                    var existingPlataforma = _context.PlataformasContratos.FirstOrDefault(a => a.PlataformaId == form.PlataformaId);
                    if (existingPlataforma == null)
                    {
                        return Json(new { success = false, message = "Plataforma não encontrada." });
                    }

                    // Atualiza os dados da plataforma
                    existingPlataforma.NomePlataforma = form.NomePlataforma;
                    existingPlataforma.Descricao = form.Descricao;
                    if (form.imagemBase64 != null)
                    {
                        byte[] barr = Convert.FromBase64String(form.imagemBase64);
                        string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "landing-page", "images", "plataforma-" + existingPlataforma.PlataformaId + "." + form.extensaoArquivo);
                        System.IO.File.WriteAllBytes(savePath, barr);
                        existingPlataforma.CaminhoImagem = $"/landing-page/images/plataforma-{existingPlataforma.PlataformaId}.{form.extensaoArquivo}";
                    }
                    _context.PlataformasContratos.Update(existingPlataforma);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Plataforma atualizada com sucesso!" });
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

                return Json(new { success = false, message = $"Ocorreu um erro ao atualizar a plataforma: {ex.Message}" });
            }
        }
    }
}
