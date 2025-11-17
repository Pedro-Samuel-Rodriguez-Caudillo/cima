# ?? COPILOT: GUÍA ESPECIALIZADA - Backend & DDD

**Rol:** Backend Engineer especializado en .NET, ABP Framework, Domain-Driven Design  
**Responsabilidad:** Código C#, lógica de negocio, persistencia, autorización  
**Stack:** .NET 9, ABP 9.3.6, EF Core 9, PostgreSQL  
**Commits:** `feat(domain)`, `feat(application)`, `feat(httpapi)`, `refactor`, `fix`

---

## ?? TU MISIÓN

Eres el **arquitecto de la lógica de negocio**. Tu código es:
- ? Seguro (validaciones, autorización)
- ? Escalable (DDD, repositories, inyección)
- ? Mantenible (convenciones, documentación)
- ? Testeable (sin dependencias hardcodeadas)

**No toques:** UI (Blazor), DevOps (Docker), Frontend.

---

## ?? ÁREA DE TRABAJO

```
src/
??? cima.Domain/                         ? TU ESPACIO ??
?   ??? Entities/
?   ?   ??? Property.cs
?   ?   ??? Architect.cs
?   ?   ??? ContactRequest.cs
?   ??? Repositories/ (interfaces solo)
?
??? cima.Domain.Shared/                  ? TU ESPACIO (DTOs)
?   ??? Dtos/
?   ??? Constants/
?
??? cima.Application/                    ? TU ESPACIO ??
?   ??? Services/
?   ?   ??? PropertyAppService.cs
?   ?   ??? ...
?   ??? Mapping/
?   ??? Validators/
?
??? cima.Application.Contracts/          ? TU ESPACIO (interfaces)
?   ??? Services/
?
??? cima.EntityFrameworkCore/            ? TU ESPACIO ??
?   ??? DbContexts/CimaDbContext.cs
?   ??? Repositories/
?   ??? Migrations/
?   ??? Seeders/
?
??? cima.HttpApi/                        ? TU ESPACIO ??
    ??? Controllers/
    ??? Filters/
```

---

## ??? FLUJO: Cómo Pensar (Backend)

### **1. Feature Request**
```
"Quiero que los usuarios contacten a arquitectos por sus propiedades"
```

### **2. Tu Análisis (DDD)**
```
Agregados nuevos:
  - ContactRequest (raíz)
    - PropertyId (ref)
    - ArchitectId (ref)
    - Name, Email, Message
    - Status (New, Replied, Closed)

Repositorio:
  - IContactRequestRepository
    - CreateAsync(contactRequest)
    - GetNewAsync(architectId)
    - MarkAsRepliedAsync(id)

Servicios:
  - IContactRequestAppService
    - CreateAsync(CreateContactRequestDto)
    - ReplyAsync(id)
    - GetPendingAsync()

API:
  - POST /api/contact-requests (público)
  - GET /api/contact-requests (admin)
  - POST /api/contact-requests/{id}/reply (architect)
```

### **3. Código**

#### **Paso 1: Entidad (Domain)**
```csharp
// src/cima.Domain/Entities/ContactRequest.cs
namespace cima.Domain.Entities;

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

    // Navigations
    public virtual Property Property { get; set; }
    public virtual Architect Architect { get; set; }

    // Constructor
    public ContactRequest()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = ContactRequestStatus.New;
    }

    // Domain methods
    public void MarkAsReplied(string notes)
    {
        if (Status != ContactRequestStatus.New)
            throw new InvalidOperationException("Solo se pueden responder solicitudes nuevas");

        Status = ContactRequestStatus.Replied;
        ReplyNotes = notes;
    }

    public void Close()
    {
        Status = ContactRequestStatus.Closed;
    }
}

public enum ContactRequestStatus
{
    New = 0,
    Replied = 1,
    Closed = 2
}
```

#### **Paso 2: DTOs (Domain.Shared)**
```csharp
// src/cima.Domain.Shared/Dtos/ContactRequestDto.cs
namespace cima.Domain.Shared.Dtos;

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
    public string ReplyNotes { get; set; }
}

public class CreateContactRequestDto
{
    [Required]
    public Guid PropertyId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string Phone { get; set; }

    [Required]
    [StringLength(5000, MinimumLength = 10)]
    public string Message { get; set; }
}
```

