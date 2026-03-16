using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalaFinders.Interfaces;

namespace SalaFinders.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService) => _auditService = auditService;

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] string? entityType, [FromQuery] int? entityId, [FromQuery] int limit = 100)
    {
        var logs = await _auditService.GetLogsAsync(entityType, entityId, limit);
        return Ok(logs);
    }
}
