using System.Security.Claims;
using EventosVivos_Api.DTO.Security;
using EventosVivos_Api.Data;
using EventosVivos_Api.Models.Security;
using EventosVivos_Api.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(UserManager<User> userManager, ApplicationDbContext context, JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);

        if (user is null || !user.IsActive || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var sessionId = Guid.NewGuid().ToString("N");
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roles, sessionId);

        _context.UserTokens.Add(new IdentityUserToken<string>
        {
            UserId = user.Id,
            LoginProvider = JwtTokenStore.LoginProvider,
            Name = sessionId,
            Value = sessionId
        });
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = User.FindFirstValue(JwtTokenStore.SessionIdClaim);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var token = await _context.UserTokens.FirstOrDefaultAsync(userToken =>
            userToken.UserId == user.Id
            && userToken.LoginProvider == JwtTokenStore.LoginProvider
            && userToken.Value == sessionId);

        if (token is null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        _context.UserTokens.Remove(token);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Session closed." });
    }
}
