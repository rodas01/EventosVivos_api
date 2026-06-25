using EventosVivos_Api.DTO;
using EventosVivos_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _eventoService;

        public EventosController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventos([FromQuery] FiltrosEventoDto filtrosDto)
        {
            var eventos = await _eventoService.GetEventosAsync(filtrosDto);
            return Ok(eventos);
        }

        [HttpPost("crear-evento")]
        [Authorize]
        public async Task<IActionResult> CrearEvento([FromBody] CrearEventoDto eventoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _eventoService.CrearEventoAsync(eventoDto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }

        [HttpGet("reporte")]
        [Authorize]
        public async Task<IActionResult> GetReporteEventos()
        {
            var reporte = await _eventoService.GetReporteEventosAsync();
            return Ok(reporte);
        }
    }
}
