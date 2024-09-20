namespace AppSysoHelp.Models.ViewModels
{
    public class ViewModelApiContrato
    {
        public string? IDSOLUTION { get; set; }
        public string? FKPLATAFORMAID { get; set; }
        public DateTime DATAINICIO { get; set; }
        public DateTime DATAFIM { get; set; }
        public string? SITUACAOCONTRATO { get; set; }
        public decimal VALOR { get; set; }
        public string? DESCRICAOCONTRATO { get; set; }
        public int PONTOSCONTRATADOS { get; set; }
        public string? FKCLIENTEID { get; set; }
    }
}
