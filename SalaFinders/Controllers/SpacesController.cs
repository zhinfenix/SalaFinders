using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalaFinders.Interfaces;
using SalaFinders.Models;
using SalaFinders.Models.DTOs;

namespace SalaFinders.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpacesController : ControllerBase
{
    private readonly ISpaceService _spaceService;

    public SpacesController(ISpaceService spaceService) => _spaceService = spaceService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Space>>> GetAll([FromQuery] SpaceFilterDto? filter)
    {
        var spaces = await _spaceService.GetAllAsync(filter);
        return Ok(spaces);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Space>> GetById(int id)
    {
        var space = await _spaceService.GetByIdAsync(id);
        if (space == null) return NotFound();
        return Ok(space);
    }

    [HttpGet("availability")]
    public async Task<ActionResult<IEnumerable<AvailabilitySlotDto>>> GetWeeklyAvailability(
        [FromQuery] DateOnly? weekStart,
        [FromQuery] SpaceFilterDto? filter)
    {
        var start = weekStart ?? DateOnly.FromDateTime(DateTime.Today);
        var slots = await _spaceService.GetWeeklyAvailabilityAsync(start, filter);
        return Ok(slots);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Space>> Create([FromBody] Space space)
    {
        var created = await _spaceService.CreateAsync(space);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Space>> Update(int id, [FromBody] Space space)
    {
        var updated = await _spaceService.UpdateAsync(id, space);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _spaceService.DeleteAsync(id)) return NotFound();
        return NoContent();
    }
}
