using Microsoft.AspNetCore.Identity;

namespace SalaFinders.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    /// <summary>Carrera del estudiante (ej. Ingeniería). Null para Admin/Staff.</summary>
    public string? Program { get; set; }
    public int NoShowCount { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
