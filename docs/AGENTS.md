# ?? GUÍA PARA AGENTES IA (Codex, Gemini, Copilot)

## ?? Propósito

Este documento establece **convenciones, patrones y protocolos** para que los agentes IA (GitHub Copilot, Google Gemini, OpenAI Codex) trabajen de manera **coordinada y eficiente** en el proyecto CIMA.

**Objetivo:** Minimizar conflictos, maximizar productividad, y mantener consistencia en el código generado.

---

## ??? ESTRUCTURA DEL PROYECTO

```
cima/
??? src/
?   ??? cima.Domain                    ? Entidades, agregados (DDD)
?   ??? cima.Domain.Shared             ? DTOs, constantes, enums
?   ??? cima.Application               ? Servicios de aplicación
?   ??? cima.Application.Contracts     ? Interfaces de servicios
?   ??? cima.EntityFrameworkCore       ? ORM, migraciones, seeders
?   ??? cima.HttpApi                   ? Controllers REST
?   ??? cima.HttpApi.Client            ? Cliente HTTP tipado
?   ??? cima.Blazor                    ? Web App Server (SSR)
?   ??? cima.Blazor.Client             ? WASM interactivo
?   ??? cima.DbMigrator                ? Utility: migraciones
??? docs/
?   ??? README.md                      ? Índice maestro
?   ??? PLAN_2_SEMANAS.md              ? Plan 10 días
?   ??? DIA_1_GUIA_EJECUTIVA.md        ? Setup lunes
?   ??? ARQUITECTURA_TECNICA.md        ? Referencia técnica
?   ??? AGENTS.md                      ? Este archivo
??? tests/                              ? Tests (Fase 2+)
```

---

## ?? PROTOCOLOS DE COLABORACIÓN

### **1. Dominios de Responsabilidad**

| Agente | Especialidad | Scope | No tocar |
|--------|-------------|-------|---------|
| **Copilot** | Backend, DDD, EF Core, API REST | C#, Domain, Services, Controllers | Frontend, DevOps, Tests avanzados |
| **Gemini** | Frontend, Blazor, UX, CSS, accessibility | Razor, HTML, CSS, Blazor components | Build pipeline, infrastructure |
| **Codex** | DevOps, Docker, CI/CD, scripts | PowerShell, YAML, bash, Dockerfile | Core business logic, tests |

### **2. Convención de Commits**

**Formato Semántico en Español:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: Nueva funcionalidad
- `fix`: Bug fix
- `refactor`: Cambio sin nueva funcionalidad
- `docs`: Cambios en documentación
- `test`: Tests (cuando esté disponible)
- `chore`: Cambios en build, deps, etc.
- `style`: Formato, convenciones (sin lógica)
- `perf`: Optimización de rendimiento

**Scopes (opcionales, pero recomendados):**
```
feat(domain):         Cambios en agregados/entidades
feat(application):    Application Services
feat(httpapi):        Controllers REST
feat(blazor):         Componentes, páginas SSR
feat(blazor-client):  WASM, interactividad
feat(efcore):         ORM, migrations, seeders
feat(database):       Esquema BD, índices
feat(devops):         Docker, CI/CD, deployment
feat(docs):           Documentación
```

**Ejemplos:**
```
feat(domain): agregado ContactRequest con validaciones
fix(httpapi): autorización en endpoint ContactRequests
refactor(application): PropertyAppService con IQueryable
docs(architecture): actualizar diagrama EER
chore(devops): actualizar Docker image base
feat(blazor): componente PropertyCard responsive
```

### **3. Normas de Pull Request**

```
Título: `feat(scope): descripción concisa`

Descripción:
- ¿Qué cambió?
- ¿Por qué?
- ¿Cómo se testea? (manual o auto)
- Referencias: #123, closes #456

Checklist:
- [ ] Código sigue convenciones del proyecto
- [ ] Compilable sin warnings
- [ ] Tests pasan (si aplica)
- [ ] Documentación actualizada
```

---

## ?? CONVENCIONES DE CÓDIGO

### **C# (.NET 9, ABP 9.3.6)**

