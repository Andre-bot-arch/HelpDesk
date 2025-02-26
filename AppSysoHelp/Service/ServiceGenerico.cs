using AppSysoHelp.Models;
using Newtonsoft.Json;

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

        internal bool UpdateGenericoRanger(IEnumerable<dynamic> obj)
        {
            _context.UpdateRange(obj);
            _context.SaveChangesAsync();
            return true;
        }

        internal async Task<bool> GravarGenericoAsync(dynamic obj)
        {
            await _context.AddAsync(obj);
            await _context.SaveChangesAsync();
            return true;
        }

        internal async Task<int> AtualizarCliente()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://sysolicencamobile.ddns.net:60443/Api/Help");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var clientes = JsonConvert.DeserializeObject<List<Clientes>>(content);

            foreach (var item in clientes)
            {
                var clienteExiste = _context.Clientes.FirstOrDefault(a => a.IdSolution == item.IdSolution);
               // if (clienteExiste.IdSolution == "10013941004207")
               // {
               //     var aqui = "";
               // }
                if (clienteExiste != null)
                {
                    clienteExiste.NomeCliente = item.NomeCliente;
                    clienteExiste.Fantasia = item.Fantasia;
                    clienteExiste.Documento = item.Documento;
                    clienteExiste.WhatsApp = item.WhatsApp;
                    clienteExiste.TelefoneCliente = item.TelefoneCliente;
                    clienteExiste.Email = item.Email;
                    clienteExiste.Cep = item.Cep;
                    clienteExiste.Logradouro = item.Logradouro;
                    clienteExiste.Bairro = item.Bairro;
                    clienteExiste.Uf = item.Uf;
                    clienteExiste.Situacao = item.Situacao;
                    UpdateGenerico(clienteExiste);
                }
                else
                {
                    GravarGenerico(item);
                }
            }
            return clientes.Count();
        }

        internal async Task<bool> UpdateGenericoRanger(IQueryable<Contratos> listacontrato)
        {
            await _context.AddRangeAsync(listacontrato);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
