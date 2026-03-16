using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalaFinders.Interfaces;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var result = await _authService.RegisterAsync(model.Email, model.Password, model.FullName, model.Role);
        if (result.Succeeded)
            return Ok(new { Message = $"Usuario {model.Email} creado con éxito." });
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var token = await _authService.LoginAsync(model.Email, model.Password);
        if (token != null)
            return Ok(new { Token = token });
        return Unauthorized(new { Message = "Credenciales incorrectas o usuario bloqueado." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var info = await _authService.GetUserInfoAsync(userId);
        if (info == null) return NotFound();
        var (user, roles) = info.Value;
        return Ok(new
        {
            user!.Id,
            user.Email,
            user.FullName,
            user.NoShowCount,
            BlockedUntil = user.BlockedUntil,
            IsBlocked = user.BlockedUntil > DateTime.UtcNow,
            Roles = roles
        });
    }
}
