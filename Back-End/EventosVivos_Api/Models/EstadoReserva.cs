using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("estados_reservas")]
    public class EstadoReserva
    {
        [Key]
        [StringLength(20)]
        [Column("estado_reserva_id")]
        public required string EstadoReservaId { get; set; }

        [Column("descripcion")]
        public required string Descripcion { get; set; }
    }
}
