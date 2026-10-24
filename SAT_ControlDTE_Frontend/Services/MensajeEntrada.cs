using System.Text.Json;

namespace Sat.Web;

// Resultado de enviar un archivo a la API, para mostrarlo en la pagina Enviar
public class ResultadoEnvio
{
    public string Archivo { get; init; } = "";
    public string Tipo { get; init; } = "";
    public bool Exito { get; init; }
    public string? Error { get; init; }
    public string JsonRespuesta { get; init; } = "";

    // Solo una de las dos tiene datos, segun el tipo de mensaje
    public RespuestaRtu? Rtu { get; init; }
    public ResultadoAutorizaciones? Autorizaciones { get; init; }
}

// Un archivo .json elegido en la pagina Enviar.
// Cada hija sabe a que endpoint mandarse; la pagina solo llama EnviarAsync sin preguntar el tipo.
public abstract class MensajeEntrada
{
    protected MensajeEntrada(string archivo, string contenido)
    {
        Archivo = archivo;
        Contenido = contenido;
    }

    public string Archivo { get; }
    public string Contenido { get; }

    public abstract string Tipo { get; }

    // Orden de envio: el RTU va primero para que los NIT existan cuando lleguen las facturas
    public abstract int Prioridad { get; }

    public abstract Task<ResultadoEnvio> EnviarAsync(ApiCliente api);

    // Decide el tipo una sola vez, al leer el archivo: si algun elemento trae "NIT" es RTU.
    // Cualquier otro contenido se manda como autorizaciones; si esta mal, la API responde el error.
    public static MensajeEntrada Crear(string archivo, string contenido)
    {
        if (TieneLlave(contenido, "NIT"))
            return new MensajeRtu(archivo, contenido);
        return new MensajeAutorizaciones(archivo, contenido);
    }

    private static bool TieneLlave(string json, string llave)
    {
        try
        {
            using JsonDocument documento = JsonDocument.Parse(json);
            JsonElement raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.Object)
                return raiz.TryGetProperty(llave, out _);

            return raiz.ValueKind == JsonValueKind.Array &&
                   raiz.EnumerateArray().Any(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(llave, out _));
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public class MensajeRtu : MensajeEntrada
{
    public MensajeRtu(string archivo, string contenido) : base(archivo, contenido) { }

    public override string Tipo => "Actualizacion de RTU";
    public override int Prioridad => 1;

    public override async Task<ResultadoEnvio> EnviarAsync(ApiCliente api)
    {
        ResultadoApi<RespuestaRtu> respuesta = await api.EnviarRtu(Contenido);
        return new ResultadoEnvio
        {
            Archivo = Archivo,
            Tipo = Tipo,
            Exito = respuesta.Exito,
            Error = respuesta.Error,
            JsonRespuesta = respuesta.JsonRespuesta,
            Rtu = respuesta.Datos
        };
    }
}

public class MensajeAutorizaciones : MensajeEntrada
{
    public MensajeAutorizaciones(string archivo, string contenido) : base(archivo, contenido) { }

    public override string Tipo => "Solicitudes de autorizacion";
    public override int Prioridad => 2;

    public override async Task<ResultadoEnvio> EnviarAsync(ApiCliente api)
    {
        ResultadoApi<RespuestaAutorizaciones> respuesta = await api.EnviarAutorizaciones(Contenido);
        return new ResultadoEnvio
        {
            Archivo = Archivo,
            Tipo = Tipo,
            Exito = respuesta.Exito,
            Error = respuesta.Error,
            JsonRespuesta = respuesta.JsonRespuesta,
            Autorizaciones = respuesta.Datos?.ResultadoAutorizaciones
        };
    }
}
