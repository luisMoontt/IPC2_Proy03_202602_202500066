using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Sat.Web;

// Resultado de una llamada a la API: los datos si salio bien, o el mensaje de error
public class ResultadoApi<T>
{
    public bool Exito { get; init; }
    public T? Datos { get; init; }
    public string? Error { get; init; }
    public string JsonRespuesta { get; init; } = "";
}

// Unica clase del frontend que conoce las rutas de Sat.Api
public class ApiCliente
{
    private static readonly JsonSerializerOptions OpcionesLectura =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private static readonly JsonSerializerOptions OpcionesFormato =
        new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly HttpClient http;

    public ApiCliente(HttpClient http)
    {
        this.http = http;
    }

    public Task<ResultadoApi<RespuestaEstado>> Estado() =>
        Get<RespuestaEstado>("api/estado");

    public Task<ResultadoApi<RespuestaRtu>> EnviarRtu(string json) =>
        Post<RespuestaRtu>("api/rtu", json);

    public Task<ResultadoApi<RespuestaAutorizaciones>> EnviarAutorizaciones(string json) =>
        Post<RespuestaAutorizaciones>("api/autorizaciones", json);

    public Task<ResultadoApi<RespuestaMensaje>> Inicializar() =>
        Post<RespuestaMensaje>("api/inicializar", "");

    public Task<ResultadoApi<ConsultaContribuyente>> ConsultarContribuyente(string nit, string tipo, DateTime desde, DateTime hasta) =>
        Get<ConsultaContribuyente>($"api/consultas/contribuyente?nit={Uri.EscapeDataString(nit)}&tipo={tipo}&desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<ResultadoApi<ResumenIva>> ResumenIva(DateTime fecha) =>
        Get<ResumenIva>($"api/consultas/iva?fecha={Fecha(fecha)}");

    public Task<ResultadoApi<ResumenRango>> ResumenRango(DateTime desde, DateTime hasta) =>
        Get<ResumenRango>($"api/consultas/rango?desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<ResultadoApi<ReporteErrores>> ReporteErrores(DateTime desde, DateTime hasta) =>
        Get<ReporteErrores>($"api/consultas/errores?desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    private static string Fecha(DateTime fecha) => fecha.ToString("yyyy-MM-dd");

    private Task<ResultadoApi<T>> Get<T>(string ruta) =>
        Enviar<T>(new HttpRequestMessage(HttpMethod.Get, ruta));

    private Task<ResultadoApi<T>> Post<T>(string ruta, string json) =>
        Enviar<T>(new HttpRequestMessage(HttpMethod.Post, ruta)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

    private async Task<ResultadoApi<T>> Enviar<T>(HttpRequestMessage peticion)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await http.SendAsync(peticion);
        }
        catch (HttpRequestException)
        {
            return new ResultadoApi<T>
            {
                Error = $"No se pudo conectar con la API en {http.BaseAddress}. Revise que Sat.Api este ejecutandose."
            };
        }

        string cuerpo = await respuesta.Content.ReadAsStringAsync();

        if (!respuesta.IsSuccessStatusCode)
            return new ResultadoApi<T> { Error = LeerError(cuerpo, respuesta.StatusCode), JsonRespuesta = Formatear(cuerpo) };

        try
        {
            return new ResultadoApi<T>
            {
                Exito = true,
                Datos = JsonSerializer.Deserialize<T>(cuerpo, OpcionesLectura),
                JsonRespuesta = Formatear(cuerpo)
            };
        }
        catch (JsonException)
        {
            return new ResultadoApi<T> { Error = "La API respondio un JSON con un formato inesperado.", JsonRespuesta = cuerpo };
        }
    }

    // La API manda los errores como { "error": "..." }
    private static string LeerError(string cuerpo, HttpStatusCode codigo)
    {
        try
        {
            using JsonDocument documento = JsonDocument.Parse(cuerpo);
            if (documento.RootElement.TryGetProperty("error", out JsonElement error))
                return error.GetString() ?? "";
        }
        catch (JsonException) { }

        return $"La API respondio con el codigo {(int)codigo}.";
    }

    private static string Formatear(string json)
    {
        try
        {
            using JsonDocument documento = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(documento.RootElement, OpcionesFormato);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
