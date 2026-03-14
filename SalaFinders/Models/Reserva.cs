namespace SalaFinders.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int EspacioId { get; set; }
        public int UsuarioId { get; set; } 
        public DateTime Fecha { get; set; }
        
        public TimeSpan HoraInicio { get; set; }
        
        public TimeSpan HoraFin { get; set; }
        
        public string Proposito { get; set; } = string.Empty; 
        public int CantidadAsistentes { get; set; }
       
        public string Estado { get; set; } = "Pending";
    }
}