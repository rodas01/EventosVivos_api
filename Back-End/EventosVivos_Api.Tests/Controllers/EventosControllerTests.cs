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
    public class EventosControllerTests
    {
        private Mock<IEventoService>? _mockEventoService;
        private EventosController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockEventoService = new Mock<IEventoService>();
            _controller = new EventosController(_mockEventoService.Object);
        }

        [TestMethod]
        public async Task GetEventos_ReturnsOkResult_WithListOfEventos()
        {
            // Arrange
            Assert.IsNotNull(_mockEventoService);
            Assert.IsNotNull(_controller);

            var filters = new FiltrosEventoDto { TipoEvento = "CON" };
            var expectedEventos = new List<EventoDto>
            {
                new EventoDto
                {
                    NombreEvento = "Concierto Rock",
                    Descripcion = "Concierto",
                    Capacidad = 100,
                    Precio = 50f,
                    TipoEventoId = "CON",
                    EstadoEventoId = EventStatusConstants.Activo,
                    Venue = new VenueDto { Nombre = "Estadio" },
                    SoldOut = false
                }
            };

            _mockEventoService.Setup(s => s.GetEventosAsync(filters))
                .ReturnsAsync(expectedEventos);

            // Act
            var result = await _controller.GetEventos(filters);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEventos = okResult.Value as IEnumerable<EventoDto>;
            Assert.IsNotNull(returnedEventos);
            Assert.AreEqual(1, returnedEventos.Count());
            Assert.AreEqual("Concierto Rock", returnedEventos.First().NombreEvento);
        }

        [TestMethod]
        public async Task CrearEvento_ReturnsOkResult_WithCreatedEvento()
        {
            // Arrange
            Assert.IsNotNull(_mockEventoService);
            Assert.IsNotNull(_controller);

            var createDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Concierto de música pop",
                IdVenue = 1,
                CapacidadMaxima = 500,
                FechaHoraInicio = DateTime.Now.AddDays(1),
                FechaHoraFin = DateTime.Now.AddDays(1).AddHours(2),
                PrecioEntrada = 45.5m,
                TipoEventoId = "CON"
            };

            var expectedEvento = new Evento
            {
                EventoId = 10,
                NombreEvento = "Concierto Pop",
                Descripcion = "Concierto de música pop",
                VenueId = 1,
                Capacidad = 500,
                FechaInicio = createDto.FechaHoraInicio,
                FechaFin = createDto.FechaHoraFin,
                Precio = 45.5f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo
            };

            _mockEventoService.Setup(s => s.CrearEventoAsync(createDto))
                .ReturnsAsync(Result<Evento>.Success(expectedEvento));

            // Act
            var result = await _controller.CrearEvento(createDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEvento = okResult.Value as Evento;
            Assert.IsNotNull(returnedEvento);
            Assert.AreEqual(10, returnedEvento.EventoId);
            Assert.AreEqual("Concierto Pop", returnedEvento.NombreEvento);
        }

        [TestMethod]
        public async Task CrearEvento_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            Assert.IsNotNull(_mockEventoService);
            Assert.IsNotNull(_controller);

            var createDto = new CrearEventoDto
            {
                Titulo = "Concierto Pop",
                Descripcion = "Concierto de música pop",
                IdVenue = 1,
                CapacidadMaxima = 500,
                FechaHoraInicio = DateTime.Now.AddDays(1),
                FechaHoraFin = DateTime.Now.AddDays(1).AddHours(2),
                PrecioEntrada = 45.5m,
                TipoEventoId = "CON"
            };

            _mockEventoService.Setup(s => s.CrearEventoAsync(createDto))
                .ReturnsAsync(Result<Evento>.Failure("El venue no tiene capacidad suficiente"));

            // Act
            var result = await _controller.CrearEvento(createDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            
            // Usamos reflexión o dynamic para verificar la propiedad del objeto anónimo retornado
            dynamic? errorMsg = badRequestResult.Value;
            Assert.IsNotNull(errorMsg);
        }

        [TestMethod]
        public async Task CrearEvento_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            Assert.IsNotNull(_controller);
            _controller.ModelState.AddModelError("Titulo", "The Titulo field is required");

            var createDto = new CrearEventoDto
            {
                Titulo = "",
                Descripcion = "Concierto de música pop",
                IdVenue = 1,
                CapacidadMaxima = 500,
                FechaHoraInicio = DateTime.Now.AddDays(1),
                FechaHoraFin = DateTime.Now.AddDays(1).AddHours(2),
                PrecioEntrada = 45.5m,
                TipoEventoId = "CON"
            };

            // Act
            var result = await _controller.CrearEvento(createDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task GetReporteEventos_ReturnsOkResult_WithListOfReporteEventos()
        {
            // Arrange
            Assert.IsNotNull(_mockEventoService);
            Assert.IsNotNull(_controller);

            var expectedReport = new List<ReporteEventoDto>
            {
                new ReporteEventoDto
                {
                    NombreEvento = "Concierto Rock",
                    Descripcion = "Concierto",
                    Capacidad = 100,
                    Precio = 50f,
                    TipoEventoId = "CON",
                    EstadoEventoId = EventStatusConstants.Activo,
                    Venue = new VenueDto { Nombre = "Estadio" },
                    Ocupacion = 9,
                    ReservasDisponibles = 89,
                    ReservasVendidas = 8,
                    TotalIngresos = 400f
                }
            };

            _mockEventoService.Setup(s => s.GetReporteEventosAsync())
                .ReturnsAsync(expectedReport);

            // Act
            var result = await _controller.GetReporteEventos();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedReport = okResult.Value as IEnumerable<ReporteEventoDto>;
            Assert.IsNotNull(returnedReport);
            Assert.AreEqual(1, returnedReport.Count());
            Assert.AreEqual("Concierto Rock", returnedReport.First().NombreEvento);
            Assert.AreEqual(9, returnedReport.First().Ocupacion);
            Assert.AreEqual(89, returnedReport.First().ReservasDisponibles);
            Assert.AreEqual(8, returnedReport.First().ReservasVendidas);
            Assert.AreEqual(400f, returnedReport.First().TotalIngresos);
        }

        [TestMethod]
        public async Task GetEventoById_ReturnsOkResult_WithEventoDto()
        {
            // Arrange
            Assert.IsNotNull(_mockEventoService);
            Assert.IsNotNull(_controller);

            var expectedEvento = new EventoDto
            {
                NombreEvento = "Concierto Rock",
                Descripcion = "Concierto",
                Capacidad = 100,
                Precio = 50f,
                TipoEventoId = "CON",
                EstadoEventoId = EventStatusConstants.Activo,
                Venue = new VenueDto { Nombre = "Estadio" },
                SoldOut = false
            };

            _mockEventoService.Setup(s => s.GetEventoByIdAsync(1))
                .ReturnsAsync(Result<EventoDto>.Success(expectedEvento));

            // Act
            var result = await _controller.GetEventoById(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEvento = okResult.Value as EventoDto;
            Assert.IsNotNull(returnedEvento);
            Assert.AreEqual("Concierto Rock", returnedEvento.NombreEvento);
        }

        [TestMethod]
        public async Task GetEventoById_ReturnsNotFoundResult_WhenEventDoesNotExist()
        {
            // Arrange
            Assert.IsNotNull(_mockEventoService);
            Assert.IsNotNull(_controller);

            _mockEventoService.Setup(s => s.GetEventoByIdAsync(999))
                .ReturnsAsync(Result<EventoDto>.Failure(MessageConstants.EventNotFoundError));

            // Act
            var result = await _controller.GetEventoById(999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            
            dynamic? errorMsg = notFoundResult.Value;
            Assert.IsNotNull(errorMsg);
        }
    }
}
