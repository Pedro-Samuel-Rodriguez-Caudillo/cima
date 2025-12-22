# ?? PLAN DE DESARROLLO: PLATAFORMA INMOBILIARIA CIMA (2 Semanas)

**Fecha inicio:** Próximo lunes  
**Duración:** 10 días laborales (2 semanas)  
**Stack:** .NET 9 + Blazor Web App + PostgreSQL + ABP 9.3.6  
**Equipo:** 1 desarrollador full-stack

---

## ?? ÍNDICE

1. [Visión del Proyecto](#visión-del-proyecto)
2. [Scope Funcional](#scope-funcional)
3. [Arquitectura Técnica](#arquitectura-técnica)
4. [Timeline Detallado (2 Semanas)](#timeline-detallado-2-semanas)
5. [Día 1: Setup & Infraestructura](#día-1-setup--infraestructura)
6. [Entregables Finales](#entregables-finales)
7. [Backlog & Fases Futuras](#backlog--fases-futuras)

---

## ?? VISIÓN DEL PROYECTO

**Objetivo:** Diseñar y entregar una plataforma inmobiliaria moderna, escalable y operativa en 2 semanas.

**Usuarios finales:**
- **Público:** Visitantes buscando propiedades
- **Arquitectos:** Proveedores que publican sus proyectos
- **Moderadores:** Gestión de contenido
- **Admin:** Administración completa del sistema

**KPIs de éxito:**
- ? MVP 100% funcional y deployable
- ? Zero deuda técnica crítica
- ? Documentación lista para handoff
- ? Base s�lida para escalar

---

## ?? SCOPE FUNCIONAL

### **Cat�logo Público (Sin login)**
- Listado de propiedades publicadas con paginación
- Filtros: t�tulo, dormitorios, ba�os, rango de precio, �rea
- P�gina de detalle con galería de im�genes
- Información de arquitecto responsable
- Formulario de contacto por propiedad

### **Portafolios de Arquitectos**
- P�gina: `/architects` ? Listado de arquitectos
- P�gina: `/architects/{id}` ? Perfil con bio y proyectos
- Grid de propiedades publicadas del arquitecto

### **Panel de Administración (Con login)**

#### Gestión de Propiedades
- CRUD completo (Crear, Leer, Actualizar, Eliminar)
- Estados: Borrador, Publicada, Archivada
- Carga de m�ltiples im�genes (max 10, max 5MB c/u)
- Edición de orden de im�genes

#### Gestión de Usuarios
- Listado de usuarios por rol
- Edición de datos de arquitecto (bio, portfolio URL)
- Asignación de propiedades a arquitectos

#### Gestión de Solicitudes
- Listado de solicitudes de contacto
- Filtro por estado (Nueva, Respondida, Cerrada)
- Marcar como respondida/cerrada
- Visualizar email y mensaje

#### Estad�sticas
- Dashboard KPI: Total propiedades, estados, arquitectos, solicitudes
- Gr�ficos: Propiedades por estado, solicitudes por mes (�ltimos 3 meses)
- Tabla de solicitudes recientes sin responder

### **Autenticación & Autorización**
- Login con identidad propia (OpenIddict)
- Roles: Admin, Moderator, Architect
- Permisos granulares por rol
- Auditoría de acciones críticas

### **Exclusiones (Fuera de MVP)**
- ? Multi-tenancy
- ? Pagos en l�nea
- ? CRM integrado
- ? Notificaciones push
- ? Integraciones externas complejas
- ? Analytics avanzado

---

## ??? ARQUITECTURA TÉCNICA

### **Stack Seleccionado**

| Capa | Tecnolog�a | Versión |
|------|-----------|---------|
| **Frontend (SSR)** | Blazor Web App (Interactive Server) | .NET 9 |
| **Frontend (WASM)** | Blazor WebAssembly (Interactive) | .NET 9 |
| **Backend** | ASP.NET Core + ABP Framework | 9.3.6 |
| **ORM** | Entity Framework Core | 9.0 |
| **Base Datos** | PostgreSQL | 16-alpine |
| **Autenticación** | OpenIddict + ABP Identity | 9.3.6 |
| **UI Components** | Blazorise | 1.8.1 |
| **Validación** | FluentValidation | 11.x |
| **Mapeo** | AutoMapper | 13.x |
| **Logging** | Serilog | 9.x |
| **DI/IoC** | Autofac | 7.x |
| **DevOps** | Docker + Docker Compose | Latest |
| **CI/CD** | GitHub Actions | Workflows |

### **Estructura de Proyectos**

```
src/
??? cima.Domain                          ? Entidades, agregados, l�gica de dominio
??? cima.Domain.Shared                   ? DTOs, constantes, enums compartidos
??? cima.Application                     ? Servicios de aplicación (CRUD, l�gica business)
??? cima.Application.Contracts           ? Interfaces de servicios (abstracciones)
??? cima.EntityFrameworkCore             ? DbContext, migraciones, seeders
??? cima.HttpApi                         ? Controllers REST (sin vistas)
??? cima.HttpApi.Client                  ? Cliente HTTP tipado (consumo de API)
??? cima.Blazor                          ? App Web principal (SSR + orquestación)
??? cima.Blazor.Client                   ? Componentes interactivos (WASM)
??? cima.DbMigrator                      ? Utilidad de migración de BD
??? cima.Web                             (si aplica) ? Configuración web

Database/
??? cima ? Base de datos PostgreSQL
```

### **Modelo de Dominio (DDD)**

```
AGREGADOS:

Listing (Ra�z de Agregado)
  ?? ListingId: Guid
  ?? Title: string
  ?? Description: string (largo)
  ?? Location: string
  ?? Price: decimal
  ?? Area: decimal (m�)
  ?? Bedrooms: int
  ?? Bathrooms: int
  ?? Status: ListingStatus (Draft, Published, Archived)
  ?? ArchitectId: Guid (FK)
  ?? CreatedAt: DateTime
  ?? CreatedBy: Guid
  ?? LastModifiedAt: DateTime
  ?? LastModifiedBy: Guid
  ?? ListingImages: List<ListingImage> (Objeto de Valor)

Architect (Ra�z de Agregado)
  ?? ArchitectId: Guid
  ?? UserId: Guid (FK ? Identity.User)
  ?? Bio: string
  ?? PortfolioUrl: string
  ?? Listings: ICollection<Listing> (navegación)

ContactRequest (Ra�z de Agregado)
  ?? ContactRequestId: Guid
  ?? ListingId: Guid (FK)
  ?? ArchitectId: Guid (FK)
  ?? Name: string
  ?? Email: string
  ?? Phone: string
  ?? Message: string
  ?? Status: ContactRequestStatus (New, Replied, Closed)
  ?? CreatedAt: DateTime
  ?? ReplyNotes: string (opcional)

ListingImage (Objeto de Valor)
  ?? ImageId: Guid
  ?? Url: string (path relativo)
  ?? DisplayOrder: int
  ?? AltText: string
  ?? FileSize: long (bytes)
  ?? ContentType: string (mime type)
```

### **Flujos Principales**

#### Flujo 1: Visitante ve cat�logo
```
Visitante
  ? [GET /Listings?filter]
API (ListingsController)
  ? [GetPublishedAsync]
ListingAppService
  ? [Query DB]
ListingRepository
  ? [EF Core query]
Database (SELECT * FROM Listings WHERE Status='Published')
  ? [ListingDto[]]
Blazor UI (Listings/Index.razor)
  ? [Renderizado SSR]
HTML + CSS
```

#### Flujo 2: Admin publica propiedad
```
Admin Login
  ? [POST /auth/login]
OpenIddict
  ? [JWT token + refresh token]
Admin (Listings/Create.razor)
  ? [POST /api/Listings + im�genes]
ListingsController
  ? [CreateAsync + authorization]
ListingAppService
  ? [Validar + mapear]
Listing (dominio)
  ? [SaveAsync]
EF Core DbContext
  ? [INSERT + ADD images]
PostgreSQL
  ? [Confirmación]
Respuesta
  ? [ListingDto + redirect]
Admin (Listings/Detail.razor)
```

#### Flujo 3: Visitante contacta arquitecto
```
Visitante (Listings/Detail.razor)
  ? [Click "Contactar"]
ContactForm Modal
  ? [POST /api/contact-requests + sin auth]
ContactRequestsController
  ? [CreateAsync]
ContactRequestAppService
  ? [Validar email format, message length]
ContactRequest (dominio)
  ? [SaveAsync]
PostgreSQL
  ? [INSERT]
Toast
  ? [? Gracias por tu mensaje]
Admin (ContactRequests/Index.razor)
  ? [Ve nueva solicitud]
```

---

## ?? TIMELINE DETALLADO (2 SEMANAS)

### **SEMANA 1: Infraestructura & Backend Core**

| Día | Focus | Duración | Resultado |
|-----|-------|----------|-----------|
| **Lunes** | Setup, modelo de dominio, BD | 8h | Proyectos listos, migraciones, seeders |
| **Martes** | EF Core, DbContext, migrations | 8h | BD con esquema, datos de prueba |
| **Mi�rcoles** | Application Services, validación | 8h | CRUD services, permisos, mapeos |
| **Jueves** | API REST, Swagger, CORS | 8h | Controllers documentados, testeable |
| **Viernes** | Auth, OpenIddict, seguridad | 8h | Usuarios, roles, permisos funcionales |

### **SEMANA 2: Frontend & Deployment**

| Día | Focus | Duración | Resultado |
|-----|-------|----------|-----------|
| **Lunes** | Cat�logo público UI | 8h | Listado, filtros, detalle, responsive |
| **Martes** | Admin CRUD, upload im�genes | 8h | Panel funcional, gestión Listings |
| **Mi�rcoles** | Portafolios, estad�sticas | 8h | Perfiles arquitectos, dashboard KPI |
| **Jueves** | Docker, CI/CD, deployment | 8h | Contenedores, GitHub Actions, VPS ready |
| **Viernes** | QA, bugs, documentación | 8h | MVP pulido, docs completas, handoff |

---

## ?? DÍA 1: SETUP & INFRAESTRUCTURA (LUNES)

### **Objetivo del Día**
Al finalizar viernes tendr�s:
- ? Solución compilada sin errores
- ? Base de datos PostgreSQL corriendo localmente
- ? Modelo de dominio implementado
- ? Migrations iniciales
- ? Data seeders bbásicos
- ? Repositorios de lectura-escritura

### **Tareas Detalladas (Con Tiempos)**

#### **08:00 - 09:00 | 1. Validar Setup Existente**

**Qué hacer:**
1. Abrir solución `cima.sln`
2. Verificar proyectos existentes:
   ```
   ? cima.Blazor (Servidor Web App)
   ? cima.Blazor.Client (WASM interactivo)
   ? cima.HttpApi (Controllers)
   ? cima.HttpApi.Client (Cliente HTTP)
   ? cima.EntityFrameworkCore (ORM)
   ? cima.Application (Services)
   ? cima.Application.Contracts (Interfaces)
   ? cima.Domain (Entidades)
   ? cima.Domain.Shared (DTOs)
   ? cima.DbMigrator (Tool)
   ```

3. Revisar archivo: `src/cima.DbMigrator/appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Port=5432;Database=cima;Username=postgres"
     },
     "OpenIddict": { ... }
   }
   ```

4. Revisar: `src/cima.Blazor/cimaBlazorModule.cs`
   - Confirmar: `IsBlazorWebApp = true`
   - Confirmar: `.AddInteractiveServerComponents()` + `.AddInteractiveWebAssemblyComponents()`

**Checklist:**
- [ ] Solución abre sin errores
- [ ] .NET 9 SDK instalado (`dotnet --version`)
- [ ] Proyectos compilables (`dotnet build`)

---

#### **09:00 - 10:30 | 2. Configurar PostgreSQL Local (Docker)**

**Qué hacer:**

1. **Levantar contenedor** (PowerShell/Terminal):
```powershell
# Si no existe
docker run --name cima-postgres `
  -e POSTGRES_DB=cima `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  -d postgres:16-alpine

# Si ya existe
docker start cima-postgres
```

2. **Verificar conexión** (usando pgAdmin o DBeaver):
   - Host: `localhost`
   - Port: `5432`
   - User: `postgres`
   - Password: `postgres`
   - Database: `cima`

3. **Crear archivo** `src/cima.DbMigrator/appsettings.secrets.json` (gitignore):
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cima;Username=postgres;Password=postgres"
  }
}
```

**Checklist:**
- [ ] `docker ps` muestra contenedor corriendo
- [ ] pgAdmin conecta exitosamente
- [ ] BD `cima` visible en pgAdmin

---

#### **10:30 - 12:00 | 3. Definir Modelo de Dominio**

**Qué hacer:**

1. **Crear archivo** `src/cima.Domain/Entities/Listing.cs`:
```csharp
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class Listing : AggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; } // m�
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; } // Draft, Published, Archived
        public Guid ArchitectId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }

        // Colección de im�genes
        public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();

        // Navegación
        public virtual Architect Architect { get; set; }
    }

    public enum ListingStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }
}
```

2. **Crear archivo** `src/cima.Domain/Entities/Architect.cs`:
```csharp
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class Architect : AggregateRoot<Guid>
    {
        public Guid UserId { get; set; } // FK a Identity.User
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }

        // Navegación
        public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}
