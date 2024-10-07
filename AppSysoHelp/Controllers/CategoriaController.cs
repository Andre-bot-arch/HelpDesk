using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly HelpdesksysoContext _context;

        public CategoriaController(HelpdesksysoContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.ChamadosCategoria.ToList());
        }

        [HttpPost]
        public JsonResult Create(ChamadosCategoria form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(form);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Categoria cadastrada com sucesso!" });
                }
                else
                    return Json(new { success = false, message = "Dados inválidos!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao cadastrar Categoria: " + ex.Message });
            }
        }


        public IActionResult Detalhes(long id)
        {
            try
            {
                // Supondo que você tenha um método para buscar a plataforma por ID
                var categoria = _context.ChamadosCategoria.FirstOrDefault(a => a.CategoriaId == id);

                if (categoria != null)
                    return Json(new { success = true, data = categoria });
                else
                    return Json(new { success = false, message = "Categoria não encontrada." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao buscar os detalhes: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Edit(ChamadosCategoria form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verifica se a plataforma existe no banco de dados
                    var existingCategoria = _context.ChamadosCategoria.FirstOrDefault(a => a.CategoriaId == form.CategoriaId);
                    if (existingCategoria == null)
                        return Json(new { success = false, message = "Categoria não encontrada." });

                    // Atualiza os dados da plataforma
                    existingCategoria.Descricao = form.Descricao;
                    existingCategoria.Status = form.Status;

                    _context.ChamadosCategoria.Update(existingCategoria);
                    _context.SaveChanges();

                    return Json(new { success = true, message = "Categoria editada com sucesso!" });
                }
                else
                    return Json(new { success = false, message = "Dados inválidos!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao editar subcategoria: " + ex.Message });
            }
        }
    }
}
