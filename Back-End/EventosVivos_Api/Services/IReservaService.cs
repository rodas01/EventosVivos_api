using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Util;

namespace EventosVivos_Api.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de reservas.
    /// </summary>
    public interface IReservaService
    {
        /// <summary>
        /// Realiza una nueva reserva para un evento específico.
        /// </summary>
        /// <param name="dto">DTO con los datos necesarios para realizar la reserva.</param>
        /// <returns>Resultado indicando éxito o fracaso, conteniendo la reserva creada en caso de éxito.</returns>
        Task<Result<Reserva>> RealizarReservaAsync(CrearReservaDto dto);

        /// <summary>
        /// Obtiene todas las reservas registradas con información detallada de eventos y clientes.
        /// </summary>
        /// <returns>Colección de DTOs de reservas.</returns>
        Task<IEnumerable<ReservaDto>> GetReservasAsync();

        /// <summary>
        /// Obtiene las reservas asociadas a un correo electrónico de cliente específico con información detallada.
        /// </summary>
        /// <param name="email">El correo electrónico del cliente a consultar.</param>
        /// <returns>Resultado de la operación que contiene la lista de reservas en caso de éxito o un error si el formato del correo no es válido.</returns>
        Task<Result<IEnumerable<ReservaDto>>> GetReservasByEmailAsync(string email);

        /// <summary>
        /// Cancela una reserva existente según reglas de negocio.
        /// </summary>
        /// <param name="reservaId">El identificador de la reserva a cancelar.</param>
        /// <returns>Resultado indicando éxito o fracaso, conteniendo la reserva modificada en caso de éxito.</returns>
        Task<Result<Reserva>> CancelarReservaAsync(int reservaId);

        /// <summary>
        /// Confirma el pago de una reserva existente, asignándole un código único.
        /// </summary>
        /// <param name="reservaId">El identificador de la reserva a confirmar.</param>
        /// <returns>Resultado indicando éxito o fracaso, conteniendo la reserva modificada en caso de éxito.</returns>
        Task<Result<Reserva>> ConfirmarPagoAsync(int reservaId);
    }
}
