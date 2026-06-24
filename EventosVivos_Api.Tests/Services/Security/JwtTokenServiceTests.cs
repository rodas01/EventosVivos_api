using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventosVivos_Api.Models.Security;
using EventosVivos_Api.Services.Security;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EventosVivos_Api.Tests.Services.Security;

[TestClass]
public class JwtTokenServiceTests
{
    private Mock<IConfiguration> _mockConfiguration = null!;
    private Mock<IConfigurationSection> _mockJwtSection = null!;
    private JwtTokenService _tokenService = null!;
    private User _testUser = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();

        // Default setup for a valid configuration
        _mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(_mockJwtSection.Object);
        _mockJwtSection.Setup(s => s["SecretKey"]).Returns("a_very_secret_key_that_is_long_enough_for_hs256");
        _mockJwtSection.Setup(s => s["Issuer"]).Returns("TestIssuer");
        _mockJwtSection.Setup(s => s["Audience"]).Returns("TestAudience");
        
        _tokenService = new JwtTokenService(_mockConfiguration.Object);

        _testUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User"
        };
    }

    [TestMethod]
    public void GenerateToken_WithValidUser_ReturnsValidTokenAndExpiry()
    {
        // Arrange
        var roles = new List<string> { "Admin", "User" };
        var sessionId = Guid.NewGuid().ToString("N");

        // Act
        var (tokenString, expiresAt) = _tokenService.GenerateToken(_testUser, roles, sessionId);

        // Assert
        Assert.IsNotNull(tokenString);
        Assert.IsTrue(expiresAt > DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(tokenString);

        Assert.AreEqual(_testUser.Id, decodedToken.Subject);
        Assert.AreEqual(_testUser.UserName, decodedToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
        Assert.AreEqual(sessionId, decodedToken.Claims.First(c => c.Type == JwtTokenStore.SessionIdClaim).Value);
        Assert.AreEqual(_testUser.Email, decodedToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.AreEqual(_testUser.FullName, decodedToken.Claims.First(c => c.Type == "full_name").Value);
        
        var roleClaims = decodedToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.AreEqual(2, roleClaims.Count);
        Assert.IsTrue(roleClaims.Any(c => c.Value == "Admin"));
        Assert.IsTrue(roleClaims.Any(c => c.Value == "User"));
    }

    [TestMethod]
    public void GenerateToken_WhenSecretKeyIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockJwtSection.Setup(s => s["SecretKey"]).Returns((string)null!);
        var roles = new List<string>();
        var sessionId = Guid.NewGuid().ToString();

        // Act & Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(() => 
            _tokenService.GenerateToken(_testUser, roles, sessionId));
        
        Assert.AreEqual("Jwt:SecretKey is not configured.", ex.Message);
    }

    [TestMethod]
    public void GenerateToken_WithMinimalUserData_OmitsOptionalClaims()
    {
        // Arrange
        _testUser.Email = null;
        _testUser.FullName = null;
        var roles = new List<string>();
        var sessionId = Guid.NewGuid().ToString("N");

        // Act
        var (tokenString, _) = _tokenService.GenerateToken(_testUser, roles, sessionId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(tokenString);

        Assert.IsNull(decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email));
        Assert.IsNull(decodedToken.Claims.FirstOrDefault(c => c.Type == "full_name"));
        Assert.AreEqual(_testUser.UserName, decodedToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
    }
}
