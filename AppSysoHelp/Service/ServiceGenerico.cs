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

        internal async Task<bool> GravarGenericoAsync(dynamic obj)
        {
            await _context.AddAsync(obj);
            await _context.SaveChangesAsync();
            return true;
        }

        internal async Task<bool> AtualizarCliente()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://sysolicencamobile.ddns.net:60443/Api/Help");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
           
            return true;
        }
    }
}
