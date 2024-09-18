using System.Reflection.Metadata.Ecma335;

namespace AppSysoHelp.Models.ViewModels
{
    public class ViewModelApiLicenca
    {
        public bool Status { get; set; }
        public string Chave { get; set; } = null!;
        public string Url { get; set; }
        public string Mensagem { get; set; } = null!;
        public string Cnpj { get; set; } = null!;
        public string Empresa { get; set; } = null!;
    }
}
