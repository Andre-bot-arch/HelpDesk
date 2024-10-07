using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class SubCategoriaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;

        public SubCategoriaController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(ChamadosSubCategoria form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(form);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Subcategoria cadastrada com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Dados inválidos!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao cadastrar subcategoria: " + ex.Message });
            }
        }

        public IActionResult Detalhes(long id)
        {
            try
            {
                // Supondo que você tenha um método para buscar a plataforma por ID
                var subcategoria = _context.ChamadosSubCategoria.FirstOrDefault(a => a.SubCategoriaId == id);

                if (subcategoria != null)
                {
                    return Json(new { success = true, data = subcategoria });
                }
                else
                {
                    return Json(new { success = false, message = "Subcategoria não encontrada." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao buscar os detalhes: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Edit(ChamadosSubCategoria form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verifica se a plataforma existe no banco de dados
                    var existingSubCategoria = _context.ChamadosSubCategoria.FirstOrDefault(a => a.SubCategoriaId == form.SubCategoriaId);
                    if (existingSubCategoria == null)
                        return Json(new { success = false, message = "Subcategoria não encontrada." });

                    // Atualiza os dados da plataforma
                    existingSubCategoria.Descricao = form.Descricao;
                    existingSubCategoria.Complexidade = form.Complexidade;
                    existingSubCategoria.Prioridade = form.Prioridade;
                    existingSubCategoria.FkCategoria = form.FkCategoria;

                    _context.ChamadosSubCategoria.Update(existingSubCategoria);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Subcategoria editada com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Dados inválidos!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao editar subcategoria: " + ex.Message });
            }
        }
    }
}
