# Sistema de Logging CIMA

## Ubicacion de Logs

```
src/
??? cima.Blazor/
?   ??? Logs/
?       ??? logs.txt          ? Logs de la aplicacion web
?
??? cima.DbMigrator/
    ??? Logs/
        ??? logs.txt          ? Logs de migraciones
```

## Configuracion Actual

### Blazor Web App

**Archivo:** `src/cima.Blazor/Program.cs`

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(c => c.File("Logs/logs.txt"))
    .WriteTo.Async(c => c.Console())
    .CreateBootstrapLogger();
```

**Niveles de log:**
- **DEBUG** (solo en modo desarrollo)
- **INFORMATION** (produccion)
- **WARNING** (Microsoft y EF Core)

### DbMigrator

**Archivo:** `src/cima.DbMigrator/Program.cs`

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
    .MinimumLevel.Override("cima", LogEventLevel.Debug) // Solo en DEBUG
    .WriteTo.Async(c => c.File("Logs/logs.txt"))
    .WriteTo.Async(c => c.Console())
    .CreateLogger();
```

## Scripts de Utilidad

### Ver Logs

```powershell
# Ver todos los logs (ultimas 50 lineas)
.\etc\scripts\ver-logs.ps1

# Ver solo Blazor
.\etc\scripts\ver-logs.ps1 -Proyecto blazor

# Ver solo DbMigrator
.\etc\scripts\ver-logs.ps1 -Proyecto migrator

# Ver mas lineas
.\etc\scripts\ver-logs.ps1 -Lineas 100
```

### Limpiar Logs

```powershell
# Limpiar todos los logs (crea backup si > 100KB)
.\etc\scripts\limpiar-logs.ps1
```

### Abrir en Editor

```powershell
# Notepad
notepad src\cima.Blazor\Logs\logs.txt

# VS Code
code src\cima.Blazor\Logs\logs.txt

# Tail en tiempo real (PowerShell)
Get-Content src\cima.Blazor\Logs\logs.txt -Wait -Tail 20
```

## Formato de Logs

### Ejemplo de Salida

```
2024-01-15 10:30:45 [INF] Starting web host.
2024-01-15 10:30:46 [DBG] Database connection opened
2024-01-15 10:30:47 [WRN] Slow query detected: 1500ms
2024-01-15 10:30:48 [ERR] Failed to connect to database
```

### Colores en Script

- **ROJO** = Errores (ERR)
- **AMARILLO** = Warnings (WRN)
- **VERDE** = Informacion (INF)
- **GRIS** = Debug (DBG)

## Mejores Practicas

### Usar ILogger en Servicios

```csharp
public class ListingAppService : cimaAppService
{
    private readonly ILogger<ListingAppService> _logger;

    public ListingAppService(ILogger<ListingAppService> logger)
    {
        _logger = logger;
    }

    public async Task<ListingDto> CreateAsync(CreateListingDto input)
    {
        _logger.LogInformation("Creating listing: {Title}", input.Title);
        
        try
        {
            // logica...
            _logger.LogDebug("Listing created with ID: {Id}", listing.Id);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating listing: {Title}", input.Title);
            throw;
        }
    }
}
```

### Niveles de Log Recomendados

| Nivel | Cuando Usar | Ejemplo |
|-------|-------------|---------|
| **Trace** | Debugging muy detallado | Entrada/salida de metodos |
| **Debug** | Informacion de desarrollo | Valores de variables, flujo |
| **Information** | Eventos normales | Usuario login, entidad creada |
| **Warning** | Situaciones inesperadas | Query lento, cache miss |
| **Error** | Errores manejados | Exception capturada |
| **Critical** | Errores criticos | DB no disponible, app crash |

## Configuracion Avanzada

### Rotacion de Logs (Futuro)

Para evitar archivos muy grandes, puedes configurar rotacion:

```csharp
.WriteTo.File(
    "Logs/logs.txt",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7, // Mantener 7 dias
    fileSizeLimitBytes: 10_000_000 // 10 MB max
)
```

### Logs Estructurados (JSON)

Para integracion con herramientas de analisis:

```csharp
.WriteTo.File(
    new JsonFormatter(),
    "Logs/logs.json"
)
```

### Enviar a Servicio Externo

```csharp
// Seq (https://datalust.co/seq)
.WriteTo.Seq("http://localhost:5341")

// Application Insights
.WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)

// Elasticsearch
.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200")))
```

## Troubleshooting

### Logs no se generan

1. Verificar permisos de escritura en carpeta `Logs/`
2. Verificar que la aplicacion este corriendo
3. Revisar consola para errores de Serilog

### Archivo de log muy grande

```powershell
# Ver tamaño
(Get-Item src\cima.Blazor\Logs\logs.txt).Length / 1MB

# Limpiar
.\etc\scripts\limpiar-logs.ps1
```

### Ver logs en tiempo real

```powershell
# PowerShell
Get-Content src\cima.Blazor\Logs\logs.txt -Wait -Tail 50

# Git Bash / Linux
tail -f src/cima.Blazor/Logs/logs.txt
```

## Integracion con Monitoring

### Health Checks con Logs

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        _logger.LogInformation("Health check: {Status}", report.Status);
        // ...
    }
});
```

### Metricas de Rendimiento

```csharp
using var operation = _logger.BeginScope("CreateListing");
var sw = Stopwatch.StartNew();

// logica...

sw.Stop();
_logger.LogInformation("Listing created in {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

## Scripts Relacionados

- `etc/scripts/ver-logs.ps1` - Ver logs con colores
- `etc/scripts/limpiar-logs.ps1` - Limpiar logs
- `etc/scripts/reset-database.ps1` - Logs del proceso de reset

## Referencias

- Serilog: https://serilog.net/
- ABP Logging: https://docs.abp.io/en/abp/latest/Logging
- .NET Logging: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging
