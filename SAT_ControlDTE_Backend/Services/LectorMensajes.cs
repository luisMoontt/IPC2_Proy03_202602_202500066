using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SAT_ControlDTE.Services
{
    public class MensajeInvalidoException : Exception
    {
        public MensajeInvalidoException(string mensaje) : base(mensaje) { }
    }

    public class LecturaFacturas
    {
        public LecturaFacturas(List<Factura> facturas, int descartadas)
        {
            Facturas = facturas;
            Descartadas = descartadas;
        }

        public List<Factura> Facturas { get; }
        public int Descartadas { get; }
    }

    // Convierte el texto JSON de los mensajes de entrada en objetos del modelo.
    public class LectorMensajes
    {
        // Busca "dd/mm/yyyy hh:mi" en cualquier parte del texto e ignora lo demas
        // ("Guatemala, ", " hrs.", etc.). Acepta dia, mes y hora de 1 o 2 digitos.
        private static readonly Regex RegexFecha =
            new Regex(@"\b(\d{1,2})/(\d{1,2})/(\d{4})\s+(\d{1,2}):(\d{2})\b");

        private readonly NitService nitService;

        public LectorMensajes(NitService nitService)
        {
            this.nitService = nitService;
        }

        public List<Contribuyente> LeerContribuyentes(string json)
        {
            using JsonDocument documento = AbrirArreglo(json, "NIT", "contribuyentes");

            var contribuyentes = new List<Contribuyente>();
            foreach (JsonElement elemento in documento.RootElement.EnumerateArray())
            {
                string nit = nitService.Normalizar(LeerTexto(elemento, "NIT"));
                string nombre = (LeerTexto(elemento, "nombre") ?? "").Trim();
                contribuyentes.Add(new Contribuyente(nit, nombre));
            }
            return contribuyentes;
        }

        public LecturaFacturas LeerFacturas(string json)
        {
            using JsonDocument documento = AbrirArreglo(json, "REFERENCIA", "solicitudes de autorizacion");

            var facturas = new List<Factura>();
            int descartadas = 0;
            foreach (JsonElement elemento in documento.RootElement.EnumerateArray())
            {
                Factura? factura = LeerFactura(elemento);
                if (factura == null)
                    descartadas++;
                else
                    facturas.Add(factura);
            }
            return new LecturaFacturas(facturas, descartadas);
        }

        // Devuelve null si a la factura le falta un dato o tiene un formato que no se puede leer.
        // Esas facturas se descartan: no reciben codigo ni cuentan como rechazadas.
        private Factura? LeerFactura(JsonElement elemento)
        {
            if (elemento.ValueKind != JsonValueKind.Object)
                return null;

            DateTime? fecha = ExtraerFecha(LeerTexto(elemento, "TIEMPO"));
            string referencia = (LeerTexto(elemento, "REFERENCIA") ?? "").Trim();
            string? emisor = LeerTexto(elemento, "NIT_EMISOR");
            string? receptor = LeerTexto(elemento, "NIT_RECEPTOR");
            decimal? valor = LeerDecimal(elemento, "VALOR");
            decimal? iva = LeerDecimal(elemento, "IVA");
            decimal? total = LeerDecimal(elemento, "TOTAL");

            if (fecha == null || referencia.Length == 0 || referencia.Length > 40)
                return null;
            if (emisor == null || receptor == null)
                return null;
            if (valor == null || iva == null || total == null || valor <= 0)
                return null;

            return new Factura(referencia, fecha.Value,
                nitService.Normalizar(emisor), nitService.Normalizar(receptor),
                valor.Value, iva.Value, total.Value);
        }

        public static DateTime? ExtraerFecha(string? tiempo)
        {
            if (tiempo == null)
                return null;

            Match coincidencia = RegexFecha.Match(tiempo);
            if (!coincidencia.Success)
                return null;

            int dia = int.Parse(coincidencia.Groups[1].Value);
            int mes = int.Parse(coincidencia.Groups[2].Value);
            int anio = int.Parse(coincidencia.Groups[3].Value);
            int hora = int.Parse(coincidencia.Groups[4].Value);
            int minuto = int.Parse(coincidencia.Groups[5].Value);

            // El regex solo revisa la forma; aqui se revisa que la fecha exista (29/02/2025 no existe)
            if (anio < 1 || mes < 1 || mes > 12 || hora > 23 || minuto > 59)
                return null;
            if (dia < 1 || dia > DateTime.DaysInMonth(anio, mes))
                return null;

            return new DateTime(anio, mes, dia, hora, minuto, 0);
        }

        // Fechas de los filtros de consulta: acepta yyyy-MM-dd (input date del navegador) o dd/MM/yyyy
        public static DateTime LeerFechaConsulta(string? texto, string nombreParametro)
        {
            string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" };
            if (DateTime.TryParseExact(texto?.Trim(), formatos, CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out DateTime fecha))
                return fecha;

            throw new MensajeInvalidoException(
                $"El parametro '{nombreParametro}' debe ser una fecha yyyy-MM-dd o dd/MM/yyyy.");
        }

        // Revisa que el mensaje sea un arreglo JSON no vacio y que al menos un elemento traiga la llave esperada
        private static JsonDocument AbrirArreglo(string json, string llaveEsperada, string contenido)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new MensajeInvalidoException("El mensaje esta vacio.");

            JsonDocument documento;
            try
            {
                documento = JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                throw new MensajeInvalidoException("El mensaje no es un JSON valido.");
            }

            JsonElement raiz = documento.RootElement;
            string? error = null;

            if (raiz.ValueKind != JsonValueKind.Array)
                error = "El mensaje debe ser un arreglo JSON: [ {...}, {...} ].";
            else if (raiz.GetArrayLength() == 0)
                error = "El arreglo esta vacio.";
            else if (!raiz.EnumerateArray().Any(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(llaveEsperada, out _)))
                error = $"El mensaje no contiene {contenido} (ningun elemento tiene la llave \"{llaveEsperada}\").";

            if (error != null)
            {
                documento.Dispose();
                throw new MensajeInvalidoException(error);
            }
            return documento;
        }

        private static string? LeerTexto(JsonElement elemento, string llave)
        {
            if (elemento.ValueKind != JsonValueKind.Object || !elemento.TryGetProperty(llave, out JsonElement valor))
                return null;

            return valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Number => valor.GetRawText(),
                _ => null
            };
        }

        // Acepta el numero como numero JSON (100.00) o como texto ("100.00")
        private static decimal? LeerDecimal(JsonElement elemento, string llave)
        {
            if (!elemento.TryGetProperty(llave, out JsonElement valor))
                return null;

            if (valor.ValueKind == JsonValueKind.Number && valor.TryGetDecimal(out decimal numero))
                return numero;

            if (valor.ValueKind == JsonValueKind.String &&
                decimal.TryParse(valor.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out numero))
                return numero;

            return null;
        }
    }
}