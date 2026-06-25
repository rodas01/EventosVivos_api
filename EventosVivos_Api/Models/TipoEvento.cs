using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("tipos_eventos")]
    public class TipoEvento
    {
        [Key]
        [StringLength(20)]
        [Column("tipo_evento_id")]
        public required string TipoEventoId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("descripcion")]
        public required string Descripcion { get; set; }
    }
}
