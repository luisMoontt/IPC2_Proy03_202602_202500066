using Microsoft.AspNetCore.Mvc;

namespace SAT_ControlDTE.Controllers
{
    [ApiController]
    [Route("api/rtu")]
    public class RtuController : ControllerBase
    {
        private readonly LectorMensajes lector;
        private readonly RtuService rtuService;

        public RtuController(LectorMensajes lector, RtuService rtuService)
        {
            this.lector = lector;
            this.rtuService = rtuService;
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar()
        {
            using var cuerpo = new StreamReader(Request.Body);
            string json = await cuerpo.ReadToEndAsync();

            try
            {
                ResultadoRtu resultado = rtuService.Actualizar(lector.LeerContribuyentes(json));
                return Ok(new
                {
                    CONTRIBUYENTES_NUEVOS = resultado.Nuevos,
                    CONTRIBUYENTES_ACTUALIZADOS = resultado.Actualizados,
                    NITS_INVALIDOS = resultado.Invalidos
                });
            }
            catch (MensajeInvalidoException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Listar()
        {
            return Ok(rtuService.Listar().Select(c => new { NIT = c.Nit, nombre = c.Nombre }));
        }
    }
}