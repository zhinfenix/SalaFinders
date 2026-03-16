using Microsoft.EntityFrameworkCore;
using SalaFinders.Data;
using SalaFinders.Interfaces;
using SalaFinders.Models;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Services;

public class SpaceService : ISpaceService
{
    private readonly ApplicationDbContext _context;

    public SpaceService(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Space>> GetAllAsync(SpaceFilterDto? filter = null)
    {
        var query = _context.Spaces.AsQueryable();
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Type))
                query = query.Where(s => s.Type == filter.Type);
            if (filter.MinCapacity.HasValue)
                query = query.Where(s => s.Capacity >= filter.MinCapacity.Value);
            if (!string.IsNullOrEmpty(filter.Building))
                query = query.Where(s => s.Building == filter.Building);
            if (!string.IsNullOrEmpty(filter.RequiredResource))
                query = query.Where(s => s.Resources.Contains(filter.RequiredResource));
        }
        return await query.ToListAsync();
    }

    public async Task<Space?> GetByIdAsync(int id) =>
        await _context.Spaces.FindAsync(id);

    public async Task<Space> CreateAsync(Space space)
    {
        _context.Spaces.Add(space);
        await _context.SaveChangesAsync();
        return space;
    }

    public async Task<Space?> UpdateAsync(int id, Space space)
    {
        var existing = await _context.Spaces.FindAsync(id);
        if (existing == null) return null;
        existing.Name = space.Name;
        existing.Type = space.Type;
        existing.Capacity = space.Capacity;
        existing.Building = space.Building;
        existing.Resources = space.Resources;
        existing.AllowedPrograms = space.AllowedPrograms;
        existing.RequiresApproval = space.RequiresApproval;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var space = await _context.Spaces.FindAsync(id);
        if (space == null) return false;
        _context.Spaces.Remove(space);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AvailabilitySlotDto>> GetWeeklyAvailabilityAsync(DateOnly weekStart, SpaceFilterDto? filter = null)
    {
        var spaces = await GetAllAsync(filter);
        var slots = new List<AvailabilitySlotDto>();
        var endOfWeek = weekStart.AddDays(7);

        foreach (var space in spaces)
        {
            for (var d = weekStart; d < endOfWeek; d = d.AddDays(1))
            {
                var reserved = await _context.Reservations
                    .Where(r => r.SpaceId == space.Id && r.Date == d && r.Status == ReservationStatus.Approved)
                    .Select(r => new { r.StartTime, r.EndTime })
                    .ToListAsync();

                var dayStart = new TimeOnly(8, 0);
                var dayEnd = new TimeOnly(20, 0);
                var current = dayStart;

                while (current.Add(TimeSpan.FromMinutes(30)) <= dayEnd)
                {
                    var slotEnd = current.Add(TimeSpan.FromMinutes(30));
                    var hasConflict = reserved.Any(r => Overlaps(current, slotEnd, r.StartTime, r.EndTime));
                    if (!hasConflict)
                        slots.Add(new AvailabilitySlotDto
                        {
                            SpaceId = space.Id,
                            SpaceName = space.Name,
                            SpaceType = space.Type,
                            Capacity = space.Capacity,
                            Date = d,
                            StartTime = current,
                            EndTime = slotEnd
                        });
                    current = slotEnd;
                }
            }
        }
        return slots;
    }

    private static bool Overlaps(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2) =>
        s1 < e2 && e1 > s2;
}