```

3. **Crear archivo** `src/cima.Domain/Entities/ContactRequest.cs`:
```csharp
using System;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class ContactRequest : AggregateRoot<Guid>
    {
        public Guid ListingId { get; set; }
        public Guid ArchitectId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public ContactRequestStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string ReplyNotes { get; set; }

        // Navegación
        public virtual Listing Listing { get; set; }
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

4. **Crear archivo** `src/cima.Domain/Entities/ListingImage.cs`:
```csharp
using System;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities
{
    public class ListingImage : ValueObject
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; } // Ruta relativa: /images/Listings/...
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
        public long FileSize { get; set; } // bytes
        public string ContentType { get; set; } // image/jpeg, image/png, etc.

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ImageId;
            yield return Url;
            yield return DisplayOrder;
        }
    }
}
```

**Checklist:**
- [ ] Archivos creados en `src/cima.Domain/Entities/`
- [ ] Enums definidos (`ListingStatus`, `ContactRequestStatus`)
- [ ] Compilación sin errores (`dotnet build`)

---

#### **12:00 - 13:00 | DESCANSO**

---

#### **13:00 - 14:30 | 4. Crear DTOs en Domain.Shared**

**Qué hacer:**

1. **Crear archivo** `src/cima.Domain.Shared/Dtos/ListingDto.cs`:
```csharp
using System;
using System.Collections.Generic;
using cima.Domain.Entities;

namespace cima.Domain.Shared.Dtos
{
    public class ListingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public ListingStatus Status { get; set; }
        public Guid ArchitectId { get; set; }
        public ArchitectDto Architect { get; set; }
        public List<ListingImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUpdateListingDto
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

    public class ListingImageDto
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
    }
}
```

2. **Crear archivo** `src/cima.Domain.Shared/Dtos/ArchitectDto.cs`:
```csharp
using System;
using System.Collections.Generic;

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

3. **Crear archivo** `src/cima.Domain.Shared/Dtos/ContactRequestDto.cs`:
```csharp
using System;
using cima.Domain.Entities;

namespace cima.Domain.Shared.Dtos
{
    public class ContactRequestDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
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
        public Guid ListingId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }
}
```

**Checklist:**
- [ ] Todos los DTOs creados
- [ ] Compilación OK

---

#### **14:30 - 16:00 | 5. Configurar DbContext (EF Core)**

**Qué hacer:**

1. **Crear archivo** `src/cima.EntityFrameworkCore/CimaDbContext.cs`:
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
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Architect> Architects { get; set; }
        public DbSet<ContactRequest> ContactRequests { get; set; }

        public CimaDbContext(DbContextOptions<CimaDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tabla: Listings
            builder.Entity<Listing>(b =>
            {
                b.ToTable("Listings");
                b.HasKey(x => x.Id);

                b.Listing(x => x.Title).IsRequired().HasMaxLength(200);
                b.Listing(x => x.Description).HasMaxLength(5000);
                b.Listing(x => x.Location).IsRequired().HasMaxLength(500);
                b.Listing(x => x.Price).HasPrecision(18, 2);
                b.Listing(x => x.Area).HasPrecision(10, 2);
                b.Listing(x => x.Status).IsRequired();

                // índices
                b.HasIndex(x => new { x.Status, x.ArchitectId });
                b.HasIndex(x => x.CreatedAt);

                // Foreign key
                b.HasOne(x => x.Architect)
                    .WithMany(a => a.Listings)
                    .HasForeignKey(x => x.ArchitectId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Value Object: ListingImages
                b.OwnsMany(x => x.Images, ib =>
                {
                    ib.ToTable("ListingImages");
                    ib.WithOwner().HasForeignKey("ListingId");
                    ib.HasKey("ListingId", "ImageId");
                });
            });

            // Tabla: Architects
            builder.Entity<Architect>(b =>
            {
                b.ToTable("Architects");
                b.HasKey(x => x.Id);

                b.Listing(x => x.Bio).HasMaxLength(2000);
                b.Listing(x => x.PortfolioUrl).HasMaxLength(500);

                b.HasIndex(x => x.UserId).IsUnique();
            });

            // Tabla: ContactRequests
            builder.Entity<ContactRequest>(b =>
            {
                b.ToTable("ContactRequests");
                b.HasKey(x => x.Id);

                b.Listing(x => x.Name).IsRequired().HasMaxLength(100);
                b.Listing(x => x.Email).IsRequired().HasMaxLength(256);
                b.Listing(x => x.Phone).HasMaxLength(20);
                b.Listing(x => x.Message).IsRequired().HasMaxLength(5000);
                b.Listing(x => x.Status).IsRequired();

                b.HasIndex(x => new { x.Status, x.CreatedAt });
                b.HasIndex(x => x.ListingId);

                // Foreign keys
                b.HasOne(x => x.Listing).WithMany().HasForeignKey(x => x.ListingId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Architect).WithMany().HasForeignKey(x => x.ArchitectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
```

