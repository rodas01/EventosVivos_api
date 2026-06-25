using System;

namespace EventosVivos_Api.DTO
{
    public class ReporteEventoDto
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
        public int Ocupacion { get; set; }
        public int ReservasDisponibles { get; set; }
        public int ReservasVendidas { get; set; }
        public float TotalIngresos { get; set; }
    }
}
