using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalaFinders.Data;
using SalaFinders.Interfaces;
using SalaFinders.Models;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Services;

public class ReservationService : IReservationService
{
    private const int NoShowThreshold = 2;
    private const int BlockDays = 7;

    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ISpaceService _spaceService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReservationService(
        ApplicationDbContext context,
        IAuditService auditService,
        ISpaceService spaceService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _auditService = auditService;
        _spaceService = spaceService;
        _userManager = userManager;
    }

    public async Task<(Reservation? Reservation, ConflictInfoDto? Conflict)> CreateReservationAsync(string userId, CreateReservationDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (null, new ConflictInfoDto { Message = "Usuario no encontrado" });
        if (user.BlockedUntil > DateTime.UtcNow)
            return (null, new ConflictInfoDto { Message = "Usuario bloqueado por política de no-show. Intente después del " + user.BlockedUntil?.ToString("yyyy-MM-dd") });

        var space = await _context.Spaces.FindAsync(dto.SpaceId);
        if (space == null)
            return (null, new ConflictInfoDto { Message = "Espacio no encontrado" });

        if (dto.AttendeeCount > space.Capacity)
            return (null, new ConflictInfoDto { Message = $"El espacio tiene capacidad para {space.Capacity} personas" });

        if (dto.EndTime <= dto.StartTime)
            return (null, new ConflictInfoDto { Message = "La hora de fin debe ser posterior a la de inicio" });

        var roles = await _userManager.GetRolesAsync(user);
        var isStudentOnly = roles.Contains("Student") && !roles.Contains("Admin") && !roles.Contains("Staff");
        if (isStudentOnly)
        {
            if (string.IsNullOrWhiteSpace(user.Program))
                return (null, new ConflictInfoDto { Message = "Debes registrar tu carrera antes de reservar espacios." });
            if (!AcademicPrograms.CanAccessSpace(space.AllowedPrograms, user.Program))
                return (null, new ConflictInfoDto
                {
                    Message = $"Este espacio solo está disponible para: {string.Join(", ", space.AllowedPrograms)}."
                });
        }

        var hasOverlap = await HasOverlapAsync(dto.SpaceId, dto.Date, dto.StartTime, dto.EndTime);
        if (hasOverlap)
        {
            var alternatives = await GetAlternativeSlotsAsync(dto.SpaceId, dto.Date, dto.StartTime, dto.EndTime, dto.AttendeeCount);
            return (null, new ConflictInfoDto
            {
                Message = "Ya existe una reserva aprobada en ese horario para este espacio.",
                AlternativeSlots = alternatives.ToList()
            });
        }

        var status = space.RequiresApproval ? ReservationStatus.Pending : ReservationStatus.Approved;
        var reservation = new Reservation
        {
            SpaceId = dto.SpaceId,
            UserId = userId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Purpose = dto.Purpose,
            AttendeeCount = dto.AttendeeCount,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Reservation", reservation.Id, "Created", userId, null,
            System.Text.Json.JsonSerializer.Serialize(new { reservation.Status, reservation.Date, reservation.StartTime, reservation.EndTime }));

        return (reservation, null);
    }

    public async Task<Reservation?> GetByIdAsync(int id) =>
        await _context.Reservations.Include(r => r.Space).Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Reservation>> GetByUserAsync(string userId) =>
        await _context.Reservations.Include(r => r.Space).Where(r => r.UserId == userId).OrderByDescending(r => r.Date).ThenBy(r => r.StartTime).ToListAsync();

    public async Task<IEnumerable<Reservation>> GetPendingApprovalsAsync() =>
        await _context.Reservations.Include(r => r.Space).Include(r => r.User).Where(r => r.Status == ReservationStatus.Pending).ToListAsync();

    public async Task<IEnumerable<Reservation>> GetNoShowCandidatesAsync(DateOnly? fromDate, DateOnly? toDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var from = fromDate ?? today.AddDays(-14);
        var to = toDate ?? today;
        return await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.User)
            .Where(r => r.Status == ReservationStatus.Approved
                && !r.IsNoShow
                && r.Date >= from
                && r.Date <= to)
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int id, string adminUserId)
    {
        var reservation = await _context.Reservations.Include(r => r.Space).FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return (false, "Reserva no encontrada");
        if (reservation.Status != ReservationStatus.Pending) return (false, "La reserva no está pendiente de aprobación");

        var hasOverlap = await HasOverlapAsync(reservation.SpaceId, reservation.Date, reservation.StartTime, reservation.EndTime, id);
        if (hasOverlap) return (false, "Existe conflicto con otra reserva aprobada");

        var oldStatus = reservation.Status.ToString();
        reservation.Status = ReservationStatus.Approved;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Reservation", id, "StatusChanged", adminUserId, oldStatus, "Approved");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(int id, string adminUserId, string? reason = null)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return (false, "Reserva no encontrada");
        if (reservation.Status != ReservationStatus.Pending) return (false, "La reserva no está pendiente");

        var oldStatus = reservation.Status.ToString();
        reservation.Status = ReservationStatus.Rejected;
        reservation.RejectedReason = reason;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Reservation", id, "StatusChanged", adminUserId, oldStatus, "Rejected");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id, string userId)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return (false, "Reserva no encontrada");
        if (reservation.UserId != userId) return (false, "No tiene permiso para cancelar esta reserva");

        var oldStatus = reservation.Status.ToString();
        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Reservation", id, "StatusChanged", userId, oldStatus, "Cancelled");
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkNoShowAsync(int id, string adminUserId)
    {
        var reservation = await _context.Reservations.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return (false, "Reserva no encontrada");
        if (reservation.Status != ReservationStatus.Approved) return (false, "Solo se pueden marcar no-show reservas aprobadas");

        reservation.IsNoShow = true;
        reservation.UpdatedAt = DateTime.UtcNow;

        var user = reservation.User;
        user.NoShowCount++;
        if (user.NoShowCount >= NoShowThreshold)
            user.BlockedUntil = DateTime.UtcNow.AddDays(BlockDays);

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Reservation", id, "MarkedNoShow", adminUserId, null, $"User blocked until: {user.BlockedUntil}");
        return (true, null);
    }

    public async Task<bool> HasOverlapAsync(int spaceId, DateOnly date, TimeOnly start, TimeOnly end, int? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(r => r.SpaceId == spaceId && r.Date == date && r.Status == ReservationStatus.Approved);
        if (excludeReservationId.HasValue)
            query = query.Where(r => r.Id != excludeReservationId.Value);

        var overlapping = await query.AnyAsync(r => start < r.EndTime && end > r.StartTime);
        return overlapping;
    }

    private async Task<IEnumerable<AvailabilitySlotDto>> GetAlternativeSlotsAsync(int spaceId, DateOnly date, TimeOnly start, TimeOnly end, int attendeeCount)
    {
        var filter = new SpaceFilterDto { Date = date, MinCapacity = attendeeCount };
        var slots = await _spaceService.GetWeeklyAvailabilityAsync(date, filter);
        return slots.Where(s => s.SpaceId == spaceId && s.Date == date).Take(5);
    }
}
