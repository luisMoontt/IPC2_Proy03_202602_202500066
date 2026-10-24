using System.Text.Json.Serialization;

namespace Sat.Web;

// Clases con la misma forma que los JSON que responde Sat.Api.
// ApiCliente las llena con JsonSerializer (sin importar mayusculas/minusculas).

public class RespuestaRtu
{
    [JsonPropertyName("CONTRIBUYENTES_NUEVOS")] public int Nuevos { get; set; }
    [JsonPropertyName("CONTRIBUYENTES_ACTUALIZADOS")] public int Actualizados { get; set; }
    [JsonPropertyName("NITS_INVALIDOS")] public int Invalidos { get; set; }
}

public class RespuestaAutorizaciones
{
    public ResultadoAutorizaciones ResultadoAutorizaciones { get; set; } = new ResultadoAutorizaciones();
}

public class ResultadoAutorizaciones
{
    public int CantidadAutorizacionesAprobadas { get; set; }
    public int CantidadAutorizacionesRechazadasPorNitEmisorInexistente { get; set; }
    public int CantidadAutorizacionesRechazadasPorNitReceptorInexistente { get; set; }
    public int CantidadAutorizacionesRechazadasPorNitEmisorInvalido { get; set; }
    public int CantidadAutorizacionesRechazadasPorNitReceptorInvalido { get; set; }
    public int CantidadAutorizacionesRechazadasPorIvaMalCalculado { get; set; }
    public int CantidadAutorizacionesRechazadasPorTotalMalCalculado { get; set; }
    public int CantidadAutorizacionesRechazadasPorReferenciaDuplicada { get; set; }
    public int CantidadAutorizacionesRechazadas { get; set; }
    public int CantidadDescartadasPorFormato { get; set; }
    public List<DetalleAprobacion> DetalleAprobaciones { get; set; } = new List<DetalleAprobacion>();
    public List<DetalleRechazo> DetalleRechazos { get; set; } = new List<DetalleRechazo>();
}

public class DetalleAprobacion
{
    public string Referencia { get; set; } = "";
    public long CodigoAprobacion { get; set; }
}

public class DetalleRechazo
{
    public string Referencia { get; set; } = "";
    public List<string> Errores { get; set; } = new List<string>();
}

public class RespuestaMensaje
{
    public string Mensaje { get; set; } = "";
}

public class RespuestaEstado
{
    public string Estado { get; set; } = "";
    public TotalesSistema Totales { get; set; } = new TotalesSistema();
}

public class TotalesSistema
{
    public int Contribuyentes { get; set; }
    public int FacturasAutorizadas { get; set; }
    public int FacturasRechazadas { get; set; }
    public decimal MontoAutorizado { get; set; }
}

public class ConsultaContribuyente
{
    public string Nit { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public List<TotalPorFecha> Fechas { get; set; } = new List<TotalPorFecha>();
    public int CantidadFacturas { get; set; }
    public decimal MontoTotal { get; set; }
    public List<FacturaDetalle> Facturas { get; set; } = new List<FacturaDetalle>();
}

public class TotalPorFecha
{
    public string Fecha { get; set; } = "";
    public int CantidadFacturas { get; set; }
    public decimal MontoTotal { get; set; }
}

public class FacturaDetalle
{
    public string Referencia { get; set; } = "";
    public string Fecha { get; set; } = "";
    public string NitEmisor { get; set; } = "";
    public string NitReceptor { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public long CodigoAprobacion { get; set; }
}

public class ResumenIva
{
    public string Fecha { get; set; } = "";
    public List<IvaContribuyente> Contribuyentes { get; set; } = new List<IvaContribuyente>();
}

public class IvaContribuyente
{
    public string Nit { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal IvaEmitido { get; set; }
    public decimal IvaRecibido { get; set; }
}

public class ResumenRango
{
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public List<RangoPorFecha> Fechas { get; set; } = new List<RangoPorFecha>();
    public decimal TotalGeneral { get; set; }
    public decimal TotalSinIva { get; set; }
}

public class RangoPorFecha
{
    public string Fecha { get; set; } = "";
    public int CantidadFacturas { get; set; }
    public decimal Total { get; set; }
    public decimal SinIva { get; set; }
}

public class ReporteErrores
{
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public List<ErroresPorFecha> Fechas { get; set; } = new List<ErroresPorFecha>();
}

public class ErroresPorFecha
{
    public string Fecha { get; set; } = "";
    public int CantidadFacturasRecibidas { get; set; }
    public ConteoErrores Errores { get; set; } = new ConteoErrores();
    public int CantidadFacturasSinErrores { get; set; }
    public int CantidadEmisores { get; set; }
    public int CantidadReceptores { get; set; }
}

public class ConteoErrores
{
    public int NitEmisorInvalido { get; set; }
    public int NitReceptorInvalido { get; set; }
    public int NitEmisorInexistente { get; set; }
    public int NitReceptorInexistente { get; set; }
    public int IvaMalCalculado { get; set; }
    public int TotalMalCalculado { get; set; }
    public int ReferenciaDuplicada { get; set; }
}
