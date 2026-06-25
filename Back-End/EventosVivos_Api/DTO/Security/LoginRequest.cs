using System.ComponentModel.DataAnnotations;

namespace EventosVivos_Api.DTO.Security;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
