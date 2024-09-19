using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace AppSysoHelp.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceGenerico _generico;
        public ClientesController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _generico = new ServiceGenerico(context);
        }
        [HttpPost]
        public IActionResult BuscarClientePorId(long id)
        {
            var cli = _context.Clientes.FirstOrDefault(a => a.ClienteId == id);
            return Json($"{cli.TelefoneCliente}/{cli.WhatsApp}");
        }

        public IActionResult Index(int? page, string? query)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (query != null)
            {
                var clientePesquisa = _context.Clientes.Where(a => a.NomeCliente.Contains(query) || a.Documento.Contains(query))
                                                       .OrderBy(u => u.ClienteId)
                                                       .ToPagedList(pageNumber, pageSize);
                ViewBag.Query = query;
                return View(clientePesquisa);
            }
            var clientes = _context.Clientes
                                    .OrderBy(u => u.ClienteId)
                                    .ToPagedList(pageNumber, pageSize);
            return View(clientes);
        }

        public async Task<IActionResult> BuscarAtualizarClienteSolutionAsync()
        {
            var totalAtualizado = await _generico.AtualizarCliente();
            return Json(new { success = true, message = $"{totalAtualizado} Cliente(s) atualizado(s) com sucesso!" });
        }
    }
}