**Checklist:**
- [ ] DbContext compilable
- [ ] Mappeos EF Core correctos
- [ ] índices definidos

---

#### **16:00 - 17:00 | 6. Crear & Ejecutar Migration Inicial**

**Qué hacer:**

1. **Generar migration**:
```powershell
cd src/cima.EntityFrameworkCore
dotnet ef migrations add InitialCreate -o Migrations
```

2. **Revisar archivo generado**: `src/cima.EntityFrameworkCore/Migrations/[timestamp]_InitialCreate.cs`
   - Debe contener tablas: Listings, Architects, ContactRequests, ListingImages

3. **Ejecutar migration**:
```powershell
cd src/cima.DbMigrator
dotnet ef database update
```

4. **Verificar en pgAdmin**:
   - Base de datos `cima` con 5 tablas creadas
   - Esquema correcto

**Checklist:**
- [ ] Migration generada sin errores
- [ ] BD creada exitosamente
- [ ] Tablas visibles en pgAdmin

---

### **FIN DÍA 1**

**Resumen de lo logrado:**
- ? Proyectos configurados (.NET 9, ABP 9.3.6)
- ? PostgreSQL corriendo localmente
- ? Modelo de dominio implementado (4 agregados)
- ? DTOs base creados
- ? DbContext configurado con Fluent API
- ? Migration inicial ejecutada
- ? BD lista con esquema completo

