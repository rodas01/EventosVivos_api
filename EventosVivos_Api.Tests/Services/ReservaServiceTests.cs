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
    public class ReservaServiceTests
    {
        private DbContextOptions<ApplicationDbContext>? _options;
        private ApplicationDbContext? _context;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);

            // Venues de prueba
            var venue = new Venue { VenueId = 1, Nombre = "Estadio", CapacidadMaxima = 50000, Ubicacion = "Ciudad" };
            _context.Venues.Add(venue);

            // Tipos de eventos
            _context.TiposEventos.Add(new TipoEvento { TipoEventoId = "CON", Descripcion = "Concierto" });

            // Estados de eventos
            _context.EstadosEventos.Add(new EstadoEvento { EstadoEventoId = "ACTIVO", Descripcion = "Activo" });

            // Evento 1: Normal
            var event1 = new Evento
            {
                EventoId = 1,
                NombreEvento = "Concierto Normal",
                Descripcion = "Descripcion",
                VenueId = 1,
                Capacidad = 100,
                FechaInicio = DateTime.UtcNow.AddDays(5),
                FechaFin = DateTime.UtcNow.AddDays(5).AddHours(3),
                Precio = 50f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            // Evento 2: Precio alto (> 100)
            var event2 = new Evento
            {
                EventoId = 2,
                NombreEvento = "Concierto VIP",
                Descripcion = "Descripcion",
                VenueId = 1,
                Capacidad = 100,
                FechaInicio = DateTime.UtcNow.AddDays(5),
                FechaFin = DateTime.UtcNow.AddDays(5).AddHours(3),
                Precio = 150f, // > 100
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            // Evento 3: Faltan menos de 24 horas (ej. 12 horas)
            var event3 = new Evento
            {
                EventoId = 3,
                NombreEvento = "Concierto de Mañana",
                Descripcion = "Descripcion",
                VenueId = 1,
                Capacidad = 100,
                FechaInicio = DateTime.UtcNow.AddHours(12), // < 24 horas y > 1 hora
                FechaFin = DateTime.UtcNow.AddHours(15),
                Precio = 50f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            // Evento 4: Falta menos de 1 hora (ej. 30 minutos)
            var event4 = new Evento
            {
                EventoId = 4,
                NombreEvento = "Concierto Ya",
                Descripcion = "Descripcion",
                VenueId = 1,
                Capacidad = 100,
                FechaInicio = DateTime.UtcNow.AddMinutes(30), // < 1 hora
                FechaFin = DateTime.UtcNow.AddHours(2),
                Precio = 50f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            // Evento 5: Con capacidad limitada / agotada
            var event5 = new Evento
            {
                EventoId = 5,
                NombreEvento = "Concierto Exclusivo",
                Descripcion = "Descripcion",
                VenueId = 1,
                Capacidad = 5, // Capacidad pequeña
                FechaInicio = DateTime.UtcNow.AddDays(5),
                FechaFin = DateTime.UtcNow.AddDays(5).AddHours(3),
                Precio = 50f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            _context.Eventos.AddRange(event1, event2, event3, event4, event5);

            // Clientes de prueba
            var client = new Cliente { ClienteId = Guid.NewGuid(), Nombre = "Cliente Existente", Correo = "existente@correo.com" };
            _context.Clientes.Add(client);

            // Reserva de prueba para evento 5
            _context.Reservas.Add(new Reserva
            {
                ReservaId = 100,
                CodigoReserva = "EV-999999",
                ClienteId = client.ClienteId,
                EventoId = 5,
                FechaReserva = DateTime.UtcNow.AddDays(-1),
                CantidadEntradas = 3,
                PrecioReserva = 150f,
                EstadoReservaId = ReservationStatusConstants.Confirmada
            });

            _context.SaveChanges();
        }

        [TestCleanup]
        public void Teardown()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithValidDataAndNewClient_CreatesReservationAndClientSuccessfully()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 4,
                NombreComprador = "Nuevo Cliente",
                EmailComprador = "nuevo@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.IsTrue(result.Value.ReservaId > 0);
            Assert.IsNull(result.Value.CodigoReserva);
            Assert.AreEqual(4, result.Value.CantidadEntradas);
            Assert.AreEqual(200f, result.Value.PrecioReserva); // 4 * 50
            Assert.AreEqual(ReservationStatusConstants.PagoPendiente, result.Value.EstadoReservaId);

            var dbClient = await _context.Clientes.FirstOrDefaultAsync(c => c.Correo == "nuevo@correo.com");
            Assert.IsNotNull(dbClient);
            Assert.AreEqual("Nuevo Cliente", dbClient.Nombre);
            Assert.AreEqual(dbClient.ClienteId, result.Value.ClienteId);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithExistingClient_ReusesClientRecord()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 2,
                NombreComprador = "Cliente Existente",
                EmailComprador = "existente@correo.com"
            };

            var initialClientCount = await _context.Clientes.CountAsync();

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            
            var finalClientCount = await _context.Clientes.CountAsync();
            Assert.AreEqual(initialClientCount, finalClientCount); // No se creó un nuevo cliente

            var dbClient = await _context.Clientes.FirstOrDefaultAsync(c => c.Correo == "existente@correo.com");
            Assert.IsNotNull(dbClient);
            Assert.AreEqual(dbClient.ClienteId, result.Value.ClienteId);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithInvalidEmailFormat_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 2,
                NombreComprador = "Prueba",
                EmailComprador = "correo_invalido"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.InvalidEmailFormatError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithQuantityLessThanOne_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 0,
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.MinQuantityError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithNonExistentEvent_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 999, // No existe
                Cantidad = 2,
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.EventNotFoundError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithEventStartingInLessThanOneHour_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 4, // Inicia en 30 minutos
                Cantidad = 2,
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.EventStartTooSoonError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithHighPriceEventAndQuantityOver10_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 2, // Precio = 150f (> 100)
                Cantidad = 11, // Excede el límite de 10
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.MaxQuantityPriceLimitError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithHighPriceEventAndQuantityOf10_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 2, // Precio = 150f (> 100)
                Cantidad = 10, // Justo el límite
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithUrgentEventAndQuantityOver5_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 3, // Inicia en 12 horas (< 24 horas)
                Cantidad = 6, // Excede el límite de 5
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.MaxQuantityTimeLimitError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithUrgentEventAndQuantityOf5_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            var dto = new CrearReservaDto
            {
                EventoId = 3, // Inicia en 12 horas (< 24 horas)
                Cantidad = 5, // Justo el límite
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithQuantityExceedingAvailableCapacity_ReturnsFailureResult()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            // Evento 5 tiene capacidad = 5 y ya cuenta con una reserva de 3 entradas. Quedan 2 disponibles.
            var dto = new CrearReservaDto
            {
                EventoId = 5,
                Cantidad = 3, // Intenta reservar 3 (excede el disponible de 2)
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.NotEnoughTicketsError, result.Error);
        }

        [TestMethod]
        public async Task RealizarReservaAsync_WithQuantityEqualToAvailableCapacity_ReturnsSuccess()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);
            // Evento 5 tiene capacidad = 5 y ya cuenta con una reserva de 3 entradas. Quedan 2 disponibles.
            var dto = new CrearReservaDto
            {
                EventoId = 5,
                Cantidad = 2, // Cantidad exacta disponible
                NombreComprador = "Prueba",
                EmailComprador = "prueba@correo.com"
            };

            // Act
            var result = await service.RealizarReservaAsync(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task GetReservasAsync_ReturnsAllReservationsWithDetailedInfo()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Act
            var result = await service.GetReservasAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            var reservation = result.First();
            Assert.AreEqual("EV-999999", reservation.CodigoReserva);
            Assert.AreEqual("Cliente Existente", reservation.NombreCliente);
            Assert.AreEqual("existente@correo.com", reservation.CorreoCliente);
            Assert.AreEqual("Concierto Exclusivo", reservation.TituloEvento);
            Assert.AreEqual("Estadio", reservation.NombreVenue);
        }

        [TestMethod]
        public async Task GetReservasByEmailAsync_WithValidEmailAndMatchingReservations_ReturnsSuccessWithList()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Acto
            var result = await service.GetReservasByEmailAsync("existente@correo.com");

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(1, result.Value.Count());

            var reservation = result.Value.First();
            Assert.AreEqual("EV-999999", reservation.CodigoReserva);
            Assert.AreEqual("Cliente Existente", reservation.NombreCliente);
            Assert.AreEqual("existente@correo.com", reservation.CorreoCliente);
        }

        [TestMethod]
        public async Task GetReservasByEmailAsync_WithValidEmailAndNoReservations_ReturnsSuccessWithEmptyList()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Acto
            var result = await service.GetReservasByEmailAsync("otro@correo.com");

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(0, result.Value.Count());
        }

        [TestMethod]
        public async Task GetReservasByEmailAsync_WithInvalidEmailFormat_ReturnsFailure()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Acto
            var result = await service.GetReservasByEmailAsync("correo_invalido");

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.InvalidEmailFormatError, result.Error);
        }

        [TestMethod]
        public async Task CancelarReservaAsync_WithNonExistentReserva_ReturnsFailure()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Acto
            var result = await service.CancelarReservaAsync(999);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.ReservaNotFoundError, result.Error);
        }

        [TestMethod]
        public async Task CancelarReservaAsync_WithReservationNotConfirmed_ReturnsFailure()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var client = await _context.Clientes.FirstAsync();
            var pendingReserva = new Reserva
            {
                ReservaId = 200,
                CodigoReserva = null,
                ClienteId = client.ClienteId,
                EventoId = 1,
                FechaReserva = DateTime.UtcNow,
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.PagoPendiente
            };
            _context.Reservas.Add(pendingReserva);
            await _context.SaveChangesAsync();

            var service = new ReservaService(_context);

            // Acto
            var result = await service.CancelarReservaAsync(200);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.OnlyConfirmedReservationsCanBeCancelledError, result.Error);
        }

        [TestMethod]
        public async Task CancelarReservaAsync_WithMoreThan48HoursRemaining_CancelsSuccessfully()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var client = await _context.Clientes.FirstAsync();
            var eventId = 1; // Ya existe en Setup y comienza en 5 días.

            var confirmedReserva = new Reserva
            {
                ReservaId = 201,
                CodigoReserva = "EV-111111",
                ClienteId = client.ClienteId,
                EventoId = eventId,
                FechaReserva = DateTime.UtcNow.AddDays(-1),
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.Confirmada
            };
            _context.Reservas.Add(confirmedReserva);
            await _context.SaveChangesAsync();

            var service = new ReservaService(_context);

            // Acto
            var result = await service.CancelarReservaAsync(201);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(ReservationStatusConstants.Cancelada, result.Value.EstadoReservaId);
            Assert.IsNotNull(result.Value.FechaModificacion);
        }

        [TestMethod]
        public async Task CancelarReservaAsync_WithLessThan48HoursRemaining_MarksAsLostSuccessfully()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var client = await _context.Clientes.FirstAsync();
            var eventId = 3; // Ya existe en Setup y comienza en 12 horas.

            var confirmedReserva = new Reserva
            {
                ReservaId = 202,
                CodigoReserva = "EV-222222",
                ClienteId = client.ClienteId,
                EventoId = eventId,
                FechaReserva = DateTime.UtcNow.AddDays(-1),
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.Confirmada
            };
            _context.Reservas.Add(confirmedReserva);
            await _context.SaveChangesAsync();

            var service = new ReservaService(_context);

            // Acto
            var result = await service.CancelarReservaAsync(202);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(ReservationStatusConstants.Perdida, result.Value.EstadoReservaId);
            Assert.IsNotNull(result.Value.FechaModificacion);
        }

        [TestMethod]
        public async Task ConfirmarPagoAsync_WithNonExistentReserva_ReturnsFailure()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Acto
            var result = await service.ConfirmarPagoAsync(999);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.ReservaNotFoundError, result.Error);
        }

        [TestMethod]
        public async Task ConfirmarPagoAsync_WithAlreadyConfirmedReservation_ReturnsFailure()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var service = new ReservaService(_context);

            // Acto
            var result = await service.ConfirmarPagoAsync(100);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.ReservationAlreadyConfirmedError, result.Error);
        }

        [TestMethod]
        public async Task ConfirmarPagoAsync_WithCancelledReservation_ReturnsFailure()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var client = await _context.Clientes.FirstAsync();
            var cancelledReserva = new Reserva
            {
                ReservaId = 203,
                CodigoReserva = null,
                ClienteId = client.ClienteId,
                EventoId = 1,
                FechaReserva = DateTime.UtcNow,
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.Cancelada
            };
            _context.Reservas.Add(cancelledReserva);
            await _context.SaveChangesAsync();

            var service = new ReservaService(_context);

            // Acto
            var result = await service.ConfirmarPagoAsync(203);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(MessageConstants.ReservationAlreadyCancelledError, result.Error);
        }

        [TestMethod]
        public async Task ConfirmarPagoAsync_WithPendingPaymentReservation_ConfirmsSuccessfullyAndGeneratesCode()
        {
            // Preparación
            Assert.IsNotNull(_context);
            var client = await _context.Clientes.FirstAsync();
            var pendingReserva = new Reserva
            {
                ReservaId = 204,
                CodigoReserva = null,
                ClienteId = client.ClienteId,
                EventoId = 1,
                FechaReserva = DateTime.UtcNow,
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.PagoPendiente
            };
            _context.Reservas.Add(pendingReserva);
            await _context.SaveChangesAsync();

            var service = new ReservaService(_context);

            // Acto
            var result = await service.ConfirmarPagoAsync(204);

            // Verificación
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(ReservationStatusConstants.Confirmada, result.Value.EstadoReservaId);
            Assert.IsNotNull(result.Value.CodigoReserva);
            Assert.IsTrue(result.Value.CodigoReserva.StartsWith("EV-"));
            Assert.AreEqual(9, result.Value.CodigoReserva.Length);
            Assert.IsNotNull(result.Value.FechaModificacion);
        }
    }
}
