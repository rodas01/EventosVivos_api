using EventosVivos_Api.Data;
using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Util;
using EventosVivos_Api.Util.Const;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventosVivos_Api.Services
{
    /// <summary>
    /// Implementación del servicio de reservas.
    /// </summary>
    public class ReservaService : IReservaService
    {
        private readonly ApplicationDbContext _context;

        public ReservaService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Realiza una nueva reserva para un evento específico.
        /// </summary>
        /// <param name="dto">DTO con los datos necesarios para realizar la reserva.</param>
        /// <returns>Resultado indicando éxito o fracaso, conteniendo la reserva creada en caso de éxito.</returns>
        public async Task<Result<Reserva>> RealizarReservaAsync(CrearReservaDto dto)
        {
            var now = DateTime.UtcNow;
            var evento = await _context.Eventos.FindAsync(dto.EventoId);

            var validationResult = await ValidateReservationAsync(dto, evento, now);
            if (!validationResult.IsSuccess)
            {
                return Result<Reserva>.Failure(validationResult.Error!);
            }

            // Buscar o registrar al cliente por correo (ignorando mayúsculas/minúsculas)
            var cliente = await GetOrCreateClienteAsync(dto.NombreComprador, dto.EmailComprador);

            // Calcular el precio total de la reserva (cantidad * precio del evento)
            var precioTotal = dto.Cantidad * evento!.Precio;

            // Registrar la reserva (el código de reserva se registrará inicialmente como nulo)
            var reserva = new Reserva
            {
                CodigoReserva = null,
                ClienteId = cliente.ClienteId,
                EventoId = dto.EventoId,
                FechaReserva = now,
                CantidadEntradas = dto.Cantidad,
                PrecioReserva = precioTotal,
                EstadoReservaId = ReservationStatusConstants.PagoPendiente
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return Result<Reserva>.Success(reserva);
        }

        /// <summary>
        /// Obtiene todas las reservas registradas con información detallada de eventos y clientes.
        /// </summary>
        /// <returns>Colección de DTOs de reservas.</returns>
        public async Task<IEnumerable<ReservaDto>> GetReservasAsync()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Evento)
                    .ThenInclude(e => e!.Venue)
                .ToListAsync();

            return reservas.Select(MapToDto);
        }

        /// <summary>
        /// Obtiene las reservas asociadas a un correo electrónico de cliente específico con información detallada.
        /// </summary>
        /// <param name="email">El correo electrónico del cliente a consultar.</param>
        /// <returns>Resultado de la operación que contiene la lista de reservas en caso de éxito o un error si el formato del correo no es válido.</returns>
        public async Task<Result<IEnumerable<ReservaDto>>> GetReservasByEmailAsync(string email)
        {
            if (!IsValidEmail(email))
            {
                return Result<IEnumerable<ReservaDto>>.Failure(MessageConstants.InvalidEmailFormatError);
            }

            var reservas = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Evento)
                    .ThenInclude(e => e!.Venue)
                .Where(r => r.Cliente != null && r.Cliente.Correo.ToLower() == email.ToLower())
                .ToListAsync();

            var dtos = reservas.Select(MapToDto);
            return Result<IEnumerable<ReservaDto>>.Success(dtos);
        }

        /// <summary>
        /// Cancela una reserva existente según reglas de negocio.
        /// </summary>
        /// <param name="reservaId">El identificador de la reserva a cancelar.</param>
        /// <returns>Resultado indicando éxito o fracaso, conteniendo la reserva modificada en caso de éxito.</returns>
        public async Task<Result<Reserva>> CancelarReservaAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Evento)
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

            if (reserva == null)
            {
                return Result<Reserva>.Failure(MessageConstants.ReservaNotFoundError);
            }

            if (reserva.EstadoReservaId != ReservationStatusConstants.Confirmada)
            {
                return Result<Reserva>.Failure(MessageConstants.OnlyConfirmedReservationsCanBeCancelledError);
            }

            var now = DateTime.UtcNow;
            if (reserva.Evento != null &&
                reserva.Evento.FechaInicio <= now.AddHours(ReservationBusinessConstants.HoursLimitBeforeEventToLoseReservation))
            {
                reserva.EstadoReservaId = ReservationStatusConstants.Perdida;
            }
            else
            {
                reserva.EstadoReservaId = ReservationStatusConstants.Cancelada;
            }

            reserva.FechaModificacion = now;
            await _context.SaveChangesAsync();

            return Result<Reserva>.Success(reserva);
        }

        /// <summary>
        /// Confirma el pago de una reserva existente, asignándole un código único.
        /// </summary>
        /// <param name="reservaId">El identificador de la reserva a confirmar.</param>
        /// <returns>Resultado indicando éxito o fracaso, conteniendo la reserva modificada en caso de éxito.</returns>
        public async Task<Result<Reserva>> ConfirmarPagoAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

            if (reserva == null)
            {
                return Result<Reserva>.Failure(MessageConstants.ReservaNotFoundError);
            }

            if (reserva.EstadoReservaId == ReservationStatusConstants.Confirmada)
            {
                return Result<Reserva>.Failure(MessageConstants.ReservationAlreadyConfirmedError);
            }

            if (reserva.EstadoReservaId == ReservationStatusConstants.Cancelada ||
                reserva.EstadoReservaId == ReservationStatusConstants.Perdida)
            {
                return Result<Reserva>.Failure(MessageConstants.ReservationAlreadyCancelledError);
            }

            // Generar código de reserva único en formato EV-{6 dígitos}
            var uniqueCode = await GenerateUniqueCodigoReservaAsync();

            var now = DateTime.UtcNow;
            reserva.EstadoReservaId = ReservationStatusConstants.Confirmada;
            reserva.CodigoReserva = uniqueCode;
            reserva.FechaModificacion = now;

            await _context.SaveChangesAsync();

            return Result<Reserva>.Success(reserva);
        }

        /// <summary>
        /// Agrupa y ejecuta todas las validaciones de negocio para una reserva.
        /// </summary>
        private async Task<Result> ValidateReservationAsync(CrearReservaDto dto, Evento? evento, DateTime now)
        {
            var basicValidation = ValidateBasicReservationData(dto);
            if (!basicValidation.IsSuccess) return basicValidation;

            if (evento == null)
            {
                return Result.Failure(MessageConstants.EventNotFoundError);
            }

            var timingValidation = ValidateEventTimingAndLimits(dto, evento, now);
            if (!timingValidation.IsSuccess) return timingValidation;

            var capacityValidation = await ValidateEventCapacityAsync(evento.EventoId, evento.Capacidad, dto.Cantidad);
            if (!capacityValidation.IsSuccess) return capacityValidation;

            return Result.Success();
        }

        /// <summary>
        /// Valida los datos básicos del DTO de reserva (email y cantidad mínima).
        /// </summary>
        private Result ValidateBasicReservationData(CrearReservaDto dto)
        {
            if (!IsValidEmail(dto.EmailComprador))
            {
                return Result.Failure(MessageConstants.InvalidEmailFormatError);
            }

            if (dto.Cantidad < ReservationBusinessConstants.MinReservationQuantity)
            {
                return Result.Failure(MessageConstants.MinQuantityError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Valida la fecha de inicio del evento y los límites de cantidad según precio y tiempo restante.
        /// </summary>
        private Result ValidateEventTimingAndLimits(CrearReservaDto dto, Evento evento, DateTime now)
        {
            // Validar fecha de inicio del evento: no se permite reservar eventos que inicien en menos de 1 hora
            if (evento.FechaInicio <= now.AddHours(ReservationBusinessConstants.MinimumHoursBeforeEventToReserve))
            {
                return Result.Failure(MessageConstants.EventStartTooSoonError);
            }

            // Validar límites de cantidad específicos
            // A. Si el precio del evento es mayor a 100, se limita la cantidad de reservas a 10
            if (evento.Precio > ReservationBusinessConstants.ExpensiveEventPriceThreshold && 
                dto.Cantidad > ReservationBusinessConstants.MaxReservationQuantityForExpensiveEvents)
            {
                return Result.Failure(MessageConstants.MaxQuantityPriceLimitError);
            }

            // B. Si faltan menos de 24 horas para iniciar el evento, se limita a 5 la cantidad máxima
            if ((evento.FechaInicio - now).TotalHours < ReservationBusinessConstants.UrgentEventHoursThreshold && 
                dto.Cantidad > ReservationBusinessConstants.MaxReservationQuantityForUrgentEvents)
            {
                return Result.Failure(MessageConstants.MaxQuantityTimeLimitError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Valida que la cantidad de entradas solicitadas no supere las entradas disponibles para el evento.
        /// </summary>
        private async Task<Result> ValidateEventCapacityAsync(int eventId, int capacity, int requestedQuantity)
        {
            var totalReservadas = await _context.Reservas
                .Where(r => r.EventoId == eventId &&
                            (r.EstadoReservaId == ReservationStatusConstants.Confirmada ||
                             r.EstadoReservaId == ReservationStatusConstants.PagoPendiente ||
                             r.EstadoReservaId == ReservationStatusConstants.Perdida))
                .SumAsync(r => r.CantidadEntradas);

            var entradasDisponibles = capacity - totalReservadas;
            if (requestedQuantity > entradasDisponibles)
            {
                return Result.Failure(MessageConstants.NotEnoughTicketsError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Valida si una dirección de correo electrónico es válida.
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                var host = addr.Host;
                return addr.Address == email && host.Contains(".") && !host.StartsWith(".") && !host.EndsWith(".");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene un cliente existente por su correo electrónico o registra uno nuevo si no existe.
        /// </summary>
        private async Task<Cliente> GetOrCreateClienteAsync(string nombre, string correo)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Correo.ToLower() == correo.ToLower());

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    ClienteId = Guid.NewGuid(),
                    Nombre = nombre,
                    Correo = correo
                };
                _context.Clientes.Add(cliente);
            }

            return cliente;
        }

        /// <summary>
        /// Mapea un objeto entidad Reserva a su DTO correspondiente ReservaDto.
        /// </summary>
        private static ReservaDto MapToDto(Reserva r)
        {
            return new ReservaDto
            {
                ReservaId = r.ReservaId,
                CodigoReserva = r.CodigoReserva,
                FechaReserva = r.FechaReserva,
                CantidadEntradas = r.CantidadEntradas,
                PrecioReserva = r.PrecioReserva,
                EstadoReservaId = r.EstadoReservaId,
                NombreCliente = r.Cliente?.Nombre ?? string.Empty,
                CorreoCliente = r.Cliente?.Correo ?? string.Empty,
                TituloEvento = r.Evento?.NombreEvento ?? string.Empty,
                FechaEvento = r.Evento?.FechaInicio ?? DateTime.MinValue,
                NombreVenue = r.Evento?.Venue?.Nombre ?? string.Empty,
                TipoEventoId = r.Evento?.TipoEventoId ?? string.Empty
            };
        }

        /// <summary>
        /// Genera un código único de reserva en formato EV-{6 dígitos}.
        /// </summary>
        private async Task<string> GenerateUniqueCodigoReservaAsync()
        {
            var random = new Random();
            string code;
            bool exists;
            do
            {
                var number = random.Next(100000, 1000000);
                code = $"EV-{number:D6}";
                exists = await _context.Reservas.AnyAsync(r => r.CodigoReserva == code);
            } while (exists);
            return code;
        }
    }
}
