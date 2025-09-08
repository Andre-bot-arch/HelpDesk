namespace AppSysoHelp.Models.Dto
{
    public class AtendimentoDto
    {
        public long AtendimentoId { get; set; }
        public long FkChamadoId { get; set; }
        public string ProcedimentosAplicados { get; set; }
        public DateTime? DataAtendimento { get; set; }
        public DateTime? NovaDataAtendimento { get; set; }
        public DateTime? DataFechamento { get; set; }
        public string SatisfacaoCliente { get; set; }
        public string CaminhoDaImagem { get; set; }
        public TimeOnly? Inicio { get; set; }
        public TimeOnly? Fim { get; set; }
        public long? FkTecnicoId { get; set; }
        public string TecnicoNome { get; set; } // Nome completo do técnico
    }
}
