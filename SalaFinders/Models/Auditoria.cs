namespace SalaFinders.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public DateTime FechaEvento { get; set; } = DateTime.Now; 
        public string UsuarioQueCambio { get; set; } = string.Empty; 
        public string Accion { get; set; } = string.Empty;
        public string Detalles { get; set; } = string.Empty; 
    }
}