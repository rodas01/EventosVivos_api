using System;

namespace EventosVivos_Api.DTO
{
    public class FiltrosEventoDto
    {
        public string? TipoEvento { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? VenueId { get; set; }
        public string? Estado { get; set; }
        public string? Titulo { get; set; }
    }
}
