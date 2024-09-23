using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class LicencaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceLicenca _licenca;
        private readonly ServiceGenerico _generico;

        public LicencaController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _licenca = new ServiceLicenca(context);
            _generico = new ServiceGenerico(context);
        }
        [HttpPost]
        public IActionResult GravarLicenca(Licencas l)
        {
            if (l.LicencaId > 0)
            {
                var obj = _context.Licencas.FirstOrDefault(a => a.LicencaId == l.LicencaId);
                obj.DataAtivacao = l.DataAtivacao;
                obj.Descricao = l.Descricao;
                obj.Modelo = l.Modelo;
                obj.NumeroSerie = l.NumeroSerie;
                obj.Urlacesso = l.Urlacesso;
                if (_generico.UpdateGenerico(obj))
                    return Json(new { success = true, data = l });
            }
            else
            {
                l.Hash = _licenca.GerarHash();
                if (_generico.GravarGenerico(l))
                    return Json(new { success = true, data = l });
            }

            return Json(new { success = false, message = "Erro ao buscar os Gravar: " });
        }

        [HttpPost]         
        public IActionResult UpdateEstatusLicenca(string ativo, long id = 0)
        {
            if (id > 0)
            {
                var licenca = _licenca.BuscarLicencaPorId(id);
                licenca.Ativo = (ativo == "True") ? false : true;
                if (_generico.UpdateGenerico(licenca))
                    return Json(new { success = true, data = licenca, message = "Finalizado com sucesso! " });

            }
          
                return Json(new { success = false, message = "Erro ao atualizar os dados: " });
        }

        [HttpPost]
        public IActionResult ExcluirDispoditivo(long id)
        {
            try
            {
                var disp = _context.Dispositivos.FirstOrDefault(a => a.Id == id);
                _context.Remove(disp);
                _context.SaveChanges();
                return Json(new { success = true, message = "Finalizado com sucesso! " });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Erro ao atualizar os dados: " });
            }
           
        }


    }
}
