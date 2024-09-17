using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppSysoHelp.Controllers.Api
{
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

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<LicencaController>/5
        [HttpGet("{licenca}/{apelido}/{serial}/{token}")]
        public IActionResult Get(string licenca, string apelido, string serial, string token)
        {
            if (token == "Syso@3680")
            {
                var contrato = _licenca.VerificarChave(licenca, apelido, serial);
                return Ok(contrato);
            }
            return BadRequest("Token inválido.");
        }

        // POST api/<LicencaController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LicencaController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LicencaController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