#### **Namespaces**
```csharp
// Domain
namespace cima.Domain.Entities { }
namespace cima.Domain.Repositories { }

// Application
namespace cima.Application.Services { }
namespace cima.Application.Mapping { }
namespace cima.Application.Validators { }

// Infrastructure
namespace cima.EntityFrameworkCore.Repositories { }
namespace cima.EntityFrameworkCore.DbContexts { }

// HTTP API
namespace cima.HttpApi.Controllers { }
namespace cima.HttpApi.Filters { }

// Blazor
namespace cima.Blazor.Pages { }
namespace cima.Blazor.Shared { }
```

#### **Clases**
```csharp
// Services (Application)
public class PropertyAppService : ApplicationService, IPropertyAppService
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyAppService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyDto> CreateAsync(CreateUpdatePropertyDto input)
    {
        // Validación
        ValidateInput(input);
        
        // Mapeo
        var property = ObjectMapper.Map<CreateUpdatePropertyDto, Property>(input);
        
        // Lógica de dominio
        property.SetStatus(PropertyStatus.Draft);
        
        // Persistencia
        var result = await _propertyRepository.InsertAsync(property, autoSave: true);
        
        // Respuesta
        return ObjectMapper.Map<Property, PropertyDto>(result);
    }
}

// Repositories (Infrastructure)
public class PropertyRepository : RepositoryBase<Property, Guid>, IPropertyRepository
{
    public async Task<List<Property>> GetPublishedAsync(int skipCount = 0, int maxResultCount = 10)
    {
        return await (await GetQueryableAsync())
            .Where(p => p.Status == PropertyStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }
}

// Controllers (HTTP API)
[ApiController]
[Route("api/[controller]")]
public class PropertiesController : AbpController
{
    private readonly IPropertyAppService _propertyAppService;

    public PropertiesController(IPropertyAppService propertyAppService)
    {
        _propertyAppService = propertyAppService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<PagedResultDto<PropertyDto>> GetPublishedAsync(
        [FromQuery] string? filter = null,
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 10)
    {
        // Delegación a Application Service
        return await _propertyAppService.GetPublishedAsync(filter, skipCount, maxResultCount);
    }
}
```

#### **DTOs**
```csharp
// Lectura
public class PropertyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    // ... other properties
}

// Creación
public class CreatePropertyDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}

// Actualización
public class UpdatePropertyDto : CreatePropertyDto
{
    public Guid Id { get; set; }
}

// O combinada
public class CreateUpdatePropertyDto
{
    // ... properties
}
```

#### **Async/Await**
```csharp
// ? Correcto
public async Task<PropertyDto> GetAsync(Guid id)
{
    var property = await _repository.GetAsync(id);
    return ObjectMapper.Map<Property, PropertyDto>(property);
}

// ? Evitar
public PropertyDto Get(Guid id)
{
    var property = _repository.GetAsync(id).Result; // NUNCA
    return ObjectMapper.Map<Property, PropertyDto>(property);
}
```

#### **Validación**
```csharp
// FluentValidation
public class CreateUpdatePropertyDtoValidator : AbstractValidator<CreateUpdatePropertyDto>
{
    public CreateUpdatePropertyDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio")
            .MaximumLength(200).WithMessage("Máximo 200 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0");
    }
}
```

#### **Autorización**
```csharp
// Atributos
[Authorize]                                    // Requiere cualquier usuario
[Authorize(Roles = "Admin")]                   // Requiere rol Admin
[Authorize(CimaPermissions.Properties.Create)] // Requiere permiso específico
[AllowAnonymous]                               // Público

// En servicios
if (!await AuthorizationService.IsGrantedAsync(CimaPermissions.Properties.Delete))
{
    throw new AbpAuthorizationException("No tienes permiso para eliminar");
}
```

### **Blazor (.razor, .razor.cs)**

#### **Estructura de Componentes**
```razor
@* Directivas y usos *@
@page "/properties"
@using cima.Blazor.Pages.Properties
@using cima.Domain.Shared.Dtos
@inject IPropertyAppService PropertyAppService
@inject MessageService MessageService

<PageTitle>Propiedades</PageTitle>

@* Layout *@
<div class="container mx-auto px-4 py-8">
    <h1 class="text-3xl font-bold mb-6">Catálogo de Propiedades</h1>

    @* Contenido *@
    @if (Properties == null)
    {
        <p>Cargando...</p>
    }
    else if (Properties.Count == 0)
    {
        <p>No hay propiedades disponibles</p>
    }
    else
    {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            @foreach (var property in Properties)
            {
                <PropertyCard Property="property" />
            }
        </div>
    }
</div>

@code {
    private List<PropertyDto>? Properties;
    private string? ErrorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await PropertyAppService.GetPublishedAsync();
            Properties = result.Items;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await MessageService.Error(ErrorMessage);
        }
    }
}
```

