using EventosVivos_Api.Controllers;
using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventosVivos_Api.Tests.Controllers
{
    [TestClass]
    public class TipoEventosControllerTests
    {
        [TestMethod]
        public async Task GetAll_ReturnsOkResult_WithListOfTipoEventos()
        {
            // Arrange
            var mockService = new Mock<ITipoEventoService>();
            mockService.Setup(service => service.GetAll())
                .ReturnsAsync(new List<TipoEventoDto>
                {
                    new TipoEventoDto { TipoEventoId = "CON", Descripcion = "Concierto" },
                    new TipoEventoDto { TipoEventoId = "DEP", Descripcion = "Deportivo" }
                });

            var controller = new TipoEventosController(mockService.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var tipoEventos = okResult.Value as IEnumerable<TipoEventoDto>;
            Assert.IsNotNull(tipoEventos);
            Assert.AreEqual(2, tipoEventos.Count());
        }
    }
}
