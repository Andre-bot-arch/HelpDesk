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
    }
}
