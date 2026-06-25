using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("estados_eventos")]
    public class EstadoEvento
    {
        [Key]
        [StringLength(20)]
        [Column("estado_evento_id")]
        public required string EstadoEventoId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("descripcion")]
        public required string Descripcion { get; set; }
    }
}
