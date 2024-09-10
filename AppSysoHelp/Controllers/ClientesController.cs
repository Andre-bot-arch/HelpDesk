using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace AppSysoHelp.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        public ClientesController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index(int? page, string? query)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (query != null)
            {
                var clientePesquisa = _context.Clientes.Where(a => a.NomeCliente.Contains(query) || a.Documento.Contains(query)).OrderBy(u => u.NomeCliente).ToPagedList(pageNumber, pageSize);
                ViewBag.Query = query;
                return View(clientePesquisa);
            }
            var clientes = _context.Clientes
                                    .OrderBy(u => u.NomeCliente)
                                    .ToPagedList(pageNumber, pageSize);
            return View(clientes);
        }
    }
}
