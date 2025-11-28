# ?? DÍA 1: GUÍA EJECUTIVA - LUNES

## ? TIMELINE DEL DÍA (8 horas)

```
08:00 - 09:00 ? Validar setup existente (Proyectos, compilación)
09:00 - 10:30 ? PostgreSQL local (Docker + conexión)
10:30 - 12:00 ? Definir modelo de dominio (4 agregados)
12:00 - 13:00 ? ? DESCANSO
13:00 - 14:30 ? Crear DTOs en Domain.Shared
14:30 - 16:00 ? Configurar DbContext (EF Core Fluent API)
16:00 - 17:00 ? Migration inicial + ejecutar en BD
```

---

## ?? OBJETIVO DEL DÍA

**Cuando termines lunes a las 17:00 tendrás:**
- ? Solución compilable sin errores
- ? PostgreSQL corriendo localmente
- ? 4 agregados de dominio implementados
- ? BD con esquema creado
- ? Base sólida para martes (seeders + repositories)

---

## ?? TAREAS ESPECÍFICAS

### **TAREA 1: Validar Setup (08:00 - 09:00)**

**En VS 2022:**

1. Abre `cima.sln` en `C:\Users\rodri\Documents\Inmobiliaria\cima\`

2. Verifica que existan estos proyectos:
   - ? `cima.Blazor`
   - ? `cima.Blazor.Client`
   - ? `cima.HttpApi`
   - ? `cima.HttpApi.Client`
   - ? `cima.EntityFrameworkCore`
   - ? `cima.Application`
   - ? `cima.Application.Contracts`
   - ? `cima.Domain`
   - ? `cima.Domain.Shared`
   - ? `cima.DbMigrator`

3. En Terminal (Ctrl + `):
```powershell
dotnet build
# Debe terminar: Build succeeded
```

4. Verifica .NET 9:
```powershell
dotnet --version
# Output: 9.0.x
```

5. Abre archivo: `src/cima.DbMigrator/appsettings.json`
   - Debe tener: `Host=localhost;Port=5432;Database=cima;Username=postgres`

? **Si todo pasó:** Avanza a tarea 2. Si falla ? revisa build errors.

---

### **TAREA 2: PostgreSQL Local (09:00 - 10:30)**

**En PowerShell (Admin):**

1. **Levanta contenedor:**
```powershell
# Si no existe aún
docker run --name cima-postgres `
  -e POSTGRES_DB=cima `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  -d postgres:16-alpine

# Si ya existe
docker start cima-postgres

# Verifica está corriendo
docker ps
# Debe listar: cima-postgres | postgres:16-alpine
```

2. **Crea secretos locales** (`src/cima.DbMigrator/appsettings.secrets.json`):
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cima;Username=postgres;Password=postgres"
  }
}
```

3. **Añade a `.gitignore`** (si no existe):
```
appsettings.secrets.json
appsettings.Production.secrets.json
```

4. **Verifica conexión** (descarga DBeaver o pgAdmin):
   - Host: `localhost`
   - Port: `5432`
   - User: `postgres`
   - Pass: `postgres`
   - Database: `cima`
   - Click "Test Connection" ? ? Success

? **Checklist completado?** Avanza a tarea 3.

---

### **TAREA 3: Modelo de Dominio (10:30 - 12:00)**

**En VS 2022, crea estos 4 archivos en `src/cima.Domain/Entities/`:**

#### Archivo 1: `Property.cs`
```csharp
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class Property : AggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; } // m²
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public PropertyStatus Status { get; set; }
        public Guid ArchitectId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public virtual Architect Architect { get; set; }
    }

    public enum PropertyStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }
}
```

#### Archivo 2: `Architect.cs`
```csharp
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class Architect : AggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }

        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
```

#### Archivo 3: `ContactRequest.cs`
```csharp
using System;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class ContactRequest : AggregateRoot<Guid>
    {
        public Guid PropertyId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string ReplyNotes { get; set; }

        public virtual Property Property { get; set; }
        public virtual Architect Architect { get; set; }
    }

    public enum ContactRequestStatus
    {
        New = 0,
        Replied = 1,
        Closed = 2
    }
}
```

#### Archivo 4: `PropertyImage.cs`
```csharp
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class PropertyImage : ValueObject
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ImageId;
            yield return Url;
            yield return DisplayOrder;
        }
    }
}
```

**Verifica:**
```powershell
dotnet build
# Debe compilar sin errores
```

? **Continúa a descanso de 1 hora**

---

### **TAREA 4: DTOs en Domain.Shared (13:00 - 14:30)**

**Crea estos 3 archivos en `src/cima.Domain.Shared/Dtos/`:**

#### Archivo 1: `PropertyDto.cs`
```csharp
using System;
using System.Collections.Generic;
using cima.Domain.Entities;

namespace cima.Domain.Shared.Dtos
{
    public class PropertyDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public PropertyStatus Status { get; set; }
        public Guid ArchitectId { get; set; }
        public ArchitectDto Architect { get; set; }
        public List<PropertyImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUpdatePropertyDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public Guid ArchitectId { get; set; }
    }

    public class PropertyImageDto
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
    }
}
```

#### Archivo 2: `ArchitectDto.cs`
```csharp
using System;

namespace cima.Domain.Shared.Dtos
{
    public class ArchitectDto
    {
        public Guid Id { get; set; }
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }
        public string UserName { get; set; }
    }

    public class UpdateArchitectDto
    {
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }
    }
}
```

#### Archivo 3: `ContactRequestDto.cs`
```csharp
using System;
using cima.Domain.Entities;

