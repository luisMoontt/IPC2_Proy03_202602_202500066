using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class IndexModel : PageModel
{
    private readonly ApiCliente api;

    public IndexModel(ApiCliente api)
    {
        this.api = api;
    }

    public bool ApiConectada { get; private set; }
    public TotalesSistema Totales { get; private set; } = new TotalesSistema();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        ResultadoApi<RespuestaEstado> estado = await api.Estado();
        ApiConectada = estado.Exito;
        Error = estado.Error;
        if (estado.Datos != null)
            Totales = estado.Datos.Totales;
    }
}