#### **Convenciones de Componentes**
```razor
@* Nombre: PropertyCard.razor *@
@namespace cima.Blazor.Shared

<div class="card shadow-lg hover:shadow-xl transition-shadow">
    @* Content *@
</div>

@code {
    [Parameter]
    public PropertyDto? Property { get; set; }

    [Parameter]
    public EventCallback OnDelete { get; set; }

    private async Task HandleDelete()
    {
        if (await ConfirmDelete())
        {
            await OnDelete.InvokeAsync();
        }
    }

    private async Task<bool> ConfirmDelete() => true; // Implementation
}
```

#### **Estilos Tailwind**
```razor
@* ? Correcto: usar clases Tailwind *@
<div class="flex flex-col md:flex-row gap-4 p-6 bg-gray-50 rounded-lg">
    <h2 class="text-2xl font-bold text-gray-900">Título</h2>
    <p class="text-gray-600 text-sm">Descripción</p>
</div>

@* ? Evitar: CSS inline *@
<div style="display: flex; gap: 10px; padding: 20px;">
    ...
</div>
```

### **SQL/Migrations**

```csharp
// Entity Framework Core Fluent API
builder.Entity<Property>(b =>
{
    b.ToTable("Properties");
    b.HasKey(x => x.Id);
    
    // Constraints
    b.Property(x => x.Title)
        .IsRequired()
        .HasMaxLength(200);
    
    // Índices (performance)
    b.HasIndex(x => new { x.Status, x.ArchitectId })
        .HasDatabaseName("IX_Properties_Status_ArchitectId");
    
    // Foreign Keys
    b.HasOne(x => x.Architect)
        .WithMany(a => a.Properties)
        .HasForeignKey(x => x.ArchitectId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // Value Objects
    b.OwnsMany(x => x.Images, ib =>
    {
        ib.ToTable("PropertyImages");
        ib.WithOwner().HasForeignKey("PropertyId");
    });
});
```

### **Docker & DevOps**

```dockerfile
# ? Correcto: multi-stage, optimizado
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/cima.Blazor/cima.Blazor.csproj", "src/cima.Blazor/"]
RUN dotnet restore "src/cima.Blazor/cima.Blazor.csproj"
COPY . .
RUN dotnet build "src/cima.Blazor/cima.Blazor.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "cima.Blazor.dll"]
```

---

## ?? ÁREAS DE ENFOQUE POR AGENTE

### **GitHub Copilot (Backend Specialist)**

**Fuerte en:**
- ? C# y .NET completamente
- ? EF Core ORM, migraciones, queries LINQ
- ? DDD, agregados, validaciones
- ? Application Services, DTOs
- ? Controllers REST, autorización
- ? Permisos RBAC, auditoría

**Tareas típicas:**
- Generar entidades y agregados
- Crear Application Services CRUD
- Implementar validaciones (FluentValidation)
- Generar Controllers HTTP
- Mapeos AutoMapper
- Queries de repositorio

**No debe hacer:**
- UI React/Vue/HTML avanzado
- DevOps/Docker/K8s
- Tests complejos

**Prompt template para Copilot:**
```
Crea un Application Service para [Entity] siguiendo:
- Hereda de ApplicationService (ABP)
- Implementa CRUD (Create, Read, Update, Delete)
- Valida input con [Validator]
- Autorización: [Permission]
- Mapeos: [Dto mapping]
- Manejo de excepciones

Stack: .NET 9, ABP 9.3.6, EF Core
```

---

### **Google Gemini (Frontend Specialist)**

**Fuerte en:**
- ? Blazor Razor components (.razor)
- ? HTML semantics, accessibility
- ? CSS Tailwind utilities
- ? Responsive design (mobile-first)
- ? UX patterns, form validation
- ? Blazorise components

