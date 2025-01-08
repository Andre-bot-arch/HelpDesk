using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers.Api
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class LicencaController : ControllerBase
    {
        private readonly ServiceGenerico _generico;
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceLicenca _licenca;

        public LicencaController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _generico = new ServiceGenerico(context);
            _licenca = new ServiceLicenca(context);
        }

        [HttpGet("{licenca}/{apelido}/{serial}/{token}")]
        public ViewModelApiLicenca Get(string licenca, string apelido, string serial, string token)
        {
            if (token == "Syso@3680")
            {
                var contrato = _licenca.VerificarChave(licenca, apelido, serial);
                return contrato;
            }
            return null;
        }

        [HttpGet("Gravar/{licenca}/{serial}/{token}")]
        public ViewModelApiLicenca Get(string licenca, string serial, string token)
        {
            if (token == "Syso@3680")
            {
                var dados = _context.Licencas.Include(a=> a.Dispositivos).FirstOrDefault(a => a.Hash.Trim() == licenca.Trim());
                foreach (var dado in dados.Dispositivos.Where(a => a.Equipamento.Trim() == serial.Trim()))
                {
                    dado.UltimoAcesso = DateTime.UtcNow.AddHours(-4);
                }
                _context.UpdateRange(dados);
                _context.SaveChanges();
                var ret = _licenca.VerificarLicencaLog(licenca);
                return ret;
            }
            return null; 
        }

        [HttpPost("{licenca}/{apelido}/{serial}/{token}")]
        public ViewModelApiLicenca Post(string licenca, string apelido, string serial, string token)
        {
            if (token == "Syso@3680")
            {
                var contrato = _licenca.VerificarEstatusDispositivo(licenca, apelido, serial);
                return contrato;
            }
            return null;
        }
    }
}
