using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class ContratoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContrato _contrato;
        private readonly ServiceGenerico _generico;

        public ContratoController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _contrato = new ServiceContrato(context);
            _generico = new ServiceGenerico(context);
        }

        public IActionResult Index()
        {
            var lista = _contrato.BuscarContratos();
            return View(lista);
        }

        public IActionResult Cancelados()
        {
            var lista = _contrato.BuscarContratosCancelados();
            return View(lista);
        }

        public IActionResult Gravar(Contratos c, string valor)
        {
            if (c.ContratoId > 0)
            {
                c.Valor = Convert.ToDecimal(valor.Replace(".", ","));
                c.SituacaoContrato = "Ativo";
                _context.Update(c);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            c.Valor = Convert.ToDecimal(valor.Replace(".", ","));
            c.SituacaoContrato = "Ativo";
            _context.Contratos.Add(c);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Detalhar(long id)
        {
            var contrato = _contrato.BuscarContratosPorId(id);
            return View(contrato);
        }

        public IActionResult UpdateEstatusContrato(string ativo, long id = 0)
        {
            if (id > 0)
            {
                var contrato = _contrato.BuscarContratosPorId(id);
                contrato.SituacaoContrato = (ativo == "Ativo") ?"Inativo" : "Ativo";
                if (_generico.UpdateGenerico(contrato))
                    return Json(new { success = true, message = "Finalizado com sucesso! " });

            }

            return Json(new { success = false, message = "Erro ao atualizar os dados: " });
        }

        public async Task<IActionResult> GetSugestaoCliente(string query)
        {
            var results = await _context.Clientes
            .Where(e => e.NomeCliente.Contains(query) || e.Fantasia.Contains(query))
            .Select(e => new
            {
                value = e.ClienteId,
                label = $"{e.Documento} - {e.Fantasia.ToUpper()}",
                telefone = $"{e.TelefoneCliente} | {e.WhatsApp}",
                fantasia = e.Fantasia,
                endereco = $"{e.Logradouro} / {e.Cidade}-{e.Uf}"
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

        public async Task<IActionResult> GetSugestaoTecnico(string query)
        {
            var results = await _context.TecnicosSupervisores
            .Where(e => e.NomeCompleto.Contains(query))
            .Select(e => new
            {
                value = e.PkId,
                label = e.NomeCompleto.ToUpper()
            })
            .ToListAsync();

            return Ok(results);
        }



    }
}
