using Microsoft.AspNetCore.Mvc;

namespace SAT_ControlDTE.Controllers
{
    [ApiController]
    [Route("api/autorizaciones")]
    public class AutorizacionController : ControllerBase
    {
        private readonly LectorMensajes lector;
        private readonly AutorizacionService autorizacionService;

        public AutorizacionController(LectorMensajes lector, AutorizacionService autorizacionService)
        {
            this.lector = lector;
            this.autorizacionService = autorizacionService;
        }

        [HttpPost]
        public async Task<IActionResult> Autorizar()
        {
            using var cuerpo = new StreamReader(Request.Body);
            string json = await cuerpo.ReadToEndAsync();

            try
            {
                LecturaFacturas lectura = lector.LeerFacturas(json);
                return Ok(autorizacionService.Procesar(lectura).CrearRespuesta());
            }
            catch (MensajeInvalidoException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}