**Próximos pasos (Martes):**
- Crear data seeders (usuarios, propiedades demo)
- Implementar repositorios
- Testing de queries b�sicas

---

## ?? ENTREGABLES FINALES (después de 2 semanas)

### **C�digo & Repositorio**
- ? Solución compilable, sin warnings críticos
- ? Todas las migraciones versionadas
- ? Data seeders para desarrollo
- ? `.gitignore` con secretos excluidos
- ? `common.props` actualizado

### **Funcionalidades Completadas**
- ? Cat�logo público (listado, filtros, detalle)
- ? Panel administrativo (CRUD Listings)
- ? Upload y gestión de im�genes
- ? Portafolios de arquitectos
- ? Formularios de contacto
- ? Gestión de usuarios y roles
- ? Dashboard de estad�sticas
- ? Autenticación y autorización completa

### **Infraestructura**
- ? Dockerfile optimizado (multi-stage)
- ? docker-compose.yml (desarrollo)
- ? docker-compose.prod.yml (producción)
- ? GitHub Actions CI/CD
- ? Scripts de deployment a VPS

### **Documentación**
- ? `ARCHITECTURE.md` ? Estructura técnica detallada
- ? `API.md` ? Documentación de todos los endpoints
- ? `DATABASE.md` ? Esquema, migraciones, backup
- ? `DEPLOYMENT.md` ? Gu�a paso a paso de deployment
- ? `ADMIN_GUIDE.md` ? Manual de operación para cliente
- ? `DEVELOPMENT.md` ? Setup local, debugging, testing

