# SalaFinders - Room/Lab/Court Booking API

Sistema de reserva de salas, laboratorios y canchas con flujo de aprobación, detección de conflictos y auditoría.

## Requisitos del sistema

- .NET 10.0
- SQL Server (LocalDB o instancia completa)
- dotnet-ef (herramienta global)

## Configuración de SQL Server

### 1. Cadena de conexión

Edita `SalaFinders/appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SalaFindersDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Variantes comunes:**
- **LocalDB:** `Server=(localdb)\\mssqllocaldb;Database=SalaFindersDb;Trusted_Connection=True;TrustServerCertificate=True`
- **Instancia nombrada:** `Server=localhost\\SQLEXPRESS;Database=SalaFindersDb;...`
- **Con usuario/contraseña:** `Server=localhost;Database=SalaFindersDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True`

### 2. Aplicar migraciones (incluye seed de datos)

```bash
cd SalaFinders
dotnet ef database update
```

La migración `SeedData` inserta automáticamente los roles (Admin, Staff, Student) y los 6 espacios iniciales mediante `OnModelCreating` en `ApplicationDbContext`. Es idempotente: si los datos ya existen, no duplica.

### 3. Ejecutar la API

```bash
dotnet run
```

La documentación Scalar estará en: **https://localhost:7xxx/scalar/v1** (o el puerto que muestre la consola).

## Estructura del proyecto

```
SalaFinders/
├── Controllers/       # Auth, Spaces, Reservations, Audit
├── Data/              # ApplicationDbContext, DbSeeder
├── Interfaces/        # IAuthService, ISpaceService, IReservationService, IAuditService
├── Models/            # Space, Reservation, AuditLog, ApplicationUser
│   └── DTOs/          # DTOs para requests/responses
├── Services/          # Implementaciones de servicios
└── Migrations/        # Migraciones EF Core
```

## Usuarios de prueba (seed)

| Email | Password | Rol |
|-------|----------|-----|
| admin@salafinders.com | Admin123! | Admin |
| staff@salafinders.com | Staff123! | Staff |
| student1@salafinders.com ... student13@salafinders.com | Student123! | Student |

## Endpoints principales

- `POST /api/auth/register` - Registro
- `POST /api/auth/login` - Login (devuelve JWT)
- `GET /api/auth/me` - Usuario actual (incluye NoShowCount, BlockedUntil, IsBlocked)
- `GET /api/spaces` - Listar espacios (filtros: type, capacity, building, resource)
- `GET /api/spaces/availability?weekStart=...` - Disponibilidad semanal
- `POST /api/reservations` - Crear reserva (requiere JWT)
- `GET /api/reservations/my` - Mis reservas
- `POST /api/reservations/{id}/approve` - Aprobar (Admin/Staff)
- `POST /api/reservations/{id}/reject` - Rechazar (Admin/Staff)
- `POST /api/reservations/{id}/cancel` - Cancelar (propietario)
- `POST /api/reservations/{id}/no-show` - Marcar no-show (Admin/Staff)
- `GET /api/audit` - Logs de auditoría (Admin)

## Rama de desarrollo

El desarrollo está en la rama `sala-finders-feature`. Para hacer merge a main:

```bash
git checkout main
git merge sala-finders-feature
```
