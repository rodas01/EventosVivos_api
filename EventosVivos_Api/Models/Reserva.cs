using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("reservas")]
    public class Reserva
    {
        [Key]
        [Column("reserva_id")]
        public int ReservaId { get; set; }
        
        [Column("codigo_reserva")]
        public string? CodigoReserva { get; set; }

        [Required]
        [Column("id_cliente")]
        [ForeignKey("Cliente")]
        public Guid ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        [Column("id_evento")]
        [ForeignKey("Evento")]
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        [Required]
        [Column("fecha_reserva")]
        public DateTime FechaReserva { get; set; }

        [Required]
        [Column("cantidad_entradas")]
        public int CantidadEntradas { get; set; }

        [Required]
        [Column("precio_reserva")]
        public float PrecioReserva { get; set; }

        [Column("fecha_modificacion")]
        public DateTime? FechaModificacion { get; set; }

        [Required]
        [StringLength(20)]
        [Column("id_estado_reserva")]
        [ForeignKey("EstadoReserva")]
        public required string EstadoReservaId { get; set; }
        public EstadoReserva? EstadoReserva { get; set; }
    }
}
