using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

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
        public IActionResult Create(ChamadosSubCategoria form)
        {
            try
            {
                if (form.SubCategoriaId > 0)
                {
                    var sub = _context.ChamadosSubCategoria.FirstOrDefault(a => a.SubCategoriaId == form.SubCategoriaId);
                    sub.Descricao = form.Descricao;
                    sub.Prioridade = form.Prioridade;
                    sub.Complexidade = form.Complexidade;
                    _context.Update(sub);
                    _context.SaveChanges();
                    form.FkCategoria = sub.FkCategoria;
                }
                else
                {
                    _context.Add(form);
                    _context.SaveChanges();
                }
                var chamado = _context.ChamadosSubCategoria.Where(a => a.FkCategoria == form.FkCategoria && a.Situacao == true).ToList();
                return PartialView("_DetalharSubCategorias", chamado);

            }
            catch (Exception ex)
            {
                return PartialView("_DetalharSubCategorias", new List<ChamadosSubCategoria>());
            }
        }

        [HttpPost]
        public IActionResult Detalhes(long id)
        {
            // Verifica se o ID fornecido é válido
            if (id <= 0)
            {
                return BadRequest("ID inválido fornecido.");
            }

            try
            {
                // Procura a subcategoria pelo ID
                var subcategoria = _context.ChamadosSubCategoria.FirstOrDefault(a => a.FkCategoria == id && a.Situacao == true);

                if (subcategoria != null)
                {
                    // Busca todos os chamados relacionados à subcategoria
                    var chamados = _context.ChamadosSubCategoria
                                           .Where(a => a.FkCategoria == id && a.Situacao == true)
                                           .ToList();

                    return PartialView("_DetalharSubCategorias", chamados);
                }
                else
                {
                    // Retorna erro 404 se não encontrar a subcategoria
                    return NotFound("Subcategoria não encontrada.");
                }
            }
            catch (Exception ex)
            {
                // Log de erro (opcional, se você tiver um sistema de logs)
                // _logger.LogError(ex, "Erro ao buscar detalhes da subcategoria com ID {id}", id);

                // Retorna erro 500 se houver uma exceção
                return StatusCode(500, $"Erro no servidor: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult ListarPorIdCategoria(long id = 0)
        {
            var categoria = _context.ChamadosSubCategoria
                                                   .Where(a => a.FkCategoria == id && a.Situacao == true)
                                                   .ToList();

            return Json(categoria);
        }

        [HttpPost]
        public IActionResult Inativar(long id = 0)
        {
            var categoria = _context.ChamadosSubCategoria
                                                   .FirstOrDefault(a => a.SubCategoriaId == id);
            categoria.Situacao = false;

            _context.Update(categoria);
            _context.SaveChanges();

            return Ok();
        }

        public IActionResult Editar(long id = 0)
        {
            var subcategoria = _context.ChamadosSubCategoria
                                                   .FirstOrDefault(a => a.SubCategoriaId == id);


            return PartialView("_ModalEditarSubCategoria",subcategoria);
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
