using EventosVivos_Api.DTO;
using EventosVivos_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventosVivos_Api.Controllers
{
    /// <summary>
    /// Controlador para la gestión de reservas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;

        public ReservasController(IReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        /// <summary>
        /// Crea una nueva reserva para un evento.
        /// </summary>
        /// <param name="dto">Datos de la reserva a crear.</param>
        /// <returns>La reserva creada en caso de éxito, o un BadRequest con el error correspondiente.</returns>
        [HttpPost]
        public async Task<IActionResult> RealizarReserva([FromBody] CrearReservaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _reservaService.RealizarReservaAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Obtiene todas las reservas registradas con información detallada de eventos y clientes.
        /// </summary>
        /// <returns>La lista de reservas registradas.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _reservaService.GetReservasAsync();
            return Ok(reservas);
        }

        /// <summary>
        /// Obtiene las reservas asociadas a un correo electrónico de cliente específico con información detallada.
        /// </summary>
        /// <param name="email">El correo electrónico del cliente a consultar.</param>
        /// <returns>La lista de reservas del cliente en caso de éxito, o un BadRequest con el error correspondiente.</returns>
        [HttpGet("cliente/{email}")]
        public async Task<IActionResult> GetReservasByEmail(string email)
        {
            var result = await _reservaService.GetReservasByEmailAsync(email);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Cancela una reserva por su ID.
        /// </summary>
        /// <param name="id">Identificador único de la reserva.</param>
        /// <returns>La reserva modificada en caso de éxito, o un BadRequest con el error correspondiente.</returns>
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var result = await _reservaService.CancelarReservaAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Confirma el pago de una reserva y genera su código único.
        /// </summary>
        /// <param name="id">Identificador único de la reserva.</param>
        /// <returns>La reserva confirmada en caso de éxito, o un BadRequest con el error correspondiente.</returns>
        [HttpPost("{id}/confirmar-pago")]
        [Authorize]
        public async Task<IActionResult> ConfirmarPago(int id)
        {
            var result = await _reservaService.ConfirmarPagoAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(result.Value);
        }
    }
}
