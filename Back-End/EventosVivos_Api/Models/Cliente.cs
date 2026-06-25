using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosVivos_Api.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("cliente_id")]
        public Guid ClienteId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("correo")]
        public required string Correo { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public required string Nombre { get; set; }
    }
}
