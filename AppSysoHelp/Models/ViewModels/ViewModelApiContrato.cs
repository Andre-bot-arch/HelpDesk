namespace AppSysoHelp.Models.ViewModels
{
    public class ViewModelApiContrato
    {
        public string? IDSOLUTION { get; set; }
        public string? FKPLATAFORMAID { get; set; }
        public DateOnly DATAINICIO { get; set; }
        public DateOnly DATAFIM { get; set; }
        public string? SITUACAOCONTRATO { get; set; }
        public double VALOR { get; set; }
        public string? DESCRICAOCONTRATO { get; set; }
        public int PONTOSCONTRATADOS { get; set; }
        public string? FKCLIENTEID { get; set; }
    }
}
