namespace AppSysoHelp.Models.ViewModels
{
    public class ViewModelPlataforma
    {
        public long? PlataformaId { get; set; }
        public string NomePlataforma { get; set; } = null!;
        public string? Descricao { get; set; }
        public string? CaminhoImagem { get; set; }
        public string? imagemBase64 { get; set; }
        public string? extensaoArquivo { get; set; }
    }
}
