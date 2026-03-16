using SalaFinders.Models;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Interfaces;

public interface ISpaceService
{
    Task<IEnumerable<Space>> GetAllAsync(SpaceFilterDto? filter = null);
    Task<Space?> GetByIdAsync(int id);
    Task<Space> CreateAsync(Space space);
    Task<Space?> UpdateAsync(int id, Space space);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<AvailabilitySlotDto>> GetWeeklyAvailabilityAsync(DateOnly weekStart, SpaceFilterDto? filter = null);
}
