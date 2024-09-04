using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AppSysoHelp.Controllers
{
    public class ContratoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContrato _contrato;

        public ContratoController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _contrato = new ServiceContrato(context);
        }

        public IActionResult Index()
        {
            var lista = _contrato.BuscarContratos();
            return View(lista);
        }

        public IActionResult Gravar(Contratos c)
        {
            c.SituacaoContrato = "1";
            _context.Add(c);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> GetSugestaoCliente(string query)
        {
            var results = await _context.Clientes
            .Where(e => e.NomeCliente.Contains(query))
            .Select(e => new
            {
                value = e.ClienteId,
                label = e.NomeCliente.ToUpper()
            })
            .ToListAsync();

            return Ok(results);
        }

        public async Task<IActionResult> GetSugestaoSistema(string query)
        {
            var results = await _context.PlataformasContratos
            .Where(e => e.NomePlataforma.Contains(query))
            .Select(e => new
            {
                value = e.PlataformaId,
                label = e.NomePlataforma.ToUpper()
            })
            .ToListAsync();

            return Ok(results);
        }



    }
}
