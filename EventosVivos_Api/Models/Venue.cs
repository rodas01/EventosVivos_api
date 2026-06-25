using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("venues")]
    public class Venue
    {
        [Key]
        [Column("venue_id")]
        public int VenueId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public required string Nombre { get; set; }

        [Required]
        [Column("capacidad_maxima")]
        public int CapacidadMaxima { get; set; }

        [Required]
        [StringLength(255)]
        [Column("ubicacion")]
        public required string Ubicacion { get; set; }
    }
}
