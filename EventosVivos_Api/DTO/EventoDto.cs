using System;

namespace EventosVivos_Api.DTO
{
    public class EventoDto
    {
        public required string NombreEvento { get; set; }
        public required string Descripcion { get; set; }
        public required VenueDto Venue { get; set; }
        public int Capacidad { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public float Precio { get; set; }
        public required string TipoEventoId { get; set; }
        public required string EstadoEventoId { get; set; }
        public required bool SoldOut { get; set; }
    }

    public class VenueDto
    {
        public required string Nombre { get; set; }
    }
}
