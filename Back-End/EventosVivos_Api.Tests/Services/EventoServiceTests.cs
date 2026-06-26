using EventosVivos_Api.Data;
using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using EventosVivos_Api.Util;
using EventosVivos_Api.Util.Const;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventosVivos_Api.Tests.Services
{
    [TestClass]
    public class EventoServiceTests
    {
        private DbContextOptions<ApplicationDbContext>? _options;
        private ApplicationDbContext? _context;
        private Venue? _venue1;
        private Venue? _venue2;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);

            // Inicialización de tipos de eventos requeridos para las validaciones
            _context.TiposEventos.AddRange(new List<TipoEvento>
            {
                new TipoEvento { TipoEventoId = "CON", Descripcion = "Concierto" },
                new TipoEvento { TipoEventoId = "DEP", Descripcion = "Deportivo" },
                new TipoEvento { TipoEventoId = "TEA", Descripcion = "Teatro" }
            });

            // Inicialización de venues de prueba
            _venue1 = new Venue { VenueId = 1, Nombre = "Estadio Nacional", CapacidadMaxima = 50000, Ubicacion = "Ciudad A" };
            _venue2 = new Venue { VenueId = 2, Nombre = "Teatro Principal", CapacidadMaxima = 1000, Ubicacion = "Ciudad B" };
            _context.Venues.AddRange(_venue1, _venue2);

            var evento1 = new Evento
            {
                EventoId = 1,
                NombreEvento = "Concierto de Rock",
                Descripcion = "Gran concierto de rock nacional",
                VenueId = 1,
                Capacidad = 100,
                FechaInicio = new DateTime(2026, 7, 10, 20, 0, 0),
                FechaFin = new DateTime(2026, 7, 10, 23, 0, 0),
                Precio = 50f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            var evento2 = new Evento
            {
                EventoId = 2,
                NombreEvento = "Partido de Futbol",
                Descripcion = "Final de la copa local",
                VenueId = 1,
                Capacidad = 200,
                FechaInicio = new DateTime(2026, 7, 15, 16, 0, 0),
                FechaFin = new DateTime(2026, 7, 15, 18, 0, 0),
                Precio = 20f,
                TipoEventoId = "DEP",
                EstadoEventoId = EventStatusConstants.Activo
            };

            var evento3 = new Evento
            {
                EventoId = 3,
                NombreEvento = "Obra de Teatro",
                Descripcion = "Clásico de la literatura",
                VenueId = 2,
                Capacidad = 50,
                FechaInicio = new DateTime(2026, 8, 1, 19, 0, 0),
                FechaFin = new DateTime(2026, 8, 1, 21, 0, 0),
                Precio = 30f,
                TipoEventoId = "TEA",
                EstadoEventoId = EventStatusConstants.Inactivo
            };

            _context.Eventos.AddRange(evento1, evento2, evento3);
            _context.SaveChanges();
        }

        [TestCleanup]
        public void Teardown()
        {
            _context?.Dispose();
        }

        #region Pruebas de GetEventosAsync

        [TestMethod]
        public async Task GetEventosAsync_WithNoFilters_ReturnsAllEventos()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto();

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public async Task GetEventosAsync_WithTipoEventoFilter_ReturnsFilteredEventos()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto { TipoEvento = "CON" };

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Concierto de Rock", result.First().NombreEvento);
        }

        [TestMethod]
        public async Task GetEventosAsync_WithDateFilters_ReturnsEventosInDateRange()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto
            {
                FechaInicio = new DateTime(2026, 7, 12),
                FechaFin = new DateTime(2026, 7, 18)
            };

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Partido de Futbol", result.First().NombreEvento);
        }

        [TestMethod]
        public async Task GetEventosAsync_WithVenueIdFilter_ReturnsEventosForVenue()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto { VenueId = 2 };

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Obra de Teatro", result.First().NombreEvento);
        }

        [TestMethod]
        public async Task GetEventosAsync_WithEstadoFilter_ReturnsEventosWithEstado()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto { Estado = EventStatusConstants.Inactivo };

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Obra de Teatro", result.First().NombreEvento);
        }

        [TestMethod]
        public async Task GetEventosAsync_WithTituloFilter_ReturnsEventosWithMatchingTitle()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto { Titulo = "rock" };

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Concierto de Rock", result.First().NombreEvento);
        }

        [TestMethod]
        public async Task GetReporteEventosAsync_ReturnsCorrectReportingMetrics()
        {
            // Arrange
            Assert.IsNotNull(_context);
            
            // Seed client, reservation states, and reservations
            var client = new Cliente { ClienteId = Guid.NewGuid(), Nombre = "Cliente Test", Correo = "test@correo.com" };
            _context.Clientes.Add(client);
            
            var confirmedState = new EstadoReserva { EstadoReservaId = "COMFIRMADA", Descripcion = "Confirmada" };
            var pendingState = new EstadoReserva { EstadoReservaId = "PAGO_PENDIENTE", Descripcion = "Pendiente Pago" };
            var lostState = new EstadoReserva { EstadoReservaId = "PERDIDA", Descripcion = "Perdida" };
            _context.EstadosReservas.AddRange(confirmedState, pendingState, lostState);

            // Add reservations for Event 1 (Rock Concert)
            // 2 confirmed: 5 entries + 3 entries
            // 1 pending: 2 entries
            // 1 lost: 1 entry
            _context.Reservas.AddRange(
                new Reserva { ReservaId = 1, CodigoReserva = "RES1", ClienteId = client.ClienteId, EventoId = 1, CantidadEntradas = 5, PrecioReserva = 250f, EstadoReservaId = "COMFIRMADA" },
                new Reserva { ReservaId = 2, CodigoReserva = "RES2", ClienteId = client.ClienteId, EventoId = 1, CantidadEntradas = 3, PrecioReserva = 150f, EstadoReservaId = "COMFIRMADA" },
                new Reserva { ReservaId = 3, CodigoReserva = "RES3", ClienteId = client.ClienteId, EventoId = 1, CantidadEntradas = 2, PrecioReserva = 100f, EstadoReservaId = "PAGO_PENDIENTE" },
                new Reserva { ReservaId = 4, CodigoReserva = "RES4", ClienteId = client.ClienteId, EventoId = 1, CantidadEntradas = 1, PrecioReserva = 50f, EstadoReservaId = "PERDIDA" }
            );
            
            await _context.SaveChangesAsync();
            
            var service = new EventoService(_context);

            // Act
            var result = await service.GetReporteEventosAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            
            var rockReport = result.First(r => r.NombreEvento == "Concierto de Rock");
            Assert.AreEqual(5 + 3 + 2, rockReport.Ocupacion); // 10
            Assert.AreEqual(100 - (5 + 3 + 2 + 1), rockReport.ReservasDisponibles); // 89
            Assert.AreEqual(5 + 3, rockReport.ReservasVendidas); // 8
            Assert.AreEqual(250f + 150f, rockReport.TotalIngresos); // 400f
        }

        [TestMethod]
        public async Task GetEventosAsync_WithPastActiveEvent_ReturnsCompletadoStatus()
        {
            // Arrange
            Assert.IsNotNull(_context);
            
            // Seed a past event with ACTIVO status
            var pastEvent = new Evento
            {
                EventoId = 101,
                NombreEvento = "Concierto Antiguo",
                Descripcion = "Concierto que ya paso",
                VenueId = 1,
                Capacidad = 100,
                FechaInicio = DateTime.UtcNow.AddDays(-2),
                FechaFin = DateTime.UtcNow.AddDays(-1),
                Precio = 10f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };
            _context.Eventos.Add(pastEvent);
            await _context.SaveChangesAsync();

            var service = new EventoService(_context);
            var filtrosDto = new FiltrosEventoDto { Titulo = "Antiguo" };

            // Act
            var result = await service.GetEventosAsync(filtrosDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(EventStatusConstants.Completado, result.First().EstadoEventoId);
        }

        [TestMethod]
        public async Task GetEventoByIdAsync_WithValidId_ReturnsSuccessResult_WithEventoDto()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);

            // Act
            var result = await service.GetEventoByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual("Concierto de Rock", result.Value.NombreEvento);
        }

        [TestMethod]
        public async Task GetEventoByIdAsync_WithNonExistentId_ReturnsFailureResult_WithNotFoundError()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);

            // Act
            var result = await service.GetEventoByIdAsync(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.EventNotFoundError, result.Error);
        }

        #endregion

        #region Pruebas de CrearEventoAsync

        [TestMethod]
        public async Task CrearEventoAsync_WithValidData_CreatesEventSuccessfully()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Festival pop del año",
                IdVenue = 1,
                CapacidadMaxima = 1000,
                FechaHoraInicio = new DateTime(2026, 7, 20, 18, 0, 0),
                FechaHoraFin = new DateTime(2026, 7, 20, 21, 0, 0),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.EventoId > 0);
            Assert.AreEqual("Concierto Pop", result.Value.NombreEvento);

            var dbEvent = await _context.Eventos.FindAsync(result.Value.EventoId);
            Assert.IsNotNull(dbEvent);
            Assert.AreEqual(EventStatusConstants.Activo, dbEvent.EstadoEventoId);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithPastStartDate_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Festival pop",
                IdVenue = 1,
                CapacidadMaxima = 1000,
                FechaHoraInicio = DateTime.UtcNow.AddMinutes(-30),
                FechaHoraFin = DateTime.UtcNow.AddHours(2),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.FutureStartDateError, result.Error);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithEndDateBeforeStartDate_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Festival pop",
                IdVenue = 1,
                CapacidadMaxima = 1000,
                FechaHoraInicio = DateTime.UtcNow.AddDays(2),
                FechaHoraFin = DateTime.UtcNow.AddDays(2).AddHours(-1),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.EndDateBeforeStartDateError, result.Error);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithWeekendStartAfter22_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            // El 11 de julio de 2026 es sábado
            var saturdayLate = new DateTime(2026, 7, 11, 22, 30, 0);

            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Fin de Semana",
                Descripcion = "Concierto nocturno",
                IdVenue = 1,
                CapacidadMaxima = 1000,
                FechaHoraInicio = saturdayLate,
                FechaHoraFin = saturdayLate.AddHours(2),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.WeekendStartTimeError, result.Error);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithNonExistentVenue_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Festival pop",
                IdVenue = 999,
                CapacidadMaxima = 1000,
                FechaHoraInicio = new DateTime(2026, 7, 20, 18, 0, 0),
                FechaHoraFin = new DateTime(2026, 7, 20, 21, 0, 0),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.VenueNotFoundError, result.Error);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithCapacityExceedingVenue_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Festival pop",
                IdVenue = 2, // Capacidad máxima del Venue 2 es 1000
                CapacidadMaxima = 1500, // Excede el límite de 1000
                FechaHoraInicio = new DateTime(2026, 7, 20, 18, 0, 0),
                FechaHoraFin = new DateTime(2026, 7, 20, 21, 0, 0),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.CapacityExceededError, result.Error);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithOverlappingEvent_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            // El evento1 ya existe en VenueId = 1 de 20:00 a 23:00 el 10 de Julio de 2026
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Otro Concierto",
                Descripcion = "Mismo lugar, misma hora",
                IdVenue = 1,
                CapacidadMaxima = 1000,
                FechaHoraInicio = new DateTime(2026, 7, 10, 21, 0, 0),
                FechaHoraFin = new DateTime(2026, 7, 10, 22, 0, 0),
                PrecioEntrada = 35.0m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.EventOverlapError, result.Error);
        }

        [TestMethod]
        public async Task CrearEventoAsync_WithNonExistentEventType_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new EventoService(_context);
            var newEventDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Festival pop",
                IdVenue = 1,
                CapacidadMaxima = 1000,
                FechaHoraInicio = new DateTime(2026, 7, 20, 18, 0, 0),
                FechaHoraFin = new DateTime(2026, 7, 20, 21, 0, 0),
                PrecioEntrada = 35.0m,
                TipoEventoId = "XYZ" // Tipo de evento inválido
            };

            // Act
            var result = await service.CrearEventoAsync(newEventDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.InvalidEventTypeError, result.Error);
        }

        #endregion
    }
}
