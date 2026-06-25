using EventosVivos_Api.DTO;
using EventosVivos_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoEventosController : ControllerBase
    {
        private readonly ITipoEventoService _tipoEventoService;

        public TipoEventosController(ITipoEventoService tipoEventoService)
        {
            _tipoEventoService = tipoEventoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoEventoDto>>> GetAll()
        {
            var tipoEventos = await _tipoEventoService.GetAll();
            return Ok(tipoEventos);
        }
    }
}
