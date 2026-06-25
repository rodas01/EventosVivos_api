using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("eventos")]
    public class Evento
    {
        [Key]
        [Column("evento_id")]
        public int EventoId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre_evento")]
        public required string NombreEvento { get; set; }

        [Required]
        [Column("descripcion", TypeName = "text")]
        public required string Descripcion { get; set; }

        [Required]
        [Column("id_venues")]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        [Required]
        [Column("capacidad")]
        public int Capacidad { get; set; }

        [Required]
        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Required]
        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Required]
        [Column("precio")]
        public float Precio { get; set; }

        [Required]
        [StringLength(20)]
        [Column("id_tipo_evento")]
        [ForeignKey("TipoEvento")]
        public required string TipoEventoId { get; set; }
        public TipoEvento? TipoEvento { get; set; }


        [Required]
        [StringLength(20)]
        [Column("id_estado_evento")]
        [ForeignKey("EstadoEvento")]
        public required string EstadoEventoId { get; set; }
        public EstadoEvento? EstadoEvento { get; set; }
    }
}