**Tareas típicas:**
- Crear páginas Razor (catálogo, detalle)
- Diseñar componentes reutilizables
- Implementar formularios
- Maquetado responsive
- Galería de imágenes, carousels
- Accesibilidad (ARIA, labels)

**No debe hacer:**
- Lógica de negocio C#
- Configuración de infraestructura
- Migraciones BD

**Prompt template para Gemini:**
```
Crea una página Blazor para [Feature]:
- URL: [route]
- Componentes: [list]
- Estilos: Tailwind CSS
- Responsive: mobile, tablet, desktop
- Accesibilidad: labels, alt text
- Interactividad: [events needed]

Stack: Blazor Web App, .NET 9, Tailwind 3, Blazorise
```

---

### **OpenAI Codex (DevOps Specialist)**

**Fuerte en:**
- ? Docker & Docker Compose
- ? GitHub Actions workflows
- ? PowerShell, bash, shell scripts
- ? YAML configuration
- ? CI/CD pipelines
- ? Deployment scripts

**Tareas típicas:**
- Crear Dockerfile optimizado
- Escribir docker-compose.yml
- Generar GitHub Actions workflows
- Scripts de deployment
- Environment configuration
- Health checks, logging

**No debe hacer:**
- Código C# business logic
- Componentes UI complejos
- Validaciones de dominio

**Prompt template para Codex:**
```
Crea un [Docker/CI-CD/Script] para [cima]:
- Requisitos: [software versions]
- Servicios: app, postgres, nginx (opcional)
- Entorno: [dev/staging/prod]
- Triggers: [push, schedule, manual]
- Steps: [build, test, deploy]

Stack: Docker, GitHub Actions, PowerShell
```

---

## ?? ARCHIVOS CRÍTICOS (NO MODIFICAR sin coordinación)

| Archivo | Quién | Razón |
|---------|-------|-------|
| `cima.sln` | Copilot only | Estructura de proyectos |
| `common.props` | Copilot + Codex | Versiones, propiedades globales |
| `Program.cs` (Blazor) | Copilot | Configuración core |
| `cimaBlazorModule.cs` | Copilot | Registro de servicios |
| `.github/workflows/*` | Codex only | CI/CD pipeline |
| `Dockerfile` | Codex only | Build & deploy |
| `docker-compose.yml` | Codex only | Local development |
| `docs/*.md` | Todos (coordinado) | Documentación viva |

---

## ?? FLUJO DE TRABAJO COORDINADO

### **Escenario: Implementar Nueva Feature (Ej: Galería de Imágenes)**

```
SEMANA: Agregar gallery de imágenes a propiedades

DÍA 1 - COPILOT (Backend)
?? Criar PropertyImage entity si no existe ?
?? Crear PropertyImageAppService
?? Endpoints: POST/DELETE /api/properties/{id}/images
?? Commit: feat(httpapi): endpoints para galería de imágenes
?? PR para review

DÍA 2 - GEMINI (Frontend)
?? Esperar merge de Copilot PR
?? Crear PropertyImageGallery.razor (componente)
?? Crear Properties/Edit.razor (admin)
?? Integrar upload UI + preview
?? Commit: feat(blazor): galería de imágenes con upload
?? PR para review

DÍA 3 - CODEX (DevOps, opcional)
?? Si necesita storage externo (S3, Azure Blob)
?? Actualizar docker-compose.yml
?? Crear scripts de backup para imágenes
?? Commit: chore(devops): configurar almacenamiento imágenes
?? PR para review
```

### **Reglas de Coordinación**

1. **Dependencias declaradas:** Si Feature X depende de Feature Y
   - Copilot termina backend de Y
   - Gemini espera merge de Y antes de empezar frontend X
   - Comunica bloqueadores en PR

2. **Code review cruzado:**
   - Copilot revisa PR de backend
   - Gemini revisa PR de frontend
   - Codex revisa PR de DevOps
   - Todos revisan PRs de `docs/`

3. **Conflictos de merge:**
   - Si dos PRs tocan mismo archivo: comunicar primero
   - Resolver conflictos en coordinación (no merge forzado)

