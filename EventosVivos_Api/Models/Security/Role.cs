using Microsoft.AspNetCore.Identity;

namespace EventosVivos_Api.Models.Security;

public class Role : IdentityRole
{
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
