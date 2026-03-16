using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalaFinders.Models;

namespace SalaFinders.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        var roles = new[] { "Student", "Staff", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminUser = await userManager.FindByEmailAsync("admin@salafinders.com");
        if (adminUser != null) return;

        var spaces = await context.Spaces.ToListAsync();
        if (spaces.Count == 0) return;

        adminUser = new ApplicationUser { UserName = "admin@salafinders.com", Email = "admin@salafinders.com", FullName = "Admin Sistema" };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");

        var staffUser = await userManager.FindByEmailAsync("staff@salafinders.com");
        if (staffUser == null)
        {
            staffUser = new ApplicationUser { UserName = "staff@salafinders.com", Email = "staff@salafinders.com", FullName = "Staff Usuario" };
            await userManager.CreateAsync(staffUser, "Staff123!");
            await userManager.AddToRoleAsync(staffUser, "Staff");
        }

        var students = new List<ApplicationUser>();
        for (var i = 1; i <= 13; i++)
        {
            var email = $"student{i}@salafinders.com";
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, FullName = $"Estudiante {i}" };
                await userManager.CreateAsync(user, "Student123!");
                await userManager.AddToRoleAsync(user, "Student");
            }
            students.Add(user);
        }

        var allUsers = new List<ApplicationUser> { adminUser, staffUser };
        allUsers.AddRange(students);

        var random = new Random(42);
        var reservations = new List<Reservation>();
        for (var i = 0; i < 30; i++)
        {
            var space = spaces[random.Next(spaces.Count)];
            var user = allUsers[random.Next(allUsers.Count)];
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(random.Next(-7, 14)));
            var startHour = random.Next(8, 18);
            var start = new TimeOnly(startHour, 0);
            var end = start.Add(TimeSpan.FromHours(1));
            var status = space.RequiresApproval
                ? (ReservationStatus)(random.Next(3))
                : ReservationStatus.Approved;
            if (status == (ReservationStatus)3) status = ReservationStatus.Pending;

            reservations.Add(new Reservation
            {
                SpaceId = space.Id,
                UserId = user.Id,
                Date = date,
                StartTime = start,
                EndTime = end,
                Purpose = $"Reunión/Clase {i + 1}",
                AttendeeCount = random.Next(1, Math.Min(space.Capacity, 20)),
                Status = status,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
            });
        }

        var addedReservations = new List<Reservation>();
        foreach (var r in reservations)
        {
            var overlap = addedReservations.Any(x =>
                x.SpaceId == r.SpaceId && x.Date == r.Date &&
                r.StartTime < x.EndTime && r.EndTime > x.StartTime);
            if (!overlap)
            {
                context.Reservations.Add(r);
                await context.SaveChangesAsync();
                addedReservations.Add(r);
            }
        }

        var auditActions = new[] { "Created", "Updated", "StatusChanged", "Approved", "Rejected" };
        var resIds = addedReservations.Select(r => r.Id).ToList();
        for (var i = 0; i < 20; i++)
        {
            var entityId = resIds.Count > 0 ? resIds[random.Next(resIds.Count)] : 1;
            context.AuditLogs.Add(new AuditLog
            {
                EntityType = "Reservation",
                EntityId = entityId,
                Action = auditActions[random.Next(auditActions.Length)],
                UserId = adminUser.Id,
                Timestamp = DateTime.UtcNow.AddHours(-random.Next(1, 72))
            });
        }
        await context.SaveChangesAsync();
    }
}
