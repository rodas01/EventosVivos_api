using EventosVivos_Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventosVivos_Api.Tests.Middleware
{
    [TestClass]
    public class ExceptionHandlingMiddlewareTests
    {
        private Mock<ILogger<ExceptionHandlingMiddleware>>? _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        }

        [TestMethod]
        public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
        {
            // Arrange
            Assert.IsNotNull(_mockLogger);
            var context = new DefaultHttpContext();
            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.IsTrue(nextCalled);
            Assert.AreEqual(200, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task InvokeAsync_WhenException_LogsErrorAndReturnsGenericMessage()
        {
            // Arrange
            Assert.IsNotNull(_mockLogger);
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            
            RequestDelegate next = (ctx) =>
            {
                throw new Exception("Test exception");
            };

            var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(500, context.Response.StatusCode);
            Assert.AreEqual("application/json", context.Response.ContentType);

            // Leer cuerpo de la respuesta devuelta por el middleware
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            
            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            Assert.AreEqual("Ha ocurrido un error interno.", result.GetProperty("message").GetString());

            // Verificar que se registró el error en el logger
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
