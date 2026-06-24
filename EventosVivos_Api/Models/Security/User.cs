using Microsoft.AspNetCore.Identity;

namespace EventosVivos_Api.Models.Security;

public class User : IdentityUser
{
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
