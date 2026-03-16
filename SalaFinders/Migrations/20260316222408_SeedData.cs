using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalaFinders.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ADMIN')
                    INSERT INTO AspNetRoles (Id, ConcurrencyStamp, Name, NormalizedName) VALUES ('a1b2c3d4-0001-4000-8000-000000000001', 'b1b2c3d4-0001-4000-8000-000000000001', 'Admin', 'ADMIN');
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'STAFF')
                    INSERT INTO AspNetRoles (Id, ConcurrencyStamp, Name, NormalizedName) VALUES ('a1b2c3d4-0002-4000-8000-000000000002', 'b1b2c3d4-0002-4000-8000-000000000002', 'Staff', 'STAFF');
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'STUDENT')
                    INSERT INTO AspNetRoles (Id, ConcurrencyStamp, Name, NormalizedName) VALUES ('a1b2c3d4-0003-4000-8000-000000000003', 'b1b2c3d4-0003-4000-8000-000000000003', 'Student', 'STUDENT');
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Spaces WHERE Id = 1)
                    INSERT INTO Spaces (Id, AllowedPrograms, Building, Capacity, Name, RequiresApproval, Resources, Type) VALUES (1, 'Ingeniería,Administración', 'Edificio A', 30, 'Sala A101', 1, 'Proyector,Pizarra', 'Room');
                IF NOT EXISTS (SELECT 1 FROM Spaces WHERE Id = 2)
                    INSERT INTO Spaces (Id, AllowedPrograms, Building, Capacity, Name, RequiresApproval, Resources, Type) VALUES (2, 'Ingeniería', 'Edificio B', 20, 'Lab B201', 1, 'Computadoras,Proyector', 'Lab');
                IF NOT EXISTS (SELECT 1 FROM Spaces WHERE Id = 3)
                    INSERT INTO Spaces (Id, AllowedPrograms, Building, Capacity, Name, RequiresApproval, Resources, Type) VALUES (3, 'Deportes,Educación Física', 'Polideportivo', 50, 'Cancha Central', 0, 'Balones', 'Court');
                IF NOT EXISTS (SELECT 1 FROM Spaces WHERE Id = 4)
                    INSERT INTO Spaces (Id, AllowedPrograms, Building, Capacity, Name, RequiresApproval, Resources, Type) VALUES (4, 'Administración', 'Edificio C', 15, 'Sala C301', 0, 'Pizarra', 'Room');
                IF NOT EXISTS (SELECT 1 FROM Spaces WHERE Id = 5)
                    INSERT INTO Spaces (Id, AllowedPrograms, Building, Capacity, Name, RequiresApproval, Resources, Type) VALUES (5, 'Ingeniería,Diseño', 'Edificio D', 25, 'Lab D102', 1, 'Computadoras,Impresora', 'Lab');
                IF NOT EXISTS (SELECT 1 FROM Spaces WHERE Id = 6)
                    INSERT INTO Spaces (Id, AllowedPrograms, Building, Capacity, Name, RequiresApproval, Resources, Type) VALUES (6, 'Todos', 'Edificio E', 40, 'Sala E201', 1, 'Proyector,Micrófono', 'Room');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-0001-4000-8000-000000000001");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-0002-4000-8000-000000000002");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-0003-4000-8000-000000000003");

            migrationBuilder.DeleteData(
                table: "Spaces",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Spaces",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Spaces",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Spaces",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Spaces",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Spaces",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
