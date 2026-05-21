# SalaFinders — API Backend

API REST para **reserva de salas, laboratorios y canchas** con JWT, roles (Admin / Staff / Student), flujo de aprobación, detección de conflictos, política de **no-show**, auditoría y catálogo de espacios.

**Repositorio frontend:** [SamuelBhoop/SalaFinder-By-Samuel-y-Jose](https://github.com/SamuelBhoop/SalaFinder-By-Samuel-y-Jose)

---

## Stack

| Tecnología | Uso |
|------------|-----|
| ASP.NET Core (.NET 10) | API REST |
| Entity Framework Core | ORM y migraciones |
| SQL Server | Base de datos |
| ASP.NET Identity + JWT | Autenticación y roles |

---

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express o instancia completa)
- [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (herramienta global)

```bash
dotnet tool install --global dotnet-ef
```

---

## Configuración

### 1. Cadena de conexión

Edita `SalaFinders/appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SalaFindersDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Variantes:**

- **LocalDB:** `Server=(localdb)\\mssqllocaldb;Database=SalaFindersDb;Trusted_Connection=True;TrustServerCertificate=True`
- **SQL Express:** `Server=localhost\\SQLEXPRESS;Database=SalaFindersDb;...`
- **Usuario/contraseña:** `Server=localhost;Database=SalaFindersDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True`

### 2. Migraciones y seed

```bash
cd SalaFinders
dotnet ef database update
```

Crea roles, usuarios demo, espacios y datos iniciales (idempotente en re-ejecución según el seed configurado).

### 3. Ejecutar la API

```bash
dotnet run
```

| Perfil | URL típica |
|--------|------------|
| HTTP | http://localhost:5155 |
| HTTPS + Scalar | https://localhost:7036/scalar/v1 |

Documentación interactiva: **Scalar** en `/scalar/v1`.

### 4. CORS (frontend)

En `appsettings.json`:

```json
"Cors": {
  "Origins": [ "http://localhost:5173" ]
}
```

Para otro puerto del frontend, añade el origen correspondiente en el array `Origins`.

---

## Usuarios de prueba (seed)

| Email | Contraseña | Rol |
|-------|------------|-----|
| admin@salafinders.com | Admin123! | Admin |
| staff@salafinders.com | Staff123! | Staff |
| student1@salafinders.com … student13@salafinders.com | Student123! | Student |

6 espacios precargados (salas, laboratorios, cancha).

---

## Política de no-show

| Regla | Valor |
|-------|--------|
| Umbral | 2 no-shows acumulados |
| Bloqueo | 7 días (`BlockedUntil`) |
| Marcar no-show | Solo reservas **Approved** sin `IsNoShow` |
| Login | Rechazado si `BlockedUntil > now` |
| Crear reserva | 409 si el usuario está bloqueado |

---

## Endpoints

Base: `/api`

### Autenticación — `/api/auth`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/register` | No | Registro (rol Student) |
| POST | `/login` | No | JWT (falla si bloqueado) |
| GET | `/me` | Sí | Perfil + `noShowCount`, `blockedUntil`, `isBlocked` |
| GET | `/programs` | Sí | Carreras académicas disponibles |
| PUT | `/program` | Sí | Actualizar carrera del usuario |

### Espacios — `/api/spaces`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/` | No | Listar (filtros opcionales) |
| GET | `/{id}` | No | Detalle |
| GET | `/availability` | No | Disponibilidad semanal (slots 30 min) |
| POST | `/` | Admin | Crear |
| PUT | `/{id}` | Admin | Actualizar |
| DELETE | `/{id}` | Admin | Eliminar |

### Reservas — `/api/reservations`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/` | Autenticado | Crear (valida bloqueo y conflictos) |
| GET | `/{id}` | Autenticado | Detalle |
| GET | `/my` | Autenticado | Mis reservas |
| GET | `/pending` | Admin, Staff | Pendientes de aprobación |
| GET | `/no-show-candidates` | Admin, Staff | Aprobadas sin no-show (rango fechas) |
| POST | `/{id}/approve` | Admin, Staff | Aprobar |
| POST | `/{id}/reject` | Admin, Staff | Rechazar (`reason` opcional) |
| POST | `/{id}/cancel` | Autenticado | Cancelar (propietario) |
| POST | `/{id}/no-show` | Admin, Staff | Marcar no-show |

### Auditoría — `/api/audit`

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/` | Admin | Logs (`entityType`, `entityId`, `limit`) |

---

## Estructura del proyecto

```
SalaFinders/
├── Controllers/       # Auth, Spaces, Reservations, Audit
├── Data/              # ApplicationDbContext, seed
├── Interfaces/        # Contratos de servicios
├── Models/            # Entidades y DTOs
├── Services/          # Lógica de negocio
├── Migrations/        # EF Core
├── Program.cs         # JWT, CORS, pipeline
└── appsettings.json
```

---

## Desarrollo local con el frontend

1. Backend: `dotnet run` → `http://localhost:5155`
2. Frontend: `VITE_API_URL=/api`, `VITE_BACKEND_URL=http://localhost:5155`
3. Vite proxy reenvía `/api` al backend.

---

## Autores

Proyecto SalaFinder — API por el equipo backend (repositorio zhinfenix).
