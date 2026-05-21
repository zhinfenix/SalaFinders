using System.ComponentModel.DataAnnotations;

namespace SalaFinders.Models.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";

    /// <summary>Carrera académica (requerida para estudiantes).</summary>
    public string? Program { get; set; }
}
