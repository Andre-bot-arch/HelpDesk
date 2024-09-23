using AppSysoHelp.Models;
using AppSysoHelp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using System.Text.Json;

namespace AppSysoHelp.Service
{
    public class ServiceContrato
    {
        private readonly HelpdesksysoContext _context;
        private readonly ServiceGenerico _generico;
        public ServiceContrato(HelpdesksysoContext context)
        {
            _context = context;
            _generico = new ServiceGenerico(context);
        }

        internal async Task<int> AtualizarContrato()
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

                                var idcliente = _context.Clientes.FirstOrDefault(a => a.IdSolution == contratos.FKCLIENTEID).ClienteId;

                                if (contrato != null && idcliente > 0)
                                {
                                    contrato.DataFim = DateOnly.FromDateTime(contratos.DATAINICIO);
                                    contrato.DataInicio = DateOnly.FromDateTime(contratos.DATAINICIO);
                                    contrato.DescricaoContrato = contratos.DESCRICAOCONTRATO;
                                    contrato.FkClienteId = idcliente;
                                    contrato.Valor = contratos.VALOR;
                                    contrato.SituacaoContrato = contratos.SITUACAOCONTRATO;
                                    contrato.FkPlataformaId = idPlataforma;
                                    contrato.PontosContratados = contratos.PONTOSCONTRATADOS;
                                    contrato.IdSolution = contratos.IDSOLUTION;

                                    _generico.UpdateGenerico(contrato);
                                }
                                else if (idcliente > 0)
                                {
                                    var obj = new Contratos
                                    {
                                        DataFim = DateOnly.FromDateTime(contratos.DATAFIM),
                                        DataInicio = DateOnly.FromDateTime(contratos.DATAINICIO),
                                        DescricaoContrato = contratos.DESCRICAOCONTRATO,
                                        FkClienteId = idcliente,
                                        Valor = contratos.VALOR,
                                        SituacaoContrato = contratos.SITUACAOCONTRATO,
                                        FkPlataformaId = idPlataforma,
                                        PontosContratados = contratos.PONTOSCONTRATADOS,
                                        IdSolution = contratos.IDSOLUTION
                                    };

                                    _generico.GravarGenerico(obj);
                                }
                            }
                        }
                    }
                    return objectList.Count();
                }
            }
            catch (Exception)
            {
                return 0;
            }


            return 0;
        }

        internal IList<Contratos> BuscarContratos()
        {
            return _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                     .Include(a=> a.Licencas)
                                      .Where(a => a.SituacaoContrato == "RENOVADO" || a.SituacaoContrato == "ATIVO")
                                     .ToList();
        }

        internal IList<Contratos> BuscarContratosCancelados()
        {
            return _context.Contratos.Include(a => a.FkPlataforma)
                                     .Include(a => a.FkCliente)
                                      .Include(a => a.Licencas)
                                     .Where(a => a.SituacaoContrato == "CANCELADO")
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
