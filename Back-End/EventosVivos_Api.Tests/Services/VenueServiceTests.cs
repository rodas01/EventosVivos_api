using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventosVivos_Api.Data;
using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventosVivos_Api.Tests.Services
{
    [TestClass]
    public class VenueServiceTests
    {
        private DbContextOptions<ApplicationDbContext> _options;

        public VenueServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EventosVivosTestDB")
                .Options;
        }

        [TestMethod]
        public async Task GetAllVenues_ReturnsAllVenues()
        {
            // Arrange
            using (var context = new ApplicationDbContext(_options))
            {
                if (context.Venues.Any())
                {
                    context.Venues.RemoveRange(context.Venues);
                    context.SaveChanges();
                }

                context.Venues.AddRange(new List<Venue>
                {
                    new Venue { VenueId = 1, Nombre = "Venue 1", CapacidadMaxima = 100, Ubicacion = "Location 1" },
                    new Venue { VenueId = 2, Nombre = "Venue 2", CapacidadMaxima = 200, Ubicacion = "Location 2" }
                });
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var service = new VenueService(context);

                // Act
                var result = await service.GetAllVenues();

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count());
            }
        }
    }
}