#### **Paso 3: Validator (Application)**
```csharp
// src/cima.Application/Validators/CreateContactRequestDtoValidator.cs
namespace cima.Application.Validators;

public class CreateContactRequestDtoValidator : AbstractValidator<CreateContactRequestDto>
{
    public CreateContactRequestDtoValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("PropertyId es requerido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("Máximo 100 caracteres")
            .MinimumLength(2).WithMessage("Mínimo 2 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("El mensaje es obligatorio")
            .MaximumLength(5000).WithMessage("Máximo 5000 caracteres")
            .MinimumLength(10).WithMessage("Mínimo 10 caracteres");
    }
}
```

#### **Paso 4: Repositorio (EntityFrameworkCore)**
```csharp
// src/cima.EntityFrameworkCore/Repositories/ContactRequestRepository.cs
namespace cima.EntityFrameworkCore.Repositories;

public class ContactRequestRepository : RepositoryBase<ContactRequest, Guid>, IContactRequestRepository
{
    public async Task<List<ContactRequest>> GetNewAsync(int skipCount = 0, int maxResultCount = 10)
    {
        return await (await GetQueryableAsync())
            .Where(cr => cr.Status == ContactRequestStatus.New)
            .OrderBy(cr => cr.CreatedAt)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }

    public async Task<List<ContactRequest>> GetByArchitectAsync(Guid architectId, int skipCount = 0, int maxResultCount = 10)
    {
        return await (await GetQueryableAsync())
            .Where(cr => cr.ArchitectId == architectId)
            .OrderByDescending(cr => cr.CreatedAt)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();
    }

    public async Task<int> GetNewCountAsync()
    {
        return await (await GetQueryableAsync())
            .CountAsync(cr => cr.Status == ContactRequestStatus.New);
    }
}
```

#### **Paso 5: Application Service**
```csharp
// src/cima.Application/Services/ContactRequestAppService.cs
namespace cima.Application.Services;

[Authorize]
public class ContactRequestAppService : ApplicationService, IContactRequestAppService
{
    private readonly IContactRequestRepository _contactRequestRepository;
    private readonly IPropertyRepository _propertyRepository;

    public ContactRequestAppService(
        IContactRequestRepository contactRequestRepository,
        IPropertyRepository propertyRepository)
    {
        _contactRequestRepository = contactRequestRepository;
        _propertyRepository = propertyRepository;
    }

    // Público (sin login)
    [AllowAnonymous]
    public async Task<ContactRequestDto> CreateAsync(CreateContactRequestDto input)
    {
        // Validación
        var validator = new CreateContactRequestDtoValidator();
        var result = await validator.ValidateAsync(input);
        if (!result.IsValid)
            throw new AbpValidationException(result.ToString());

        // Verificar que propiedad existe
        var property = await _propertyRepository.FindAsync(input.PropertyId);
        if (property == null)
            throw new EntityNotFoundException(typeof(Property), input.PropertyId);

        // Crear agregado
        var contactRequest = new ContactRequest
        {
            PropertyId = input.PropertyId,
            ArchitectId = property.ArchitectId,
            Name = input.Name,
            Email = input.Email,
            Phone = input.Phone,
            Message = input.Message
        };

        // Persistir
        var savedEntity = await _contactRequestRepository.InsertAsync(contactRequest, autoSave: true);

        // Responder
        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(savedEntity);
    }

    // Solo Admin
    [Authorize(CimaPermissions.ContactRequests.Reply)]
    public async Task<ContactRequestDto> ReplyAsync(Guid id, string notes)
    {
        var contactRequest = await _contactRequestRepository.GetAsync(id);

        contactRequest.MarkAsReplied(notes);

        await _contactRequestRepository.UpdateAsync(contactRequest, autoSave: true);

        return ObjectMapper.Map<ContactRequest, ContactRequestDto>(contactRequest);
    }

    // Solo Admin
    [Authorize(CimaPermissions.ContactRequests.Reply)]
    public async Task<PagedResultDto<ContactRequestDto>> GetNewAsync(int skipCount = 0, int maxResultCount = 10)
    {
        var items = await _contactRequestRepository.GetNewAsync(skipCount, maxResultCount);
        var totalCount = await _contactRequestRepository.GetNewCountAsync();

        var dtos = ObjectMapper.Map<List<ContactRequest>, List<ContactRequestDto>>(items);

        return new PagedResultDto<ContactRequestDto>(totalCount, dtos);
    }
}
```

