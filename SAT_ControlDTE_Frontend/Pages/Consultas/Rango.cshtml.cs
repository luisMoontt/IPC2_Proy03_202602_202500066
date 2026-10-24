using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class RangoModel : PageModel
{
    private readonly ApiCliente api;

    public RangoModel(ApiCliente api)
    {
        this.api = api;
    }

    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Hasta { get; set; }

    // "total" = valores con IVA, "siniva" = solo el VALOR
    [BindProperty(SupportsGet = true)] public string Valores { get; set; } = "total";

    public ResumenRango? Resultado { get; private set; }
    public string? Error { get; private set; }

    public bool ConIva => Valores != "siniva";
    public string NombreValores => ConIva ? "Total (con IVA)" : "Sin IVA";

    // Valor que se muestra por fecha segun lo que eligio el usuario
    public decimal ValorDe(RangoPorFecha fila) => ConIva ? fila.Total : fila.SinIva;
    public decimal ValorGeneral => Resultado == null ? 0 : (ConIva ? Resultado.TotalGeneral : Resultado.TotalSinIva);

    public async Task OnGetAsync()
    {
        if (Desde == null && Hasta == null)
            return;

        if (Desde == null || Hasta == null)
        {
            Error = "Completa las dos fechas para continuar.";
            return;
        }

        ResultadoApi<ResumenRango> respuesta = await api.ResumenRango(Desde.Value, Hasta.Value);
        Resultado = respuesta.Datos;
        Error = respuesta.Error;
    }
}
