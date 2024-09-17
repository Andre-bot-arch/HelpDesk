using System.Reflection.Metadata.Ecma335;

namespace AppSysoHelp.Models.ViewModels
{
    public class ViewModelApiLicenca
    {
        public bool Autorizado { get; set; }
        public string Hash { get; set; } = null!;
        public string Mensagem { get; set; } = null!;
        public Clientes Cliente { get; set; } = new Clientes();

    }
}
