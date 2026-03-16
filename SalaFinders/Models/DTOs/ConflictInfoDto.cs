namespace SalaFinders.Models.DTOs;

public class ConflictInfoDto
{
    public string Message { get; set; } = string.Empty;
    public int? ConflictingReservationId { get; set; }
    public List<AvailabilitySlotDto> AlternativeSlots { get; set; } = [];
}
