
using Microsoft.EntityFrameworkCore;
using SalaFinders.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Agrega estas líneas para que el proyecto reconozca Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Esto activa la generación del archivo JSON y la interfaz visual
    app.UseSwagger();
    app.UseSwaggerUI();

    // Si quieres conservar la nueva forma de .NET 9, puedes dejar esta:
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // ¡Esta es vital!

app.Run();