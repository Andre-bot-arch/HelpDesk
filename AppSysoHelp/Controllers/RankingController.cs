using AppSysoHelp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class RankingController : Controller
    {

        private readonly ILogger<RankingController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HelpdesksysoContext _context;

        public RankingController(ILogger<RankingController> logger, HelpdesksysoContext context, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoje = DateTime.Now;
            var ranking = await ObterRankingMesAtual();

            if (ranking.Result is OkObjectResult okResult)
            {
                var viewModel = new RankingTecnicoViewModel
                {
                    Ranking = okResult.Value as List<RankingTecnico>,
                    DataReferencia = hoje
                };

                if (viewModel.Ranking != null)
                {
                    viewModel.TotalMes = (int)viewModel.Ranking.Sum(a => a.TotalComplexidadeMes);
                    viewModel.TotalChamadoMes = viewModel.Ranking.Sum(a => a.QtdChamadosMes);
                }

                return View(viewModel);
            }

            return View(new RankingTecnicoViewModel());
        }



        [HttpGet("mes-atual")]
        public async Task<ActionResult<List<RankingTecnico>>> ObterRankingMesAtual()
         {
            var hoje = DateTime.Now;

            var ranking = await _context.RankingTecnicos
                .FromSqlRaw("SELECT * FROM helpdesk.fn_RankingTecnicos({0}, {1}, NULL)",
                    hoje.Year, hoje.Month)
                .OrderByDescending(r => r.TotalComplexidadeMes)
                .ToListAsync();

            ViewBag.totalmes = ranking.Sum(a => a.TotalComplexidadeMes);
            ViewBag.totalChamadoMes = ranking.Sum(a => a.QtdChamadosMes);
            ViewBag.Ranking = ranking;

            return Ok(ranking);
        }

        /// <summary>
        /// Retorna o ranking de hoje
        /// </summary>
        // GET: api/ranking/hoje
        [HttpGet("hoje")]
        public async Task<ActionResult<List<RankingTecnico>>> ObterRankingHoje()
        {
            var hoje = DateTime.Now;

            var ranking = await _context.RankingTecnicos
                .FromSqlRaw("SELECT * FROM helpdesk.fn_RankingTecnicos({0}, {1}, {2})",
                    hoje.Year, hoje.Month, hoje.Day)
                .OrderByDescending(r => r.TotalComplexidadeDia)
                .ToListAsync();

            return Ok(ranking);
        }

        /// <summary>
        /// Retorna o ranking de um mês específico
        /// </summary>
        /// <param name="ano">Ano (ex: 2024)</param>
        /// <param name="mes">Mês (1-12)</param>
        // GET: api/ranking/mes?ano=2024&mes=10
        [HttpGet("mes")]
        public async Task<ActionResult<List<RankingTecnico>>> ObterRankingFiltrado(
            [FromQuery] int ano,
            [FromQuery] int mes)
        {
            if (mes < 1 || mes > 12)
                return BadRequest("Mês deve ser entre 1 e 12");

            if (ano < 2000 || ano > 2100)
                return BadRequest("Ano inválido");

            var ranking = await _context.RankingTecnicos
                .FromSqlRaw("SELECT * FROM helpdesk.fn_RankingTecnicos({0}, {1}, NULL)",
                    ano, mes)
                .OrderByDescending(r => r.TotalComplexidadeMes)
                .ToListAsync();

            return Ok(ranking);
        }

        /// <summary>
        /// Retorna o ranking de um dia específico
        /// </summary>
        /// <param name="ano">Ano (ex: 2024)</param>
        /// <param name="mes">Mês (1-12)</param>
        /// <param name="dia">Dia (1-31)</param>
        // GET: api/ranking/dia?ano=2024&mes=10&dia=27
        [HttpGet("dia")]
        public async Task<ActionResult<List<RankingTecnico>>> ObterRankingDia(
            [FromQuery] int ano,
            [FromQuery] int mes,
            [FromQuery] int dia)
        {
            if (mes < 1 || mes > 12)
                return BadRequest("Mês deve ser entre 1 e 12");

            if (dia < 1 || dia > 31)
                return BadRequest("Dia deve ser entre 1 e 31");

            if (ano < 2000 || ano > 2100)
                return BadRequest("Ano inválido");

            var ranking = await _context.RankingTecnicos
                .FromSqlRaw("SELECT * FROM helpdesk.fn_RankingTecnicos({0}, {1}, {2})",
                    ano, mes, dia)
                .OrderByDescending(r => r.TotalComplexidadeDia)
                .ToListAsync();

            return Ok(ranking);
        }

        /// <summary>
        /// Retorna o ranking com filtros opcionais
        /// </summary>
        // GET: api/ranking?ano=2024&mes=10&dia=27
        [HttpGet]
        public async Task<ActionResult<List<RankingTecnico>>> ObterRanking(
            [FromQuery] int? ano = null,
            [FromQuery] int? mes = null,
            [FromQuery] int? dia = null)
        {
            var dataReferencia = DateTime.Now;

            var anoFinal = ano ?? dataReferencia.Year;
            var mesFinal = mes ?? dataReferencia.Month;
            int? diaFinal = dia;

            if (mesFinal < 1 || mesFinal > 12)
                return BadRequest("Mês deve ser entre 1 e 12");

            if (dia.HasValue && (dia < 1 || dia > 31))
                return BadRequest("Dia deve ser entre 1 e 31");

            var ranking = await _context.RankingTecnicos
                .FromSqlRaw("SELECT * FROM dbo.fn_RankingTecnicos({0}, {1}, {2})",
                    anoFinal, mesFinal, diaFinal ?? (object)DBNull.Value)
                .OrderByDescending(r => diaFinal.HasValue ? r.TotalComplexidadeDia : r.TotalComplexidadeMes)
                .ToListAsync();

            return Ok(ranking);
        }


       
    }
}


public class RankingTecnicoViewModel
{
    public List<RankingTecnico> Ranking { get; set; } = new();
    public int TotalMes { get; set; }
    public int TotalChamadoMes { get; set; }
    public DateTime DataReferencia { get; set; } = DateTime.Now;
    public string PeriodoAtual => DataReferencia.ToString("MMMM/yyyy");
}