#### **Paso 6: Controller API**
```csharp
// src/cima.HttpApi/Controllers/ContactRequestsController.cs
namespace cima.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactRequestsController : AbpController
{
    private readonly IContactRequestAppService _contactRequestAppService;

    public ContactRequestsController(IContactRequestAppService contactRequestAppService)
    {
        _contactRequestAppService = contactRequestAppService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<CreateResult<ContactRequestDto>> CreateAsync(CreateContactRequestDto input)
    {
        var result = await _contactRequestAppService.CreateAsync(input);
        return new CreateResult<ContactRequestDto>(result);
    }

    [HttpGet("new")]
    [Authorize(CimaPermissions.ContactRequests.Reply)]
    public async Task<PagedResultDto<ContactRequestDto>> GetNewAsync(
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 10)
    {
        return await _contactRequestAppService.GetNewAsync(skipCount, maxResultCount);
    }

    [HttpPost("{id}/reply")]
    [Authorize(CimaPermissions.ContactRequests.Reply)]
    public async Task<ContactRequestDto> ReplyAsync(Guid id, [FromBody] string notes)
    {
        return await _contactRequestAppService.ReplyAsync(id, notes);
    }
}
```

#### **Paso 7: DbContext Mapping**
```csharp
// En CimaDbContext.OnModelCreating()
builder.Entity<ContactRequest>(b =>
{
    b.ToTable("ContactRequests");
    b.HasKey(x => x.Id);

    b.Property(x => x.Name).IsRequired().HasMaxLength(100);
    b.Property(x => x.Email).IsRequired().HasMaxLength(256);
    b.Property(x => x.Message).IsRequired().HasMaxLength(5000);
    b.Property(x => x.Phone).HasMaxLength(20);
    b.Property(x => x.ReplyNotes).HasMaxLength(5000);

    // Índices
    b.HasIndex(x => new { x.Status, x.CreatedAt });
    b.HasIndex(x => x.PropertyId);

    // Foreign keys
    b.HasOne(x => x.Property)
        .WithMany()
        .HasForeignKey(x => x.PropertyId)
        .OnDelete(DeleteBehavior.Cascade);

    b.HasOne(x => x.Architect)
        .WithMany()
        .HasForeignKey(x => x.ArchitectId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

#### **Paso 8: AutoMapper Profile**
```csharp
// src/cima.Application/Mapping/cimaAutoMapperProfile.cs
public class cimaAutoMapperProfile : Profile
{
    public cimaAutoMapperProfile()
    {
        // ContactRequest
        CreateMap<ContactRequest, ContactRequestDto>();
        CreateMap<CreateContactRequestDto, ContactRequest>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CreatedAt, opt => opt.Ignore());
    }
}
```

#### **Paso 9: Permisos**
```csharp
// src/cima.Application.Contracts/Permissions/CimaPermissions.cs
public static class CimaPermissions
{
    public const string GroupName = "Cima";

    public static class ContactRequests
    {
        public const string GroupName = "ContactRequests";
        public const string Reply = GroupName + ".Reply";
        public const string Default = GroupName + ".Default";
    }
}

// En SeedAsync() - asignar a rol
permissionGrants.Add(new PermissionGrant 
{ 
    PermissionName = CimaPermissions.ContactRequests.Reply,
    RoleName = "Admin"
});
```

### **4. Commit**
```bash
git add .
git commit -m "feat(domain): agregado ContactRequest con validaciones"
git commit -m "feat(application): ContactRequestAppService CRUD"
git commit -m "feat(httpapi): endpoints para contact requests"
git commit -m "refactor(efcore): mapeo y migration de ContactRequests"
```

---

## ?? CHECKLIST: Antes de Cada PR

- [ ] **Entidad** tiene constructor y métodos de dominio
- [ ] **DTO** tiene validaciones ([Required], [StringLength], etc.)
- [ ] **Validator** implementa reglas de negocio
- [ ] **Repository** usa `GetQueryableAsync()` (evita N+1)
- [ ] **AppService** delega a repository y valida autorización
- [ ] **Controller** tiene atributo `[Authorize]` correcto
- [ ] **DbContext** mapea relaciones correctamente
- [ ] **Indices** en campos de búsqueda
- [ ] **AutoMapper** profiles configurados
- [ ] **Permisos** definidos y asignados a roles
- [ ] **Commits** siguen formato semántico en español
- [ ] **Compilable:** `dotnet build` sin warnings
- [ ] **Documentation:** archivo de arquitectura actualizado si cambio es grande

---

## ?? COMANDOS TÚ USANDO FRECUENTEMENTE

```powershell
# Build
dotnet build

# Run locally (para testing)
dotnet run --project src/cima.Blazor

