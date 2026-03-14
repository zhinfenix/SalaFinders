using Microsoft.EntityFrameworkCore;
using SalaFinders.Models;

namespace SalaFinders.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Espacio> Espacios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; } 
    }
}