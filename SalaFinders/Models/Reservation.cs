namespace SalaFinders.Models;

public enum ReservationStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public class Reservation
{
    public int Id { get; set; }
    public int SpaceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int AttendeeCount { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? RejectedReason { get; set; }
    public bool IsNoShow { get; set; }

    public Space? Space { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public ApplicationUser User { get; set; } = null!;
}
