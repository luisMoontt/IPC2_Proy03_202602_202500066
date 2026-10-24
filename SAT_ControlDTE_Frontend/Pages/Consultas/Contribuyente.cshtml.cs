using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class ContribuyenteModel : PageModel
{
    private readonly ApiCliente api;

    public ContribuyenteModel(ApiCliente api)
    {
        this.api = api;
    }

    // SupportsGet: el formulario usa GET, asi la consulta queda en la URL
    [BindProperty(SupportsGet = true)] public string? Nit { get; set; }
    [BindProperty(SupportsGet = true)] public string Tipo { get; set; } = "emitidas";
    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [BindProperty(SupportsGet = true), DataType(DataType.Date)] public DateTime? Hasta { get; set; }

    public ConsultaContribuyente? Resultado { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        // Primera vez que se abre la pagina: solo se muestra el formulario
        if (Nit == null && Desde == null && Hasta == null)
            return;

        if (string.IsNullOrWhiteSpace(Nit) || Desde == null || Hasta == null)
        {
            Error = "Ingrese el NIT y el rango de fechas.";
            return;
        }

        ResultadoApi<ConsultaContribuyente> respuesta = await api.ConsultarContribuyente(Nit, Tipo, Desde.Value, Hasta.Value);
        Resultado = respuesta.Datos;
        Error = respuesta.Error;
    }
}
