using AppSysoHelp.Models;

namespace AppSysoHelp.Service
{
    public class ServiceGenerico
    {
        private readonly HelpdesksysoContext _context;
        public ServiceGenerico(HelpdesksysoContext context)
        {
            _context = context;
        }

        internal bool GravarGenerico(dynamic obj)
        {
            _context.Add(obj);
            _context.SaveChanges();
            return true;
        }

        internal bool UpdateGenerico(dynamic obj)
        {
            _context.Update(obj);
            _context.SaveChanges();
            return true;
        }
    }
}
