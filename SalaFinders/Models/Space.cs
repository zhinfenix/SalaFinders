namespace SalaFinders.Models;

public class Space
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Building { get; set; } = string.Empty;
    public List<string> Resources { get; set; } = [];
    public List<string> AllowedPrograms { get; set; } = [];
    public bool RequiresApproval { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = [];
}