### **Calidad**
- ? Responsive en mobile, tablet, desktop
- ? Validaciones server-side y client-side
- ? Manejo robusto de errores
- ? Logging centralizado (Serilog)
- ? Accesibilidad b�sica (labels, alt text)
- ? Rendimiento optimizado (índices BD, lazy loading)

---

## ?? BACKLOG & FASES FUTURAS

### **Fase 2 (Semanas 3-4)**
- Integration tests (xUnit + Testcontainers)
- E2E tests (Playwright o Selenium)
- Redis cache para queries frecuentes
- Search avanzado (Elasticsearch, opcional)

### **Fase 3 (Semanas 5-6)**
- Galería de fotos avanzada (carousel 360�, lightbox)
- B�squeda por ubicación (Google Maps API)
- Notificaciones por email (SendGrid)
- Reportes PDF exportables

### **Fase 4 (Mes 2)**
- Pagos en l�nea (Stripe/PayPal)
- Sistema de favoritos
- Reviews y ratings
- Newsletter integrado

### **Fase 5+ (Roadmap Largo Plazo)**
- Multi-language support
- Mobile app nativa (MAUI)
- AI-powered image recognition
- CRM integrado para leads

---

## ?? NOTAS OPERACIONALES

| Aspecto | Decisión | Justificación |
|---------|----------|---------------|
| **Scope MVP** | Solo cat�logo + admin + auth | M�ximo impacto m�nimo en 2 semanas |
| **DB Sofware** | PostgreSQL (no SQLite) | Escalable, production-ready |
| **Auth** | OpenIddict (no terceros) | Control total, simple integración ABP |
| **UI Framework** | Blazorise sobre Blazor | Componentes premium, ahorra desarrollo |
| **DevOps** | Docker + GitHub Actions | Standard industry, f�cil deployment |
| **Testing** | Manual en MVP (auto en Fase 2) | Trade-off: velocidad vs cobertura |
| **Logging** | Serilog estructurado | Debugging eficiente en producción |

---

## ? RECURSOS NECESARIOS

- 1 Developer (full-stack)
- Laptop con: VS 2022 / VS Code + C# extension
- Docker Desktop (Postgres + app)
- GitHub repo (CI/CD)
- VPS staging/prod ($ m�nimo) o Railway/Heroku
- Dominio + SSL (Let's Encrypt)

---

**�ltima actualización:** [Fecha]  
**Próxima revisión:** Viernes EOD (después de Semana 1)  
**Contacto:** [Tu email]

