using Microsoft.AspNetCore.Identity;
using SalaFinders.Models;

namespace SalaFinders.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(string email, string password, string fullName, string role);
    Task<string?> LoginAsync(string email, string password);
    Task<(ApplicationUser? User, IList<string> Roles)?> GetUserInfoAsync(string userId);
}
