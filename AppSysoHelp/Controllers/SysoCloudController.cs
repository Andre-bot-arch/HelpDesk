using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using X.PagedList.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AppSysoHelp.Controllers
{
    public class SysoCloudController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContrato _contrato;
        private readonly ServiceGenerico _generico;
        //private readonly ServiceGoogle _google;

        public SysoCloudController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _contrato = new ServiceContrato(context);
            _generico = new ServiceGenerico(context);
        }
        public IActionResult Index(int? page, string query)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            if (!string.IsNullOrEmpty(query))
            {
                var lista = _contrato.BuscarContratos().Where(a => a.FkPlataforma.NomePlataforma.Contains("Syso Cloud") && a.SituacaoContrato.Trim() == "ATIVO"
                                                                   && (a.FkCliente.Fantasia.Contains(query.ToUpper()) || a.FkCliente.NomeCliente.Contains(query.ToUpper())))
                                                       .OrderBy(a => a.FkCliente.Fantasia)
                                                       .ToPagedList(pageNumber, pageSize);
                ViewBag.Query = query;
                return View(lista);
            }
            var contratos = _contrato.BuscarContratos()
                                     .Where(a => a.FkPlataforma.NomePlataforma.Contains("Syso Cloud") && a.SituacaoContrato.Trim() == "ATIVO")
                                     .OrderBy(a => a.FkCliente.Fantasia)
                                     .ToPagedList(pageNumber, pageSize);
            return View(contratos);
            
        }

        public IActionResult Detalhar(long id)
        {
            var contrato = _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .Include(a => a.Licencas)
                                     .ThenInclude(a => a.Dispositivos)
                                     .FirstOrDefault(a => a.ContratoId == id) ?? new Contratos();
            var eventos = _context.SysoCloud.Where(a => a.FkContratoId == contrato.ContratoId).ToList();
            ViewBag.Eventos = eventos;
            return View(contrato);
        }

        [HttpPost]
        public IActionResult Create(SysoCloud s)
        {
            s.DataCreate = DateTime.UtcNow.AddHours(-4);
            _generico.GravarGenerico(s);
            return RedirectToAction("Detalhar", "SysoCloud", new { id = s.FkContratoId });
        }

        public FileContentResult GerarEventoTxt(long contratoId)
        {
            var listaEventos = _context.SysoCloud.Where(a => a.FkContratoId == contratoId).ToList();

            // Crie uma StringBuilder para construir o conteúdo do arquivo
            StringBuilder sb = new StringBuilder();

            // Dicionário de dias da semana
            var diasDaSemana = new Dictionary<string, Func<SysoCloud, bool>>
            {
                {"Monday", item => (bool)item.Segunda},
                {"Tuesday", item => (bool)item.Terca},
                {"Wednesday", item => (bool)item.Quarta},
                {"Thursday", item => (bool)item.Quinta},
                {"Friday", item => (bool)item.Sexta},
                {"Saturday", item => (bool)item.Sabado},
                {"Sunday", item => (bool)item.Domingo}
            };

            var contador = 1;

            // Iterar sobre cada item da lista
            foreach (var item in listaEventos)
            {
                // Verificar cada dia da semana
                foreach (var dia in diasDaSemana)
                {
                    // Se o dia da semana for verdadeiro, adicionar ao StringBuilder
                    if (dia.Value(item))
                    {
                        sb.AppendLine($"{contador}|{item.Horario}|{dia.Key}|{item.CaminhoBuscaArquivo}|{item.CaminhoDownload}|{item.CaminhoDownload}|{item.CaminhoUpload}|{DateTime.Parse(item.DataCreate.ToString()).ToString("dd/MM/yyyy HH:mm:ss")}|{item.Descricao}|{item.Extensao}|{item.Horario}|{item.PkId}|{item.ExecutarAposBackup}|{item.TipoBackup}");
                        contador++;
                    }
                }
            }

            // Converta o conteúdo para um array de bytes
            var fileContent = Encoding.UTF8.GetBytes(sb.ToString());

            // Retorne o arquivo como resposta
            return File(fileContent, "text/plain", "Evento.txt");
        }


        public FileContentResult GerarEmpresaTxt(long contratoId)
        {
            var empresa = _context.Contratos
                                  .Include(a => a.FkCliente)
                                  .FirstOrDefault(a => a.ContratoId == contratoId);
            
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{empresa.FkCliente.Bairro}|{empresa.FkCliente.Documento}||{empresa.DataInicio}|{empresa.DataFim}|{empresa.FkCliente.Email}||{empresa.FkClienteId}|{empresa.FkCliente.NomeCliente}||{empresa.FkCliente.NomeCliente}|{empresa.FkCliente.Logradouro}|{empresa.FkCliente.TelefoneCliente}");

            // Converta o conteúdo para um array de bytes
            var fileContent = Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileContent, "text/plain", "Empresa.txt");
        }

        public FileContentResult GerarEmailTxt()
        {
            // Crie uma StringBuilder para construir o conteúdo do arquivo
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("sysotecnologia@gmail.com|3|SISTEMA DE GERENCIAMENTO DE BACKUP|587|clwjsrzgoiqfjksf|smtp.gmail.com");

            // Converta o conteúdo para um array de bytes
            var fileContent = Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileContent, "text/plain", "Email.txt");
        }

        public IActionResult Delete(long id)
        {
            var evento = _context.SysoCloud.FirstOrDefault(a => a.PkId == id);
            _context.SysoCloud.Remove(evento);
            _context.SaveChanges();
            return RedirectToAction("Detalhar", "SysoCloud", new { id = evento.FkContratoId });
        }

        public IActionResult ListaPasta()
        {
            return View();
        }

        //public async System.Threading.Tasks.Task<ActionResult> Index(System.Threading.CancellationToken cancellationToken)
        //{
        //    //var result = await new Google.Apis.Auth.OAuth2.Mvc.AuthorizationCodeMvcApp(this, new ServiceGoogle()).
        //        //AuthorizeAsync(cancellationToken);

        //    if (result.Credential != null)
        //    {
        //        var service = new Google.Apis.Drive.v3.DriveService(new Google.Apis.Services.BaseClientService.Initializer
        //        {
        //            HttpClientInitializer = result.Credential,
        //            ApplicationName = "ASP.NET MVC Sample"
        //        });

        //        ViewBag.Arquivos = ListarArquivos(service);

        //        return View();
        //    }
        //    else
        //    {
        //        return new RedirectResult(result.RedirectUri);
        //    }
        //}
    }
}
