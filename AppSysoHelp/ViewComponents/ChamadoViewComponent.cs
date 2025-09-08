using AppSysoHelp.Models.Dto;
using AppSysoHelp.Service;
using Microsoft.AspNetCore.Mvc;

namespace AppSysoHelp.ViewComponents
{
    public class ChamadoViewComponent : ViewComponent
    {

        private readonly ServiceGenerico _serviceGenerico;

        public ChamadoViewComponent(ServiceGenerico serviceGenerico)
        {
            _serviceGenerico = serviceGenerico;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var chamados = await _serviceGenerico.BuscarChamadosPreso();
                return View(chamados);
            }
            catch (Exception ex)
            {
                // Log do erro
                var listaVazia = new List<AtendimentoDto>();
                return View(listaVazia);
            }
        }
    }
}
