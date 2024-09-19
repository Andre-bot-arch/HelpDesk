using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;

namespace AppSysoHelp.Service
{
    public class ServiceContrato
    {
        private readonly HelpdesksysoContext _context;
        public ServiceContrato(HelpdesksysoContext context)
        {
            _context = context;
        }

        internal async Task<bool> AtualizarContrato()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://sysolicencamobile.ddns.net:60443/Api/Contratos");
            var response = await client.SendAsync(request);
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var objectList = JsonSerializer.Deserialize<List<ViewModelApiContrato>>(jsonResponse);
                    foreach (var item in objectList.GroupBy(a => a.DESCRICAOCONTRATO))
                    {
                        var idPlataforma = (item.Key.Contains("BACKUP")) ? 9 :
                                           (item.Key.Contains("SYSO MOBILE")) ? 10 :
                                           (item.Key.Contains("IMENDES")) ? 4 :
                                           (item.Key.Contains("COLETOR")) ? 11 :
                                           (item.Key.Contains("SYSO CAR")) ? 7 : 0;
                        foreach (var contratos in item)
                        {
                            if (idPlataforma > 0)
                            {
                                var pkid = _context.PlataformasContratos.FirstOrDefault(a => a.PlataformaId == idPlataforma);
                                var contrato = _context.Contratos.Include(a => a.FkCliente)
                                                                 .FirstOrDefault(a => a.IdSolution.Trim() == contratos.IDSOLUTION.Trim()
                                                                                   && a.FkCliente.IdSolution == contratos.FKCLIENTEID);
                                if (contratos != null)
                                {
                                    
                                }
                                else
                                {
                                    var obj = new Contratos
                                    {
                                        DataFim = contratos.DATAFIM,
                                    };
                                }
                             }
                        }
                    }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }


            return true;
        }

        internal IList<Contratos> BuscarContratos()
        {
            return _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .ToList();
        }

        internal IList<Contratos> BuscarContratosCancelados()
        {
            return _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .Where(a => a.SituacaoContrato == "Inativo")
                                     .ToList();
        }

        internal Contratos BuscarContratosPorId(long contratoId)
        {
            return _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .Include(a => a.Licencas)
                                     .ThenInclude(a => a.Dispositivos)
                                     .FirstOrDefault(a => a.ContratoId == contratoId) ?? new Contratos();
        }
    }
}