4. **Cambios en arquitectura:**
   - Requerirá actualizar `ARQUITECTURA_TECNICA.md`
   - Copilot propone cambio en PR
   - Se discute antes de merge

---

## ?? TESTING & QUALITY

### **Cuándo Hacer QA**

**Copilot (Backend):**
- [ ] Código compila sin warnings
- [ ] Queries LINQ sin N+1
- [ ] Validaciones server-side funciona
- [ ] Endpoints responden con status correcto

**Gemini (Frontend):**
- [ ] Componentes renderizan sin errores
- [ ] Responsive en móvil, tablet, desktop
- [ ] Formularios validan (client-side + server)
- [ ] Accesibilidad básica (DevTools accessibility audit)

**Codex (DevOps):**
- [ ] Docker builds sin errores
- [ ] docker-compose levanta servicios
- [ ] GitHub Actions workflow ejecuta exitosamente
- [ ] Environment variables correctas

---

## ?? CHECKLIST PRE-COMMIT

```
Antes de cada commit:

TODOS:
- [ ] Código compilable (`dotnet build`)
- [ ] Sigue convenciones del proyecto
- [ ] Commit message: `type(scope): descripción`
- [ ] PR description: qué, por qué, cómo se testea
- [ ] Sin secretos hardcodeados
- [ ] Actualizar documentación si aplica

COPILOT (Backend):
- [ ] Nuevas entidades: tiene migrations
- [ ] Services: tiene autorización check
- [ ] Controllers: tiene [Authorize] atributos
- [ ] DTOs: tiene validaciones

GEMINI (Frontend):
- [ ] Componentes: son reutilizables
- [ ] Estilos: usa Tailwind (no inline CSS)
- [ ] Formularios: validación client + server
- [ ] Imágenes: tienen alt text

CODEX (DevOps):
- [ ] Docker: multi-stage, optimizado
- [ ] CI/CD: todos los steps documentados
- [ ] Variables: en secrets, no en código
- [ ] Scripts: son idempotentes
```

---

## ?? ANTIPATRONES A EVITAR

| Antipatrón | Por qué es malo | Solución |
|-----------|-----------------|----------|
| `entity.Property.FirstAsync()` | N+1 queries | `Include(e => e.Property)` en repo |
| Lógica compleja en Controller | Difícil de testear | Mover a ApplicationService |
| Estilos inline en Razor | Difícil de mantener | Usar clases Tailwind |
| Valores hardcodeados | No escalable | Usar `IConfiguration` |
| Commit grande (100+ cambios) | Difícil de revertir | Commits pequeños, frecuentes |
| DTOs sin validación | Inseguro | Validar en ApplicationService |
| Queries sin índices | Lento en prod | Crear índices en migration |
| `async void` en handlers | Puede crashes | Siempre `Task` o `Task<T>` |

---

## ?? MÉTRICAS DE PRODUCTIVIDAD

**Tracking semanal:**
- PRs mergeados por agente
- Build failures / successes
- Code review time (avg)
- Blockers / dependencies

**EOD Viernes:**
- Features completadas vs plan
- Technical debt introduced
- Documentación actualizada

---

## ?? REFERENCIAS RÁPIDAS

### Links Importantes
- **ABP Docs:** https://docs.abp.io
- **Blazor Guide:** https://learn.microsoft.com/aspnet/core/blazor
- **EF Core:** https://learn.microsoft.com/ef/core
- **Tailwind:** https://tailwindcss.com/docs
- **GitHub Actions:** https://docs.github.com/en/actions

### Comandos Útiles
```powershell
# Build
dotnet build

# EF Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run locally
dotnet run --project src/cima.Blazor

# Docker
docker build -t cima:latest .
docker-compose up -d
```

---

## ?? ÚLTIMAS NOTAS

1. **Comunicación es clave:** Si estás bloqueado ? comunica NOW
2. **Documentación viva:** Actualiza `ARQUITECTURA_TECNICA.md` si cambias algo crítico
3. **Code quality > speed:** 100 líneas limpias > 500 hacky
4. **Revisa antes de merging:** Nadie es perfecto
5. **Tests en Fase 2:** Por ahora QA manual es OK

---

**Última actualización:** Setup inicial  
**Versión:** 1.0  
**Para:** Copilot, Gemini, Codex

