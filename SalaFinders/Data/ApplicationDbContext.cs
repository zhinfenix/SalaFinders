using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalaFinders.Models;

namespace SalaFinders.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        const string adminRoleId = "a1b2c3d4-0001-4000-8000-000000000001";
        const string staffRoleId = "a1b2c3d4-0002-4000-8000-000000000002";
        const string studentRoleId = "a1b2c3d4-0003-4000-8000-000000000003";

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "b1b2c3d4-0001-4000-8000-000000000001" },
            new IdentityRole { Id = staffRoleId, Name = "Staff", NormalizedName = "STAFF", ConcurrencyStamp = "b1b2c3d4-0002-4000-8000-000000000002" },
            new IdentityRole { Id = studentRoleId, Name = "Student", NormalizedName = "STUDENT", ConcurrencyStamp = "b1b2c3d4-0003-4000-8000-000000000003" }
        );

        builder.Entity<Space>().HasData(
            new Space { Id = 1, Name = "Sala A101", Type = "Room", Capacity = 30, Building = "Edificio A", Resources = ["Proyector", "Pizarra"], AllowedPrograms = ["Ingeniería", "Administración"], RequiresApproval = true },
            new Space { Id = 2, Name = "Lab B201", Type = "Lab", Capacity = 20, Building = "Edificio B", Resources = ["Computadoras", "Proyector"], AllowedPrograms = ["Ingeniería"], RequiresApproval = true },
            new Space { Id = 3, Name = "Cancha Central", Type = "Court", Capacity = 50, Building = "Polideportivo", Resources = ["Balones"], AllowedPrograms = ["Deportes", "Educación Física"], RequiresApproval = false },
            new Space { Id = 4, Name = "Sala C301", Type = "Room", Capacity = 15, Building = "Edificio C", Resources = ["Pizarra"], AllowedPrograms = ["Administración"], RequiresApproval = false },
            new Space { Id = 5, Name = "Lab D102", Type = "Lab", Capacity = 25, Building = "Edificio D", Resources = ["Computadoras", "Impresora"], AllowedPrograms = ["Ingeniería", "Diseño"], RequiresApproval = true },
            new Space { Id = 6, Name = "Sala E201", Type = "Room", Capacity = 40, Building = "Edificio E", Resources = ["Proyector", "Micrófono"], AllowedPrograms = ["Todos"], RequiresApproval = true }
        );

        builder.Entity<Space>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Building).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Resources).HasConversion(
                v => string.Join(',', v ?? new List<string>()),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.AllowedPrograms).HasConversion(
                v => string.Join(',', v ?? new List<string>()),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        builder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Purpose).HasMaxLength(500);
            entity.HasOne(e => e.Space).WithMany(s => s.Reservations).HasForeignKey(e => e.SpaceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.SpaceId, e.Date, e.StartTime, e.EndTime });
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Program).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.NoShowCount).HasDefaultValue(0);
            entity.Property(e => e.BlockedUntil).IsRequired(false);
        });
    }
}
