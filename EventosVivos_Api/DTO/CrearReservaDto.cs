using System.ComponentModel.DataAnnotations;

namespace EventosVivos_Api.DTO
{
    /// <summary>
    /// DTO para la creación de una nueva reserva.
    /// </summary>
    public class CrearReservaDto
    {
        /// <summary>
        /// Identificador único del evento para el cual se realiza la reserva.
        /// </summary>
        [Required(ErrorMessage = "El identificador del evento es requerido.")]
        public int EventoId { get; set; }

        /// <summary>
        /// Cantidad de entradas a reservar.
        /// </summary>
        [Required(ErrorMessage = "La cantidad de entradas es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de entradas debe ser al menos 1.")]
        public int Cantidad { get; set; }

        /// <summary>
        /// Nombre del comprador de la reserva.
        /// </summary>
        [Required(ErrorMessage = "El nombre del comprador es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre del comprador no puede superar los 100 caracteres.")]
        public required string NombreComprador { get; set; }

        /// <summary>
        /// Correo electrónico del comprador de la reserva.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico del comprador es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico del comprador no es válido.")]
        [StringLength(100, ErrorMessage = "El correo electrónico del comprador no puede superar los 100 caracteres.")]
        public required string EmailComprador { get; set; }
    }
}
