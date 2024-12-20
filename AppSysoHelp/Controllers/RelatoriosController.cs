using AppSysoHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly HelpdesksysoContext _context;

        public RelatoriosController(HelpdesksysoContext context)
        {
            _context = context;
        }
        public IActionResult Index(int id)
        {
            ViewBag.tipo = id;
            return View();
        }

        public IActionResult ChamadosPorPeriodo(string? tecnico, string? cliente, DateTime dataInicio, DateTime dataFinal)
        {

            var chamadosFiltrados = _context.Atendimentos.Include(a => a.FkTecnico)
                                                         .Include(c => c.FkChamado).ThenInclude(c => c.FkSituacaoChamado)
                                                         .Include(a => a.FkChamado).ThenInclude(c => c.FkCliente)
                                                         .Where(a => a.DataFechamento >= dataInicio && a.DataFechamento <= Convert.ToDateTime(dataFinal.ToString("dd/MM/yyyy 23:59:59")))
                                                         .AsEnumerable();
            var agrupamento = new List<Atendimentos>();
            foreach (var item in chamadosFiltrados.GroupBy(a=> a.FkChamadoId))
            {
                var aten = item.OrderByDescending(a => a.AtendimentoId).FirstOrDefault();
                agrupamento.Add(aten!);
            }

            if (!string.IsNullOrEmpty(cliente))
                agrupamento = agrupamento.Where(a => a.FkChamado.FkCliente.ClienteId == Convert.ToInt64(cliente)).ToList();


            if (!string.IsNullOrEmpty(tecnico))
                agrupamento = agrupamento.Where(a => a.FkTecnicoId == Convert.ToInt64(tecnico)).ToList();


            ViewBag.DataInicio = dataInicio.ToShortDateString();
            ViewBag.DataFinal = dataFinal.ToShortDateString();
            ViewBag.TotalChamados = agrupamento.Count();

            return View(agrupamento);
        }

        public IActionResult Atendimentos(DateTime? startDate, DateTime? endDate)
        {
            var chamados = ListaAtendimentos();

            // Filtra os chamados de acordo com o intervalo de datas, se as datas forem fornecidas
            if (startDate.HasValue && endDate.HasValue)
            {
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                chamados = chamados.Where(ch => ch.DataAbertura >= startDate.Value && ch.DataAbertura <= endDate.Value).ToList();
            }

            return View(chamados);
        }
        public List<VwAtendimentos> ListaAtendimentos()
        {
            // Realiza a consulta com a ordenação diretamente no Entity Framework
            var todosAtendimentos = _context.VwAtendimentos
                .OrderByDescending(a => a.ProtocoloChamado)
                .ThenBy(a => a.DataAtendimento)
                .ToList();

            return todosAtendimentos;
        }
    }
}
