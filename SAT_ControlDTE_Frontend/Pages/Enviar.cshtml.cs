using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sat.Web;

public class EnviarModel : PageModel
{
    private readonly ApiCliente api;

    public EnviarModel(ApiCliente api)
    {
        this.api = api;
    }

    [BindProperty]
    public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();

    public List<ResultadoEnvio> Resultados { get; } = new List<ResultadoEnvio>();
    public string? Error { get; private set; }

    public void OnGet()
    {
    }

    public async Task OnPostAsync()
    {
        if (Archivos.Count == 0)
        {
            Error = "Seleccione al menos un archivo .json.";
            return;
        }

        var mensajes = new List<MensajeEntrada>();
        foreach (IFormFile archivo in Archivos)
        {
            if (!archivo.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Resultados.Add(new ResultadoEnvio { Archivo = archivo.FileName, Error = "Solo se aceptan archivos con extension .json." });
                continue;
            }

            using var lector = new StreamReader(archivo.OpenReadStream());
            mensajes.Add(MensajeEntrada.Crear(archivo.FileName, await lector.ReadToEndAsync()));
        }

        // Primero los RTU y luego las autorizaciones; dentro de cada tipo, por nombre de archivo
        foreach (MensajeEntrada mensaje in mensajes.OrderBy(m => m.Prioridad).ThenBy(m => m.Archivo))
            Resultados.Add(await mensaje.EnviarAsync(api));
    }
}
