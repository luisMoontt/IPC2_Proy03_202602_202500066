using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class ErroresModel : PageModel
{
    private readonly ApiCliente api;

    public ErroresModel(ApiCliente api)
    {
        this.api = api;
    }

    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Hasta { get; set; }

    public ReporteErrores? Resultado { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        if (Desde == null && Hasta == null)
            return;

        if (Desde == null || Hasta == null)
        {
            Error = "Ingrese las dos fechas del rango.";
            return;
        }

        ResultadoApi<ReporteErrores> respuesta = await api.ReporteErrores(Desde.Value, Hasta.Value);
        Resultado = respuesta.Datos;
        Error = respuesta.Error;
    }
}
