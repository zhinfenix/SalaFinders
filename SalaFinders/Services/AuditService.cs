using Microsoft.EntityFrameworkCore;
using SalaFinders.Data;
using SalaFinders.Interfaces;
using SalaFinders.Models;

namespace SalaFinders.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context) => _context = context;

    public async Task LogAsync(string entityType, int entityId, string action, string? userId, string? oldValues = null, string? newValues = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType = null, int? entityId = null, int limit = 100)
    {
        var query = _context.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(entityType)) query = query.Where(a => a.EntityType == entityType);
        if (entityId.HasValue) query = query.Where(a => a.EntityId == entityId.Value);
        return await query.OrderByDescending(a => a.Timestamp).Take(limit).ToListAsync();
    }
}
