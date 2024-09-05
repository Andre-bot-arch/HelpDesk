using AppSysoHelp.Models;
using Microsoft.EntityFrameworkCore;

namespace AppSysoHelp.Service
{
    public class ServiceContrato
    {
        private readonly HelpdesksysoContext _context;
        public ServiceContrato(HelpdesksysoContext context)
        {
            _context = context;
        }

        internal IList<Contratos> BuscarContratos()
        {
            return _context.Contratos.Include(a=> a.FkPlataforma)
                                     .Include(a=> a.FkCliente)
                                     .ToList();
        }

        internal Contratos BuscarContratosPorId(long contratoId)
        {
            return _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .FirstOrDefault(a=> a.ContratoId == contratoId);
        }
    }
}
