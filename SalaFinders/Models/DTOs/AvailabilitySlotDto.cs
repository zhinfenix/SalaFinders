namespace SalaFinders.Models.DTOs;

public class AvailabilitySlotDto
{
    public int SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public string SpaceType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
