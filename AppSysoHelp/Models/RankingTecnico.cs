namespace AppSysoHelp.Models
{
    public class RankingTecnico
    {
        public long TecnicoId { get; set; }
        public string? NomeTecnico { get; set; }
        public string? Imagem { get; set; }
        public long TotalComplexidadeDia { get; set; }
        public long TotalComplexidadeMes { get; set; }
        public int QtdChamadosDia { get; set; }
        public int QtdChamadosMes { get; set; }
    }
}
