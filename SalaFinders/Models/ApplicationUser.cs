using Microsoft.AspNetCore.Identity;

namespace SalaFinders.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int NoShowCount { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
