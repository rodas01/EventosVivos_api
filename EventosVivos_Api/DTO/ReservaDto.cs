using System;

namespace EventosVivos_Api.DTO
{
    /// <summary>
    /// DTO para transferir información detallada de una reserva.
    /// </summary>
    public class ReservaDto
    {
        /// <summary>
        /// Identificador único de la reserva.
        /// </summary>
        public int ReservaId { get; set; }

        /// <summary>
        /// Código único de la reserva.
        /// </summary>
        public string? CodigoReserva { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la reserva.
        /// </summary>
        public DateTime FechaReserva { get; set; }

        /// <summary>
        /// Cantidad de entradas reservadas.
        /// </summary>
        public int CantidadEntradas { get; set; }

        /// <summary>
        /// Precio total calculado de la reserva.
        /// </summary>
        public float PrecioReserva { get; set; }

        /// <summary>
        /// Identificador del estado de la reserva.
        /// </summary>
        public required string EstadoReservaId { get; set; }

        /// <summary>
        /// Nombre del cliente asociado a la reserva.
        /// </summary>
        public required string NombreCliente { get; set; }

        /// <summary>
        /// Correo electrónico del cliente asociado a la reserva.
        /// </summary>
        public required string CorreoCliente { get; set; }

        /// <summary>
        /// Título o nombre del evento reservado.
        /// </summary>
        public required string TituloEvento { get; set; }

        /// <summary>
        /// Fecha de inicio del evento reservado.
        /// </summary>
        public DateTime FechaEvento { get; set; }

        /// <summary>
        /// Nombre del venue o establecimiento donde se realizará el evento.
        /// </summary>
        public required string NombreVenue { get; set; }
    }
}
