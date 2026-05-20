using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalaFinders.Interfaces;
using SalaFinders.Models;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService) => _reservationService = reservationService;

    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var (reservation, conflict) = await _reservationService.CreateReservationAsync(userId, dto);
        if (conflict != null)
            return Conflict(new { conflict.Message, conflict.AlternativeSlots });
        if (reservation == null)
            return BadRequest("No se pudo crear la reserva.");
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Reservation>> GetById(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null) return NotFound();
        return Ok(reservation);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetMyReservations()
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var reservations = await _reservationService.GetByUserAsync(userId);
        return Ok(reservations);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetPendingApprovals()
    {
        var reservations = await _reservationService.GetPendingApprovalsAsync();
        return Ok(reservations);
    }

    [HttpGet("no-show-candidates")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetNoShowCandidates(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate)
    {
        var reservations = await _reservationService.GetNoShowCandidatesAsync(fromDate, toDate);
        return Ok(reservations);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Approve(int id)
    {
        var adminUserId = UserId;
        if (string.IsNullOrEmpty(adminUserId)) return Unauthorized();

        var (success, error) = await _reservationService.ApproveAsync(id, adminUserId);
        if (!success) return error == "Reserva no encontrada" ? NotFound() : BadRequest(error);
        return Ok(new { Message = "Reserva aprobada." });
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectDto? dto = null)
    {
        var adminUserId = UserId;
        if (string.IsNullOrEmpty(adminUserId)) return Unauthorized();

        var (success, error) = await _reservationService.RejectAsync(id, adminUserId, dto?.Reason);
        if (!success) return error == "Reserva no encontrada" ? NotFound() : BadRequest(error);
        return Ok(new { Message = "Reserva rechazada." });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = UserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var (success, error) = await _reservationService.CancelAsync(id, userId);
        if (!success) return error == "Reserva no encontrada" ? NotFound() : BadRequest(error);
        return Ok(new { Message = "Reserva cancelada." });
    }

    [HttpPost("{id:int}/no-show")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> MarkNoShow(int id)
    {
        var adminUserId = UserId;
        if (string.IsNullOrEmpty(adminUserId)) return Unauthorized();

        var (success, error) = await _reservationService.MarkNoShowAsync(id, adminUserId);
        if (!success) return error == "Reserva no encontrada" ? NotFound() : BadRequest(error);
        return Ok(new { Message = "Marcado como no-show. Usuario bloqueado si aplica política." });
    }
}

public class RejectDto
{
    public string? Reason { get; set; }
}
