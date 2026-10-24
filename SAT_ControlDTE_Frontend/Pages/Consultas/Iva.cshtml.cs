using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class IvaModel : PageModel
{
    private readonly ApiCliente api;

    public IvaModel(ApiCliente api)
    {
        this.api = api;
    }

    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Fecha { get; set; }

    public ResumenIva? Resultado { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        if (Fecha == null)
            return;

        ResultadoApi<ResumenIva> respuesta = await api.ResumenIva(Fecha.Value);
        Resultado = respuesta.Datos;
        Error = respuesta.Error;
    }
}
