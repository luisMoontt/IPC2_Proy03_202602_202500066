using Microsoft.AspNetCore.Mvc;
using SAT_ControlDTE.DataBase;

namespace SAT_ControlDTE.Controllers
{
    [ApiController]
    [Route("api")]
    public class SistemaController : ControllerBase
    {
        private readonly ConexionBD conexionBD;
        private readonly ConsultaService consultaService;

        public SistemaController(ConexionBD conexionBD, ConsultaService consultaService)
        {
            this.conexionBD = conexionBD;
            this.consultaService = consultaService;
        }

        // POST /api/inicializar  -> borra todos los datos (RTU, facturas, errores y correlativos)
        [HttpPost("inicializar")]
        public IActionResult Inicializar()
        {
            conexionBD.Inicializar();
            return Ok(new { mensaje = "Sistema inicializado. No hay datos almacenados." });
        }

        // GET /api/estado  -> para probar que la API responde; incluye los totales almacenados
        [HttpGet("estado")]
        public IActionResult Estado()
        {
            return Ok(new { estado = "API SAT funcionando", fecha = DateTime.Now, totales = consultaService.Totales() });
        }
    }
}