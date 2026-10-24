using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class InicializarModel : PageModel
{
    private readonly ApiCliente api;

    public InicializarModel(ApiCliente api)
    {
        this.api = api;
    }

    public string? Mensaje { get; private set; }
    public string? Error { get; private set; }

    public void OnGet()
    {
    }

    public async Task OnPostAsync()
    {
        ResultadoApi<RespuestaMensaje> respuesta = await api.Inicializar();
        Mensaje = respuesta.Datos?.Mensaje;
        Error = respuesta.Error;
    }
}
