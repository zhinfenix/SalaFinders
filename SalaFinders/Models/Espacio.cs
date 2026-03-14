namespace SalaFinders.Models
{
    public class Espacio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; 
        public int Capacidad { get; set; }
        public string Edificio { get; set; } = string.Empty;
        public List<string> Recursos { get; set; } = new(); 
        public List<string> ProgramasPermitidos { get; set; } = new();
        public bool RequiereAprobacion { get; set; } 
    }
}