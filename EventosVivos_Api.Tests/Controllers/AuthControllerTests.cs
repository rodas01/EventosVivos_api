using System.Security.Claims;
using EventosVivos_Api.Controllers;
using EventosVivos_Api.Data;
using EventosVivos_Api.DTO.Security;
using EventosVivos_Api.Models.Security;
using EventosVivos_Api.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EventosVivos_Api.Tests.Controllers;

[TestClass]
public class AuthControllerTests
{
    private Mock<UserManager<User>> _mockUserManager = null!;
    private Mock<JwtTokenService> _mockJwtTokenService = null!;
    private ApplicationDbContext _context = null!;
    private AuthController _controller = null!;
    private User _testUser = null!;

    // Helper to create a mock UserManager
    private Mock<UserManager<User>> CreateMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [TestInitialize]
    public void Setup()
    {
        // User
        _testUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            IsActive = true
        };

        // DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Mocks
        _mockUserManager = CreateMockUserManager();
        var mockConfiguration = new Mock<IConfiguration>();
        _mockJwtTokenService = new Mock<JwtTokenService>(mockConfiguration.Object);

        // Controller
        _controller = new AuthController(_mockUserManager.Object, _context, _mockJwtTokenService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsOkAndToken()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "testuser", Password = "Password123" };
        var roles = new List<string> { "User" };
        var tokenInfo = ("test_token", DateTime.UtcNow.AddHours(1));

        _mockUserManager.Setup(um => um.FindByNameAsync(loginRequest.Username))
            .ReturnsAsync(_testUser);

        _mockUserManager.Setup(um => um.CheckPasswordAsync(_testUser, loginRequest.Password))
            .ReturnsAsync(true);

        _mockUserManager.Setup(um => um.GetRolesAsync(_testUser))
            .ReturnsAsync(roles);
        
        _mockUserManager.Setup(um => um.UpdateAsync(_testUser))
            .ReturnsAsync(IdentityResult.Success);

        _mockJwtTokenService.Setup(ts => ts.GenerateToken(_testUser, roles, It.IsAny<string>()))
            .Returns(tokenInfo);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        var loginResponse = (LoginResponse)okResult.Value!;
        
        Assert.AreEqual(tokenInfo.Item1, loginResponse.Token);
        Assert.AreEqual(1, await _context.UserTokens.CountAsync());
        Assert.IsNotNull(_testUser.LastLoginAt);
    }

    [TestMethod]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "nouser", Password = "Password123" };
        _mockUserManager.Setup(um => um.FindByNameAsync(loginRequest.Username))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        _testUser.IsActive = false;
        var loginRequest = new LoginRequest { Username = "testuser", Password = "Password123" };
        _mockUserManager.Setup(um => um.FindByNameAsync(loginRequest.Username))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "testuser", Password = "WrongPassword" };
        _mockUserManager.Setup(um => um.FindByNameAsync(loginRequest.Username))
            .ReturnsAsync(_testUser);
        _mockUserManager.Setup(um => um.CheckPasswordAsync(_testUser, loginRequest.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Logout_WithValidSession_ReturnsOkAndRemovesToken()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString("N");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUser.Id),
            new Claim(JwtTokenStore.SessionIdClaim, sessionId)
        };
        var claimsIdentity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var userToken = new IdentityUserToken<string>
        {
            UserId = _testUser.Id,
            LoginProvider = JwtTokenStore.LoginProvider,
            Name = sessionId,
            Value = sessionId
        };
        _context.UserTokens.Add(userToken);
        await _context.SaveChangesAsync();

        _mockUserManager.Setup(um => um.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        Assert.AreEqual(0, await _context.UserTokens.CountAsync());
    }

    [TestMethod]
    public async Task Logout_WithInvalidSession_ReturnsUnauthorized()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString("N");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUser.Id),
            new Claim(JwtTokenStore.SessionIdClaim, sessionId) // This session does not exist in the DB
        };
        var claimsIdentity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }
}
