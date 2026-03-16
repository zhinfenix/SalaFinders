namespace SalaFinders.Models.DTOs;

public class SpaceFilterDto
{
    public string? Type { get; set; }
    public int? MinCapacity { get; set; }
    public string? Building { get; set; }
    public string? RequiredResource { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}
