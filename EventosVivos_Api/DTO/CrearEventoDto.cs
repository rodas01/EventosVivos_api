using System;
using System.ComponentModel.DataAnnotations;

namespace EventosVivos_Api.DTO
{
    /// <summary>
    /// DTO para la creación de un nuevo evento.
    /// </summary>
    public class CrearEventoDto
    {
        /// <summary>
        /// Título del evento.
        /// </summary>
        [Required(ErrorMessage = "El título es requerido.")]
        [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
        public required string Titulo { get; set; }

        /// <summary>
        /// Descripción del evento.
        /// </summary>
        [Required(ErrorMessage = "La descripción es requerida.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public required string Descripcion { get; set; }

        /// <summary>
        /// Identificador único del venue donde se realizará el evento.
        /// </summary>
        [Required(ErrorMessage = "El identificador del venue es requerido.")]
        public int IdVenue { get; set; }

        /// <summary>
        /// Capacidad máxima de asistentes permitida en el evento.
        /// </summary>
        [Required(ErrorMessage = "La capacidad máxima es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad máxima debe ser al menos 1.")]
        public int CapacidadMaxima { get; set; }

        /// <summary>
        /// Fecha y hora de inicio del evento.
        /// </summary>
        [Required(ErrorMessage = "La fecha y hora de inicio es requerida.")]
        public DateTime FechaHoraInicio { get; set; }

        /// <summary>
        /// Fecha y hora de finalización del evento.
        /// </summary>
        [Required(ErrorMessage = "La fecha y hora de fin es requerida.")]
        public DateTime FechaHoraFin { get; set; }

        /// <summary>
        /// Precio de la entrada para el evento.
        /// </summary>
        [Required(ErrorMessage = "El precio de la entrada es requerido.")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio de la entrada debe ser mayor o igual a 0.")]
        public decimal PrecioEntrada { get; set; }

        /// <summary>
        /// Identificador único del tipo de evento.
        /// </summary>
        [Required(ErrorMessage = "El identificador del tipo de evento es requerido.")]
        public required string TipoEventoId { get; set; }
    }
}