namespace cima.Domain.Shared.Dtos
{
    public class ContactRequestDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateContactRequestDto
    {
        public Guid PropertyId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }
}
```

```powershell
dotnet build
# ? Build succeeded
```

---

### **TAREA 5: DbContext & Migraciones (14:30 - 17:00)**

#### Paso 1: Crear `CimaDbContext.cs` (14:30 - 15:30)

Archivo: `src/cima.EntityFrameworkCore/CimaDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using cima.Domain.Entities;

namespace cima.EntityFrameworkCore
{
    [ConnectionStringName("Default")]
    public class CimaDbContext : AbpDbContext<CimaUser>
    {
        public DbSet<Property> Properties { get; set; }
        public DbSet<Architect> Architects { get; set; }
        public DbSet<ContactRequest> ContactRequests { get; set; }

        public CimaDbContext(DbContextOptions<CimaDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Properties
            builder.Entity<Property>(b =>
            {
                b.ToTable("Properties");
                b.HasKey(x => x.Id);
                b.Property(x => x.Title).IsRequired().HasMaxLength(200);
                b.Property(x => x.Description).HasMaxLength(5000);
                b.Property(x => x.Location).IsRequired().HasMaxLength(500);
                b.Property(x => x.Price).HasPrecision(18, 2);
                b.Property(x => x.Area).HasPrecision(10, 2);
                b.HasIndex(x => new { x.Status, x.ArchitectId });
                b.HasIndex(x => x.CreatedAt);
                b.HasOne(x => x.Architect)
                    .WithMany(a => a.Properties)
                    .HasForeignKey(x => x.ArchitectId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.OwnsMany(x => x.Images, ib =>
                {
                    ib.ToTable("PropertyImages");
                    ib.WithOwner().HasForeignKey("PropertyId");
                    ib.HasKey("PropertyId", "ImageId");
                });
            });

            // Architects
            builder.Entity<Architect>(b =>
            {
                b.ToTable("Architects");
                b.HasKey(x => x.Id);
                b.Property(x => x.Bio).HasMaxLength(2000);
                b.Property(x => x.PortfolioUrl).HasMaxLength(500);
                b.HasIndex(x => x.UserId).IsUnique();
            });

            // ContactRequests
            builder.Entity<ContactRequest>(b =>
            {
                b.ToTable("ContactRequests");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired().HasMaxLength(100);
                b.Property(x => x.Email).IsRequired().HasMaxLength(256);
                b.Property(x => x.Phone).HasMaxLength(20);
                b.Property(x => x.Message).IsRequired().HasMaxLength(5000);
                b.HasIndex(x => new { x.Status, x.CreatedAt });
                b.HasIndex(x => x.PropertyId);
                b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Architect).WithMany().HasForeignKey(x => x.ArchitectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
```

#### Paso 2: Generar Migration (15:30 - 16:30)

Terminal:
```powershell
cd src/cima.EntityFrameworkCore

dotnet ef migrations add InitialCreate -o Migrations

# Output:
# To undo this action, use 'ef migrations remove'
```

Abre archivo generado: `src/cima.EntityFrameworkCore/Migrations/[TIMESTAMP]_InitialCreate.cs`
- Verifica tenga: `CreateTable("Properties")`, `CreateTable("Architects")`, `CreateTable("ContactRequests")`, `CreateTable("PropertyImages")`

#### Paso 3: Ejecutar Migration (16:30 - 17:00)

Terminal:
```powershell
cd src/cima.DbMigrator

dotnet run
# Output: 
# Migrating database...
# Migration completed successfully!
```

Verifica en pgAdmin o DBeaver:
- Base de datos `cima` creada
- Tablas: Properties, Architects, ContactRequests, PropertyImages, AspNetUsers, AspNetRoles, etc.

```powershell
# Verifica esquema SQL
SELECT table_name FROM information_schema.tables WHERE table_schema='public';
```

Expected output:
```
 table_name
?????????????????????????
 Properties
 Architects
 ContactRequests
 PropertyImages
 AbpAuditLogs
 AbpClaimTypes
 AbpFeatureValues
 AbpLanguages
 AbpPermissionGrants
 AbpRoleClaims
 AbpRoles
 AbpSessionActivityLogs
 AbpSettings
 AbpTenants
 AbpUserClaims
 AbpUserLogins
 AbpUserRoles
 AbpUserTokens
 AbpUsers
 (más tablas de ABP Identity)
```

---

## ? CHECKLIST FIN DE DÍA

- [ ] Solución compila sin errores
- [ ] Docker: `docker ps` muestra `cima-postgres` corriendo
- [ ] PostgreSQL: Conecta exitosamente desde DBeaver/pgAdmin
- [ ] Código: 4 entidades (Property, Architect, ContactRequest, PropertyImage)
- [ ] DTOs: 3 archivos en Domain.Shared
- [ ] DbContext: Configurado con Fluent API
- [ ] BD: Migration ejecutada, tablas visibles en pgAdmin
- [ ] Git: Cambios listos para commit (sin secrets)

---

## ?? TROUBLESHOOTING COMÚN

| Problema | Solución |
|----------|----------|
| `Connection refused` (Postgres) | Verifica `docker ps` y que puerto sea 5432 |
| `Migration fails` | Verifica `appsettings.secrets.json` existe y BD `cima` existe |
| `Build error: missing using` | Añade `using cima.Domain.Entities;` al DbContext |
| `Foreign key violation` | Verifica `DeleteBehavior` en `HasOne().WithMany()` |
| `.NET SDK not found` | Instala .NET 9 desde https://dotnet.microsoft.com/download |

---

## ?? RECURSOS

- ?? [ABP Framework Docs](https://docs.abp.io)
- ?? [EF Core Fluent API](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)
- ?? [Docker PostgreSQL Setup](https://hub.docker.com/_/postgres)

---

**Lunes EOD = Listo para martes (Seeders + Repositories)**

