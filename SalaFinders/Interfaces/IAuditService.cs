using SalaFinders.Models;

namespace SalaFinders.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityType, int entityId, string action, string? userId, string? oldValues = null, string? newValues = null);
    Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType = null, int? entityId = null, int limit = 100);
}
