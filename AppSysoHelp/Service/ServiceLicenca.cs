using AppSysoHelp.Models;

namespace AppSysoHelp.Service
{
    public class ServiceLicenca
    {
        private readonly HelpdesksysoContext _context;
        public ServiceLicenca(HelpdesksysoContext context)
        {
            _context = context;
        }

        internal Licencas BuscarLicencaPorId(long id)
        {
            return _context.Licencas.FirstOrDefault(a => a.LicencaId == id);
        }
    }
}
