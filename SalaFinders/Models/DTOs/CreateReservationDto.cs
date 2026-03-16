using System.ComponentModel.DataAnnotations;

namespace SalaFinders.Models.DTOs;

public class CreateReservationDto
{
    [Required]
    public int SpaceId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    [MaxLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int AttendeeCount { get; set; }
}
