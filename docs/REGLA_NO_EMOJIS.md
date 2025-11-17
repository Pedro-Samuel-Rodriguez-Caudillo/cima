# NO EMOJIS: REGLA CRÍTICA PARA TODOS LOS AGENTS

## Implementado el: Sesión AGENTS + Multi-tenancy

---

## REGLA UNIVERSAL

**En TODO contexto de código, logs y comentarios:**

### PROHIBIDO:
- Usar emojis en comentarios C#
- Usar emojis en logs/output
- Usar emojis en strings dentro del código
- Usar emojis en nombres de variables, métodos, clases
- Usar emojis en JSON, YAML, appsettings
- Usar emojis en commits
- Usar emojis en console output

### PERMITIDO:
- Usar emojis SOLO en documentación Markdown (*.md)
- Usar emojis en README
- Usar emojis en guías y planes (estos documentos)

---

## EJEMPLOS APLICADOS AL PROYECTO CIMA

### CÓDIGO C# - CORRECTO (SIN emojis)

```csharp
namespace cima.Application.Services;

public class PropertyAppService : ApplicationService, IPropertyAppService
{
    private readonly IPropertyRepository _propertyRepository;
    
    public PropertyAppService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyDto> CreateAsync(CreateUpdatePropertyDto input)
    {
        // Validate input parameters
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        // Map DTO to entity
        var property = ObjectMapper.Map<CreateUpdatePropertyDto, Property>(input);
        
        // Set initial status as draft
        property.SetStatus(PropertyStatus.Draft);
        
        // Persist to database
        var result = await _propertyRepository.InsertAsync(property, autoSave: true);
        
        // Return mapped response
        return ObjectMapper.Map<Property, PropertyDto>(result);
    }

    public async Task<PagedResultDto<PropertyDto>> GetPublishedAsync(
        int skipCount = 0, 
        int maxResultCount = 10)
    {
        // Query published properties
        var items = await _propertyRepository.GetPublishedAsync(skipCount, maxResultCount);
        
        // Count total records for pagination
        var totalCount = await _propertyRepository.GetPublishedCountAsync();
        
        // Map to DTOs
        var dtos = ObjectMapper.Map<List<Property>, List<PropertyDto>>(items);
        
        // Return paged result
        return new PagedResultDto<PropertyDto>(totalCount, dtos);
    }
}
```

### CÓDIGO C# - INCORRECTO (CON emojis)

```csharp
// INCORRECTO - NO HACER ESTO
public async Task<PropertyDto> CreateAsync(CreateUpdatePropertyDto input)
{
    // Validate input parameters ??
    if (input == null)
        throw new ArgumentNullException(nameof(input));

    // Map DTO to entity ??
    var property = ObjectMapper.Map<CreateUpdatePropertyDto, Property>(input);
    
    // Set initial status as draft ?
    property.SetStatus(PropertyStatus.Draft);
    
    // Persist to database ??
    var result = await _propertyRepository.InsertAsync(property, autoSave: true);
    
    // Return mapped response ??
    return ObjectMapper.Map<Property, PropertyDto>(result);
}
```

---

### LOGS - CORRECTO (SIN emojis)

```csharp
// CORRECTO - Usar así
Logger.LogInformation("Property created successfully. PropertyId: {PropertyId}, Title: {Title}", 
    property.Id, property.Title);

Logger.LogWarning("Property price seems unusually high. PropertyId: {PropertyId}, Price: {Price}", 
    property.Id, property.Price);

Logger.LogError(ex, "Failed to create property. Error: {Message}", ex.Message);

Logger.LogDebug("Querying published properties. SkipCount: {SkipCount}, MaxResultCount: {MaxResultCount}", 
    skipCount, maxResultCount);
```

### LOGS - INCORRECTO (CON emojis)

```csharp
// INCORRECTO - NO HACER ESTO
Logger.LogInformation("Property created successfully ??. PropertyId: {PropertyId}", property.Id);

Logger.LogWarning("Property price seems high ??. Price: {Price}", property.Price);

Logger.LogError("Failed to create property ??. Error: {Message}", ex.Message);

Logger.LogDebug("Querying published properties ??");
```

---

### COMMITS - CORRECTO (SIN emojis)

```powershell
# Cambios en Domain
git commit -m "feat(domain): agregar agregado ContactRequest con validaciones"

# Cambios en Application Services
git commit -m "feat(application): implementar PropertyAppService CRUD completo"

# Cambios en API Controllers
git commit -m "feat(httpapi): crear endpoints para gestión de propiedades"

# Bug fixes
git commit -m "fix(efcore): corregir relación entre Property y Architect"

# Refactorización
git commit -m "refactor(application): optimizar queries con Include para lazy loading"

# Documentación
git commit -m "docs(architecture): actualizar diagrama de entidades"
```

### COMMITS - INCORRECTO (CON emojis)

```powershell
# INCORRECTO - NO HACER ESTO
git commit -m "feat(domain): agregar agregado ContactRequest con validaciones ?"

git commit -m "feat(application): implementar PropertyAppService CRUD completo ??"

git commit -m "fix(efcore): corregir relación entre Property y Architect ??"

git commit -m "refactor(application): optimizar queries ??"
```

---

### VARIABLES Y MÉTODOS - CORRECTO (SIN emojis)

