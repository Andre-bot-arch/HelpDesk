namespace AppSysoHelp.Models.ViewModels
{
    public class ViewModelTime
    {
        public long? PkId { get; set; }
        public long? FkTecnico { get; set; }
        public string Nome { get; set; } = null!;
        public string? Funcao { get; set; }
        public string? CaminhoImagem { get; set; }
        public string? imagemBase64 { get; set; }
        public string? extensaoArquivo { get; set; }
    }
}
