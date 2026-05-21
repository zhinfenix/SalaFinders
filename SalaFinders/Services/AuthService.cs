using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SalaFinders.Interfaces;
using SalaFinders.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SalaFinders.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<IdentityResult> RegisterAsync(string email, string password, string fullName, string role, string? program = null)
    {
        if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(program))
                return IdentityResult.Failed(new IdentityError { Description = "Los estudiantes deben indicar su carrera." });
            if (!AcademicPrograms.IsValidSelectable(program))
                return IdentityResult.Failed(new IdentityError { Description = "La carrera indicada no es válida." });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            Program = string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase) ? program!.Trim() : null
        };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
            await _userManager.AddToRoleAsync(user, role);
        }
        return result;
    }

    public async Task<(bool Success, string? Error)> UpdateProgramAsync(string userId, string program)
    {
        if (!AcademicPrograms.IsValidSelectable(program))
            return (false, "La carrera indicada no es válida.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "Usuario no encontrado");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Student"))
            return (false, "Solo los estudiantes pueden actualizar su carrera.");

        user.Program = program.Trim();
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? (true, null) : (false, string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return null;

        if (user.BlockedUntil.HasValue && user.BlockedUntil > DateTime.UtcNow)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return GenerateJwtToken(user, roles);
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddHours(3),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(ApplicationUser? User, IList<string> Roles)?> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return (user, roles);
    }
}