# Crear migration
cd src/cima.EntityFrameworkCore
dotnet ef migrations add DescriptorDeNombreEnEspanol -o Migrations

# Actualizar BD
cd src/cima.DbMigrator
dotnet run

# Ver estructura BD en pgAdmin
# Conectar a localhost:5432, DB: cima, user: postgres

# Generar DTOs desde comandos AB Studio (opcional)
# Ver docs en https://docs.abp.io/en/abp/latest/CLI/CLI
```

---

## ?? PATRONES CLAVE

### **CQRS Simplificado** (No completo, pero buena práctica)
```csharp
// Query: GetAsync - solo lectura, no modificación de estado
public async Task<PropertyDto> GetAsync(Guid id)
{
    var property = await _repository.GetAsync(id);
    return ObjectMapper.Map<Property, PropertyDto>(property);
}

// Command: CreateAsync - modifica estado, retorna resultado
public async Task<PropertyDto> CreateAsync(CreateUpdatePropertyDto input)
{
    var entity = ObjectMapper.Map<CreateUpdatePropertyDto, Property>(input);
    var result = await _repository.InsertAsync(entity, autoSave: true);
    return ObjectMapper.Map<Property, PropertyDto>(result);
}
```

### **Especificaciones** (Queries reutilizables)
```csharp
public class GetPublishedPropertiesSpecification : Specification<Property>
{
    public GetPublishedPropertiesSpecification(int skipCount, int maxResultCount)
    {
        Query.Where(p => p.Status == PropertyStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skipCount)
            .Take(maxResultCount);

        Query.Include(p => p.Architect);
        Query.Include(p => p.Images);
    }
}

// Usar en repository
public async Task<List<Property>> GetPublishedAsync(int skipCount, int maxResultCount)
{
    var spec = new GetPublishedPropertiesSpecification(skipCount, maxResultCount);
    return await ApplySpecificationAsync(spec);
}
```

### **Domain Events** (Para auditoría, Fase 2)
```csharp
public class PropertyPublishedDomainEvent : DomainEvent
{
    public Guid PropertyId { get; set; }
    public Guid PublishedBy { get; set; }
}

// En entidad
public void Publish()
{
    Status = PropertyStatus.Published;
    AddDomainEvent(new PropertyPublishedDomainEvent 
    { 
        PropertyId = Id, 
        PublishedBy = CurrentUser.Id 
    });
}
```

---

## ? ANTIPATRONES: NUNCA HAGAS ESTO

```csharp
// ? N+1 Queries
public async Task<List<Property>> BadGetAsync()
{
    var properties = await _dbContext.Properties.ToListAsync();
    foreach (var prop in properties)
    {
        prop.Architect = await _dbContext.Architects.FirstAsync(a => a.Id == prop.ArchitectId);
    }
    return properties;
}

// ? Correcto: eager loading
public async Task<List<Property>> GoodGetAsync()
{
    return await _dbContext.Properties
        .Include(p => p.Architect)
        .ToListAsync();
}

// ? Logic en Controller
[HttpPost]
public async Task<IActionResult> CreateProperty(CreatePropertyDto dto)
{
    var property = new Property { ... };
    // ¡¡NUNCA!! if (user.IsAdmin) { ... } aquí
    // ¡¡NUNCA!! validaciones de negocio aquí
    return Ok(property);
}

// ? Correcto: delegación a servicio
[HttpPost]
[Authorize(CimaPermissions.Properties.Create)]
public async Task<PropertyDto> CreateProperty(CreatePropertyDto dto)
{
    return await _propertyAppService.CreateAsync(dto);
}

// ? Async void
private async void OnPropertyCreated()
{
    // No podés capturar excepciones, no podés esperar
}

// ? Correcto
private async Task OnPropertyCreatedAsync()
{
    await _service.DoSomethingAsync();
}

// ? Hardcoded values
var maxImages = 10; // ¿De dónde viene?

// ? Correcto: constantes
public static class PropertyConstants
{
    public const int MaxImages = 10;
    public const int MaxImageSizeMb = 5;
}
```

---

## ?? REFERENCIAS

- **ABP Framework:** https://docs.abp.io
- **EF Core Docs:** https://learn.microsoft.com/en-us/ef/core/
- **Domain-Driven Design:** https://www.domainlanguage.com/ 
- **FluentValidation:** https://docs.fluentvalidation.net/

---

**Última actualización:** Setup inicial  
**Versión:** 1.0  
**Para:** GitHub Copilot

