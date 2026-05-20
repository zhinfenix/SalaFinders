using SalaFinders.Models;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Interfaces;

public interface IReservationService
{
    Task<(Reservation? Reservation, ConflictInfoDto? Conflict)> CreateReservationAsync(string userId, CreateReservationDto dto);
    Task<Reservation?> GetByIdAsync(int id);
    Task<IEnumerable<Reservation>> GetByUserAsync(string userId);
    Task<IEnumerable<Reservation>> GetPendingApprovalsAsync();
    Task<IEnumerable<Reservation>> GetNoShowCandidatesAsync(DateOnly? fromDate, DateOnly? toDate);
    Task<(bool Success, string? Error)> ApproveAsync(int id, string adminUserId);
    Task<(bool Success, string? Error)> RejectAsync(int id, string adminUserId, string? reason = null);
    Task<(bool Success, string? Error)> CancelAsync(int id, string userId);
    Task<(bool Success, string? Error)> MarkNoShowAsync(int id, string adminUserId);
    Task<bool> HasOverlapAsync(int spaceId, DateOnly date, TimeOnly start, TimeOnly end, int? excludeReservationId = null);
}
