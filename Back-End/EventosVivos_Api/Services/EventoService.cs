using EventosVivos_Api.Data;
using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Util;
using EventosVivos_Api.Util.Const;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos_Api.Services
{
    public class EventoService : IEventoService
    {
        private readonly ApplicationDbContext _context;

        public EventoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Evento>> CrearEventoAsync(CrearEventoDto eventoDto)
        {
            var validationResult = await ValidateEvent(eventoDto);
            if (!validationResult.IsSuccess)
            {
                return Result<Evento>.Failure(validationResult.Error!);
            }

            var evento = new Evento
            {
                NombreEvento = eventoDto.Titulo,
                Descripcion = eventoDto.Descripcion,
                VenueId = eventoDto.IdVenue,
                Capacidad = eventoDto.CapacidadMaxima,
                FechaInicio = eventoDto.FechaHoraInicio,
                FechaFin = eventoDto.FechaHoraFin,
                Precio = (float)eventoDto.PrecioEntrada,
                TipoEventoId = eventoDto.TipoEventoId,
                EstadoEventoId = EventStatusConstants.Activo
            };

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            return Result<Evento>.Success(evento);
        }

        public async Task<Result<EventoDto>> GetEventoByIdAsync(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null)
            {
                return Result<EventoDto>.Failure(MessageConstants.EventNotFoundError);
            }

            var totalReservas = await GetTotalReservationsAsync(evento.EventoId);
            var estadoCalculado = (evento.EstadoEventoId == EventStatusConstants.Activo && DateTime.UtcNow > evento.FechaFin) ? EventStatusConstants.Completado : evento.EstadoEventoId;

            var eventoDto = new EventoDto
            {
                EventoId = evento.EventoId,
                NombreEvento = evento.NombreEvento,
                Descripcion = evento.Descripcion,
                Venue = new VenueDto { Nombre = evento.Venue?.Nombre ?? string.Empty },
                Capacidad = evento.Capacidad,
                FechaInicio = evento.FechaInicio,
                FechaFin = evento.FechaFin,
                Precio = evento.Precio,
                TipoEventoId = evento.TipoEventoId,
                EstadoEventoId = estadoCalculado,
                SoldOut = totalReservas >= evento.Capacidad,
                EntradasDisponibles = evento.Capacidad - totalReservas
            };

            return Result<EventoDto>.Success(eventoDto);
        }

        public async Task<IEnumerable<EventoDto>> GetEventosAsync(FiltrosEventoDto filtrosDto)
        {
            var query = _context.Eventos.Include(e => e.Venue).AsQueryable();

            if (filtrosDto != null)
            {
                if (!string.IsNullOrEmpty(filtrosDto.TipoEvento))
                {
                    query = query.Where(e => e.TipoEventoId == filtrosDto.TipoEvento);
                }

                if (filtrosDto.FechaInicio.HasValue)
                {
                    query = query.Where(e => e.FechaInicio >= filtrosDto.FechaInicio.Value);
                }

                if (filtrosDto.FechaFin.HasValue)
                {
                    query = query.Where(e => e.FechaInicio <= filtrosDto.FechaFin.Value);
                }

                if (filtrosDto.VenueId.HasValue)
                {
                    query = query.Where(e => e.VenueId == filtrosDto.VenueId.Value);
                }

                if (!string.IsNullOrEmpty(filtrosDto.Estado))
                {
                    query = query.Where(e => e.EstadoEventoId == filtrosDto.Estado);
                }

                if (!string.IsNullOrEmpty(filtrosDto.Titulo))
                {
                    query = query.Where(e => e.NombreEvento.ToLower().Contains(filtrosDto.Titulo.ToLower()));
                }
            }

            var eventos = await query.ToListAsync();

            var eventosDto = new List<EventoDto>();
            foreach (var evento in eventos)
            {
                var totalReservas = await GetTotalReservationsAsync(evento.EventoId);
                var estadoCalculado = (evento.EstadoEventoId == EventStatusConstants.Activo && DateTime.UtcNow > evento.FechaFin) ? EventStatusConstants.Completado : evento.EstadoEventoId;

                eventosDto.Add(new EventoDto
                {
                    EventoId = evento.EventoId,
                    NombreEvento = evento.NombreEvento,
                    Descripcion = evento.Descripcion,
                    Venue = new VenueDto { Nombre = evento.Venue.Nombre },
                    Capacidad = evento.Capacidad,
                    FechaInicio = evento.FechaInicio,
                    FechaFin = evento.FechaFin,
                    Precio = evento.Precio,
                    TipoEventoId = evento.TipoEventoId,
                    EstadoEventoId = estadoCalculado,
                    SoldOut = totalReservas >= evento.Capacidad,
                    EntradasDisponibles = evento.Capacidad - totalReservas
                });
            }

            return eventosDto;
        }

        public async Task<IEnumerable<ReporteEventoDto>> GetReporteEventosAsync()
        {
            var eventos = await _context.Eventos.Include(e => e.Venue).ToListAsync();

            var reporteEventos = new List<ReporteEventoDto>();
            foreach (var evento in eventos)
            {
                var totalReservas = await GetTotalReservationsAsync(evento.EventoId);
                var reservasDisponibles = evento.Capacidad - totalReservas;

                var ocupacion = await _context.Reservas
                    .Where(r => r.EventoId == evento.EventoId && 
                                (r.EstadoReservaId == ReservationStatusConstants.Confirmada || 
                                 r.EstadoReservaId == ReservationStatusConstants.PagoPendiente))
                    .SumAsync(r => r.CantidadEntradas);

                var reservasVendidas = await _context.Reservas
                    .Where(r => r.EventoId == evento.EventoId && r.EstadoReservaId == ReservationStatusConstants.Confirmada)
                    .SumAsync(r => r.CantidadEntradas);

                var totalIngresos = await _context.Reservas
                    .Where(r => r.EventoId == evento.EventoId && r.EstadoReservaId == ReservationStatusConstants.Confirmada)
                    .SumAsync(r => r.PrecioReserva);

                var estadoCalculado = (evento.EstadoEventoId == EventStatusConstants.Activo && DateTime.UtcNow > evento.FechaFin) ? EventStatusConstants.Completado : evento.EstadoEventoId;

                reporteEventos.Add(new ReporteEventoDto
                {
                    NombreEvento = evento.NombreEvento,
                    Descripcion = evento.Descripcion,
                    Venue = new VenueDto { Nombre = evento.Venue?.Nombre ?? string.Empty },
                    Capacidad = evento.Capacidad,
                    FechaInicio = evento.FechaInicio,
                    FechaFin = evento.FechaFin,
                    Precio = evento.Precio,
                    TipoEventoId = evento.TipoEventoId,
                    EstadoEventoId = estadoCalculado,
                    Ocupacion = ocupacion,
                    ReservasDisponibles = reservasDisponibles,
                    ReservasVendidas = reservasVendidas,
                    TotalIngresos = totalIngresos
                });
            }

            return reporteEventos;
        }

        private async Task<Result> ValidateEvent(CrearEventoDto eventoDto)
        {
            var dateResult = ValidateDateEvent(eventoDto);
            if (!dateResult.IsSuccess) return dateResult;

            var capacityResult = await ValidateEventCapacity(eventoDto);
            if (!capacityResult.IsSuccess) return capacityResult;

            var availabilityResult = await ValidateEventAvailability(eventoDto);
            if (!availabilityResult.IsSuccess) return availabilityResult;

            var typeResult = await ValidateEventType(eventoDto);
            if (!typeResult.IsSuccess) return typeResult;

            return Result.Success();
        }

        private Result ValidateDateEvent(CrearEventoDto eventoDto)
        {
            // Validar que la fecha de inicio es futura
            if (eventoDto.FechaHoraInicio <= DateTime.UtcNow)
            {
                return Result.Failure(MessageConstants.FutureStartDateError);
            }

            // Validar que la fecha de fin es posterior a la de inicio
            if (eventoDto.FechaHoraFin <= eventoDto.FechaHoraInicio)
            {
                return Result.Failure(MessageConstants.EndDateBeforeStartDateError);
            }

            // Validar la hora de inicio en fines de semana
            if (eventoDto.FechaHoraInicio.DayOfWeek == DayOfWeek.Saturday || eventoDto.FechaHoraInicio.DayOfWeek == DayOfWeek.Sunday)
            {
                if (eventoDto.FechaHoraInicio.Hour >= 22)
                {
                    return Result.Failure(MessageConstants.WeekendStartTimeError);
                }
            }

            return Result.Success();
        }

        private async Task<Result> ValidateEventCapacity(CrearEventoDto eventoDto)
        {
            // Validar capacidad del venue
            var venue = await _context.Venues.FindAsync(eventoDto.IdVenue);
            if (venue == null)
            {
                return Result.Failure(MessageConstants.VenueNotFoundError);
            }
            if (eventoDto.CapacidadMaxima > venue.CapacidadMaxima)
            {
                return Result.Failure(MessageConstants.CapacityExceededError);
            }
            return Result.Success();
        }

        private async Task<Result> ValidateEventAvailability(CrearEventoDto eventoDto)
        {
            // Validar superposición de eventos
            var overlappingEvent = await _context.Eventos
                .Where(e => e.VenueId == eventoDto.IdVenue &&
                            e.FechaInicio < eventoDto.FechaHoraFin &&
                            e.FechaFin > eventoDto.FechaHoraInicio)
                .FirstOrDefaultAsync();
            if (overlappingEvent != null)
            {
                return Result.Failure(MessageConstants.EventOverlapError);
            }
            return Result.Success();
        }

        private async Task<Result> ValidateEventType(CrearEventoDto eventoDto)
        {
            var tipoEvento = await _context.TiposEventos.FindAsync(eventoDto.TipoEventoId);
            if (tipoEvento == null)
            {
                return Result.Failure(MessageConstants.InvalidEventTypeError);
            }
            return Result.Success();
        }

        private async Task<int> GetTotalReservationsAsync(int eventId)
        {
            return await _context.Reservas
                .Where(r => r.EventoId == eventId && 
                            (r.EstadoReservaId == ReservationStatusConstants.Confirmada || 
                             r.EstadoReservaId == ReservationStatusConstants.PagoPendiente || 
                             r.EstadoReservaId == ReservationStatusConstants.Perdida))
                .SumAsync(r => r.CantidadEntradas);
        }
    }
}