```csharp
// CORRECTO - Nombres claros sin emojis

// Variables
var propertyId = Guid.NewGuid();
var totalCount = properties.Count;
var isPublished = property.Status == PropertyStatus.Published;
var errorMessage = "Failed to retrieve property";

// Métodos
public async Task CreatePropertyAsync(CreateUpdatePropertyDto input) { }
public async Task<PropertyDto> GetPropertyAsync(Guid id) { }
public async Task<List<PropertyDto>> GetPublishedPropertiesAsync() { }
public async Task DeletePropertyAsync(Guid id) { }
public async Task UpdatePropertyStatusAsync(Guid id, PropertyStatus status) { }
```

### VARIABLES Y MÉTODOS - INCORRECTO (CON emojis)

```csharp
// INCORRECTO - NO HACER ESTO

// Variables
var propertyId_?? = Guid.NewGuid();
var totalCount_?? = properties.Count;

// Métodos
public async Task CreateProperty_?Async(CreateUpdatePropertyDto input) { }
public async Task<PropertyDto> GetProperty_??Async(Guid id) { }
public async Task DeleteProperty_???Async(Guid id) { }
```

---

### JSON CONFIG - CORRECTO (SIN emojis)

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cima;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  },
  "App": {
    "SelfUrl": "http://localhost:44350",
    "RootUrl": "http://localhost:44350",
    "CorsOrigins": "http://localhost:4200,http://localhost:44350"
  }
}
```

### JSON CONFIG - INCORRECTO (CON emojis)

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cima;Username=postgres;Password=postgres - Database connection string ???"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information - Log level ??"
    }
  }
}
```

---

### DOCKER - CORRECTO (SIN emojis)

```dockerfile
# Dockerfile - CORRECTO

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /src

# Restore dependencies
COPY ["src/cima.Blazor/cima.Blazor.csproj", "src/cima.Blazor/"]
RUN dotnet restore "src/cima.Blazor/cima.Blazor.csproj"

# Build application
COPY . .
RUN dotnet build "src/cima.Blazor/cima.Blazor.csproj" -c Release -o /app/build

# Publish stage
FROM build as publish
RUN dotnet publish "src/cima.Blazor/cima.Blazor.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 as runtime
WORKDIR /app

# Run as non-root user
RUN useradd -m -u 1000 appuser
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "cima.Blazor.dll"]
```

### DOCKERFILE - INCORRECTO (CON emojis)

```dockerfile
# INCORRECTO - NO HACER ESTO

# Build stage - Building the application ??
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

# Restore dependencies ??
COPY ["src/cima.Blazor/cima.Blazor.csproj", "src/cima.Blazor/"]

# Health check - Service is ready ??
HEALTHCHECK --interval=30s --timeout=3s ...
```

---

## CONTROL DE CALIDAD

### Checklist PRE-COMMIT (TODOS LOS AGENTES)

```powershell
# Validar que NO hay emojis en código
grep -r "[^a-zA-Z0-9 ]" src/cima.Domain/Entities/ | grep -E "[??-????-??]"
# Si devuelve algo: REVISAR

# Validar commits
git log --oneline | grep -E "[??-????-??]"
# Si devuelve algo: NO HACER PUSH

# Validar logs en código
grep -r "Logger\." src/ | grep -E "[??-????-??]"
# Si devuelve algo: REVISAR
```

### CI/CD Validation (GitHub Actions)

```yaml
name: Check No Emojis

on: [push, pull_request]

jobs:
  check-emojis:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Check for emojis in code
        run: |
          if grep -r '[??-????-??]' src/ --include="*.cs" --include="*.razor" --include="Dockerfile"; then
            echo "ERROR: Found emojis in source code. Remove them."
            exit 1
          fi
          
      - name: Check for emojis in logs
        run: |
          if grep -r 'Logger\.[a-zA-Z]*("[^"]*[??-????-??]' src/; then
            echo "ERROR: Found emojis in logging statements. Remove them."
            exit 1
          fi
          
      - name: Check for emojis in commit messages
        run: |
          if git log --pretty=%B | grep -E '[??-????-??]'; then
            echo "ERROR: Found emojis in commit messages. Avoid them."
            exit 1
          fi
```

---

## PENALIDAD POR INCUMPLIMIENTO

Si algún agente añade emojis en código, logs o comentarios:

1. PR será rechazado con comentario: "Please remove emojis from code/logs/comments"
2. Debe corregir y hacer force-push
3. Tres infracciones = revisión manual de todos los commits

---

## EXCEPCIONES PERMITIDAS

Las ÚNICAS excepciones son:

1. En archivos `*.md` (documentación)
2. En README.md
3. En este documento (REGLA_NO_EMOJIS.md)
4. En especificaciones y guías de usuario

TODO lo demás:
- Código C#
- Logs
- Commits
- JSON config
- Docker files
- Scripts
- Etc.

DEBE estar sin emojis.

---

**FIRMADO por todos los agents:**

- GitHub Copilot: Sin emojis en C#, logs, commits
- Google Gemini: Sin emojis en Razor, HTML, CSS, logs
- OpenAI Codex: Sin emojis en Dockerfile, PowerShell, YAML, scripts

---

**Versión:** 1.0  
**Efectiva desde:** Sesión AGENTS  
**Revisión:** Por pull request
