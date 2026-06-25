using EventosVivos_Api.Data;
using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos_Api.Tests.Services
{
    [TestClass]
    public class TipoEventoServiceTests
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
            _context.TiposEventos.AddRange(new List<TipoEvento>
            {
                new TipoEvento { TipoEventoId = "CON", Descripcion = "Concierto" },
                new TipoEvento { TipoEventoId = "DEP", Descripcion = "Deportivo" }
            });
            _context.SaveChanges();
        }

        [TestCleanup]
        public void Teardown()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task GetAll_ReturnsAllTipoEventos()
        {
            // Arrange
            Assert.IsNotNull(_context);
            var service = new TipoEventoService(_context);

            // Act
            var result = await service.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Concierto", result.First(t => t.TipoEventoId == "CON").Descripcion);
            Assert.AreEqual("Deportivo", result.First(t => t.TipoEventoId == "DEP").Descripcion);
        }
    }
}
