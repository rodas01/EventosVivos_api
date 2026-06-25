using EventosVivos_Api.Controllers;
using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventosVivos_Api.Tests.Controllers
{
    [TestClass]
    public class VenuesControllerTests
    {
        [TestMethod]
        public async Task GetVenues_ReturnsOkResult_WithListOfVenues()
        {
            // Arrange
            var mockService = new Mock<IVenueService>();
            mockService.Setup(service => service.GetAllVenues())
                .ReturnsAsync(new List<Venue>
                {
                    new Venue { VenueId = 1, Nombre = "Venue 1", CapacidadMaxima = 100, Ubicacion = "Location 1" },
                    new Venue { VenueId = 2, Nombre = "Venue 2", CapacidadMaxima = 200, Ubicacion = "Location 2" }
                });

            var controller = new VenuesController(mockService.Object);

            // Act
            var result = await controller.GetVenues();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var venues = okResult.Value as IEnumerable<Venue>;
            Assert.IsNotNull(venues);
            Assert.AreEqual(2, new List<Venue>(venues).Count);
        }
    }
}
