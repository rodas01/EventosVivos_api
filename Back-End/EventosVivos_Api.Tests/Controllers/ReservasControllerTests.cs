using EventosVivos_Api.Controllers;
using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using EventosVivos_Api.Util;
using EventosVivos_Api.Util.Const;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventosVivos_Api.Tests.Controllers
{
    [TestClass]
    public class ReservasControllerTests
    {
        private Mock<IReservaService>? _mockReservaService;
        private ReservasController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockReservaService = new Mock<IReservaService>();
            _controller = new ReservasController(_mockReservaService.Object);
        }

        [TestMethod]
        public async Task RealizarReserva_ReturnsOkResult_WithCreatedReservation()
        {
            // Arrange
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 2,
                NombreComprador = "Comprador Prueba",
                EmailComprador = "prueba@correo.com"
            };

            var expectedReserva = new Reserva
            {
                ReservaId = 1,
                CodigoReserva = "EV-123456",
                ClienteId = Guid.NewGuid(),
                EventoId = 1,
                FechaReserva = DateTime.UtcNow,
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.PagoPendiente
            };

            _mockReservaService.Setup(s => s.RealizarReservaAsync(dto))
                .ReturnsAsync(Result<Reserva>.Success(expectedReserva));

            // Act
            var result = await _controller.RealizarReserva(dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedReserva = okResult.Value as Reserva;
            Assert.IsNotNull(returnedReserva);
            Assert.AreEqual("EV-123456", returnedReserva.CodigoReserva);
            Assert.AreEqual(2, returnedReserva.CantidadEntradas);
            Assert.AreEqual(100f, returnedReserva.PrecioReserva);
        }

        [TestMethod]
        public async Task RealizarReserva_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            Assert.IsNotNull(_controller);
            _controller.ModelState.AddModelError("EmailComprador", "El formato del correo electrónico del comprador no es válido.");

            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 2,
                NombreComprador = "Comprador Prueba",
                EmailComprador = "invalido"
            };

            // Act
            var result = await _controller.RealizarReserva(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task RealizarReserva_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var dto = new CrearReservaDto
            {
                EventoId = 1,
                Cantidad = 2,
                NombreComprador = "Comprador Prueba",
                EmailComprador = "prueba@correo.com"
            };

            _mockReservaService.Setup(s => s.RealizarReservaAsync(dto))
                .ReturnsAsync(Result<Reserva>.Failure(MessageConstants.NotEnoughTicketsError));

            // Act
            var result = await _controller.RealizarReserva(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            
            // Verificamos el mensaje de error usando reflection o dynamic
            dynamic? errorObj = badRequestResult.Value;
            Assert.IsNotNull(errorObj);
        }

        [TestMethod]
        public async Task GetReservas_ReturnsOkResult_WithListOfReservations()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var expectedReservas = new List<ReservaDto>
            {
                new ReservaDto
                {
                    ReservaId = 1,
                    CodigoReserva = "EV-123456",
                    FechaReserva = DateTime.UtcNow,
                    CantidadEntradas = 2,
                    PrecioReserva = 100f,
                    EstadoReservaId = ReservationStatusConstants.PagoPendiente,
                    NombreCliente = "Cliente Prueba",
                    CorreoCliente = "prueba@correo.com",
                    TituloEvento = "Evento Prueba",
                    FechaEvento = DateTime.UtcNow.AddDays(5),
                    NombreVenue = "Venue Prueba"
                }
            };

            _mockReservaService.Setup(s => s.GetReservasAsync())
                .ReturnsAsync(expectedReservas);

            // Acto
            var result = await _controller.GetReservas();

            // Verificación
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedReservas = okResult.Value as IEnumerable<ReservaDto>;
            Assert.IsNotNull(returnedReservas);
            Assert.AreEqual(1, returnedReservas.Count());
            var firstReserva = returnedReservas.First();
            Assert.AreEqual("EV-123456", firstReserva.CodigoReserva);
            Assert.AreEqual("Cliente Prueba", firstReserva.NombreCliente);
            Assert.AreEqual("prueba@correo.com", firstReserva.CorreoCliente);
            Assert.AreEqual("Evento Prueba", firstReserva.TituloEvento);
            Assert.AreEqual("Venue Prueba", firstReserva.NombreVenue);
        }

        [TestMethod]
        public async Task GetReservasByEmail_ReturnsOkResult_WithListOfReservations()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var email = "prueba@correo.com";
            var expectedReservas = new List<ReservaDto>
            {
                new ReservaDto
                {
                    ReservaId = 1,
                    CodigoReserva = "EV-123456",
                    FechaReserva = DateTime.UtcNow,
                    CantidadEntradas = 2,
                    PrecioReserva = 100f,
                    EstadoReservaId = ReservationStatusConstants.PagoPendiente,
                    NombreCliente = "Cliente Prueba",
                    CorreoCliente = email,
                    TituloEvento = "Evento Prueba",
                    FechaEvento = DateTime.UtcNow.AddDays(5),
                    NombreVenue = "Venue Prueba"
                }
            };

            _mockReservaService.Setup(s => s.GetReservasByEmailAsync(email))
                .ReturnsAsync(Result<IEnumerable<ReservaDto>>.Success(expectedReservas));

            // Acto
            var result = await _controller.GetReservasByEmail(email);

            // Verificación
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedReservas = okResult.Value as IEnumerable<ReservaDto>;
            Assert.IsNotNull(returnedReservas);
            Assert.AreEqual(1, returnedReservas.Count());
            var firstReserva = returnedReservas.First();
            Assert.AreEqual("EV-123456", firstReserva.CodigoReserva);
            Assert.AreEqual("Cliente Prueba", firstReserva.NombreCliente);
            Assert.AreEqual(email, firstReserva.CorreoCliente);
        }

        [TestMethod]
        public async Task GetReservasByEmail_ReturnsBadRequest_WhenServiceFails()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var email = "correo_invalido";

            _mockReservaService.Setup(s => s.GetReservasByEmailAsync(email))
                .ReturnsAsync(Result<IEnumerable<ReservaDto>>.Failure(MessageConstants.InvalidEmailFormatError));

            // Acto
            var result = await _controller.GetReservasByEmail(email);

            // Verificación
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            dynamic? errorObj = badRequestResult.Value;
            Assert.IsNotNull(errorObj);
        }

        [TestMethod]
        public async Task CancelarReserva_ReturnsOkResult_WhenCancellationSucceeds()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var reservaId = 1;
            var cancelledReserva = new Reserva
            {
                ReservaId = reservaId,
                CodigoReserva = "EV-123456",
                ClienteId = Guid.NewGuid(),
                EventoId = 1,
                FechaReserva = DateTime.UtcNow,
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.Cancelada
            };

            _mockReservaService.Setup(s => s.CancelarReservaAsync(reservaId))
                .ReturnsAsync(Result<Reserva>.Success(cancelledReserva));

            // Acto
            var result = await _controller.CancelarReserva(reservaId);

            // Verificación
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedReserva = okResult.Value as Reserva;
            Assert.IsNotNull(returnedReserva);
            Assert.AreEqual(reservaId, returnedReserva.ReservaId);
            Assert.AreEqual(ReservationStatusConstants.Cancelada, returnedReserva.EstadoReservaId);
        }

        [TestMethod]
        public async Task CancelarReserva_ReturnsBadRequest_WhenCancellationFails()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var reservaId = 1;

            _mockReservaService.Setup(s => s.CancelarReservaAsync(reservaId))
                .ReturnsAsync(Result<Reserva>.Failure(MessageConstants.ReservaNotFoundError));

            // Acto
            var result = await _controller.CancelarReserva(reservaId);

            // Verificación
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            dynamic? errorObj = badRequestResult.Value;
            Assert.IsNotNull(errorObj);
        }

        [TestMethod]
        public async Task ConfirmarPago_ReturnsOkResult_WhenPaymentConfirmationSucceeds()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var reservaId = 1;
            var confirmedReserva = new Reserva
            {
                ReservaId = reservaId,
                CodigoReserva = "EV-123456",
                ClienteId = Guid.NewGuid(),
                EventoId = 1,
                FechaReserva = DateTime.UtcNow,
                CantidadEntradas = 2,
                PrecioReserva = 100f,
                EstadoReservaId = ReservationStatusConstants.Confirmada
            };

            _mockReservaService.Setup(s => s.ConfirmarPagoAsync(reservaId))
                .ReturnsAsync(Result<Reserva>.Success(confirmedReserva));

            // Acto
            var result = await _controller.ConfirmarPago(reservaId);

            // Verificación
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedReserva = okResult.Value as Reserva;
            Assert.IsNotNull(returnedReserva);
            Assert.AreEqual(reservaId, returnedReserva.ReservaId);
            Assert.AreEqual(ReservationStatusConstants.Confirmada, returnedReserva.EstadoReservaId);
            Assert.AreEqual("EV-123456", returnedReserva.CodigoReserva);
        }

        [TestMethod]
        public async Task ConfirmarPago_ReturnsBadRequest_WhenPaymentConfirmationFails()
        {
            // Preparación
            Assert.IsNotNull(_mockReservaService);
            Assert.IsNotNull(_controller);

            var reservaId = 1;

            _mockReservaService.Setup(s => s.ConfirmarPagoAsync(reservaId))
                .ReturnsAsync(Result<Reserva>.Failure(MessageConstants.ReservationAlreadyConfirmedError));

            // Acto
            var result = await _controller.ConfirmarPago(reservaId);

            // Verificación
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            dynamic? errorObj = badRequestResult.Value;
            Assert.IsNotNull(errorObj);
        }
    }
}
