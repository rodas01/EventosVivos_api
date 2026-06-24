using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventosVivos_Api.Models.Security;
using Microsoft.IdentityModel.Tokens;

namespace EventosVivos_Api.Services.Security;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public virtual (string Token, DateTime ExpiresAt) GenerateToken(User user, IEnumerable<string> roles, string sessionId)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtTokenStore.SessionIdClaim, sessionId)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new Claim("full_name", user.FullName));
        }

        claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
