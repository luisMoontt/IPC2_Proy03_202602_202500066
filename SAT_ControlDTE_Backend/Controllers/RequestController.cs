using Microsoft.AspNetCore.Mvc;
using SAT_ControlDTE.Services;

namespace SAT_ControlDTE.Controllers
{
    [ApiController]
    [Route("api/consultas")]
    public class ConsultasController : ControllerBase
    {
        private readonly ConsultaService consultaService;
        private readonly NitService nitService;

        public ConsultasController(ConsultaService consultaService, NitService nitService)
        {
            this.consultaService = consultaService;
            this.nitService = nitService;
        }

        // GET /api/consultas/contribuyente?nit=999999K&tipo=emitidas&desde=2025-03-01&hasta=2025-03-02
        [HttpGet("contribuyente")]
        public IActionResult Contribuyente(string? nit, string? tipo, string? desde, string? hasta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nit))
                    throw new MensajeInvalidoException("Debe enviar el parametro 'nit'.");

                string tipoNormalizado = (tipo ?? "").Trim().ToLowerInvariant();
                if (tipoNormalizado != "emitidas" && tipoNormalizado != "recibidas")
                    throw new MensajeInvalidoException("El parametro 'tipo' debe ser 'emitidas' o 'recibidas'.");

                var (inicio, fin) = LeerRango(desde, hasta);
                return Ok(consultaService.ConsultarContribuyente(
                    nitService.Normalizar(nit), tipoNormalizado == "emitidas", inicio, fin));
            }
            catch (MensajeInvalidoException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET /api/consultas/iva?fecha=2025-03-02
        [HttpGet("iva")]
        public IActionResult ResumenIva(string? fecha)
        {
            try
            {
                return Ok(consultaService.ResumenIva(LectorMensajes.LeerFechaConsulta(fecha, "fecha")));
            }
            catch (MensajeInvalidoException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET /api/consultas/rango?desde=2025-03-01&hasta=2025-03-02
        [HttpGet("rango")]
        public IActionResult ResumenRango(string? desde, string? hasta)
        {
            try
            {
                var (inicio, fin) = LeerRango(desde, hasta);
                return Ok(consultaService.ResumenRango(inicio, fin));
            }
            catch (MensajeInvalidoException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET /api/consultas/errores?desde=2025-04-01&hasta=2025-04-07
        [HttpGet("errores")]
        public IActionResult ReporteErrores(string? desde, string? hasta)
        {
            try
            {
                var (inicio, fin) = LeerRango(desde, hasta);
                return Ok(consultaService.ReporteErrores(inicio, fin));
            }
            catch (MensajeInvalidoException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private static (DateTime, DateTime) LeerRango(string? desde, string? hasta)
        {
            DateTime inicio = LectorMensajes.LeerFechaConsulta(desde, "desde");
            DateTime fin = LectorMensajes.LeerFechaConsulta(hasta, "hasta");
            if (inicio > fin)
                throw new MensajeInvalidoException("La fecha 'desde' no puede ser mayor que 'hasta'.");
            return (inicio, fin);
        }
    }
}