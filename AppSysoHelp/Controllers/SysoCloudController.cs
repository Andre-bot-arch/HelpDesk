using AppSysoHelp.Models;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AppSysoHelp.Controllers
{
    public class SysoCloudController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;
        private readonly ServiceContrato _contrato;
        private readonly ServiceGenerico _generico;

        public SysoCloudController(IConfiguration configuration, HelpdesksysoContext context)
        {
            _configuration = configuration;
            _context = context;
            _contrato = new ServiceContrato(context);
            _generico = new ServiceGenerico(context);
        }
        public IActionResult Index()
        {
            return View(_contrato.BuscarContratos().Where(a => a.FkPlataforma.NomePlataforma.Contains("Syso Cloud")));
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
            s.DataCreate = DateTime.Now;
            _generico.GravarGenerico(s);
            return RedirectToAction("Index");
        }

        public void ExportarTxt(long contratoId)
        {
            var listaEventos = _context.SysoCloud.Where(a => a.FkContratoId == contratoId).ToList();

            // Defina o caminho do arquivo
            string filePath = "eventos.txt";

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

            // Iterar sobre cada item da lista
            foreach (var item in listaEventos)
            {
                // Verificar cada dia da semana
                foreach (var dia in diasDaSemana)
                {
                    // Se o dia da semana for verdadeiro, adicionar ao StringBuilder
                    if (dia.Value(item))
                    {
                        sb.AppendLine($"835|{item.Horario}|{dia.Key}|{item.CaminhoDownload}||{item.CaminhoUpload}|{item.DataCreate}|{item.Descricao}|{item.Extensao}|{item.Horario}|11|{item.ExecutarAposBackup}|{item.TipoBackup}");
                    }
                }
            }
        }
    }

}
