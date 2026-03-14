namespace SalaFinders.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; 
        public int ContadorNoShows { get; set; }
        public bool EstaBloqueado { get; set; }
        public DateTime? FechaFinBloqueo { get; set; }
    }
}