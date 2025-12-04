# Mejoras Implementadas en CIMA

Este documento describe las mejoras implementadas.

---

## Paquetes NuGet Requeridos

Ejecutar el script:
```powershell
./etc/scripts/install-improvement-packages.ps1
```

O manualmente:

```bash
# Redis Cache
dotnet add src/cima.Blazor package Microsoft.Extensions.Caching.StackExchangeRedis

# OpenTelemetry
dotnet add src/cima.Blazor package OpenTelemetry
dotnet add src/cima.Blazor package OpenTelemetry.Extensions.Hosting
dotnet add src/cima.Blazor package OpenTelemetry.Instrumentation.AspNetCore
dotnet add src/cima.Blazor package OpenTelemetry.Instrumentation.Http
dotnet add src/cima.Blazor package OpenTelemetry.Instrumentation.Runtime
dotnet add src/cima.Blazor package OpenTelemetry.Instrumentation.Process
dotnet add src/cima.Blazor package OpenTelemetry.Exporter.Console
dotnet add src/cima.Blazor package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add src/cima.Blazor package OpenTelemetry.Exporter.Prometheus.AspNetCore

# Tests
dotnet add test/cima.Application.Tests package Testcontainers.PostgreSql
dotnet add test/cima.Application.Tests package FluentAssertions
```

---

## 2. Rate Limiting

**Archivo:** `src/cima.Blazor/Infrastructure/RateLimiting/RateLimitingConfiguration.cs`

### Politicas configuradas:
- **FixedPolicy**: 100 requests/minuto - API general
- **SlidingPolicy**: 50 requests/30 segundos - Endpoints sensibles
- **ApiPolicy**: 200 requests/minuto por IP
- **AuthPolicy**: 10 requests/minuto por IP - Login/Register
- **GlobalLimiter**: 1000 requests/5 minutos por IP

### Configuracion en appsettings.json:
```json
{
  "RateLimiting": {
    "FixedWindow": { "PermitLimit": 100, "WindowSeconds": 60 },
    "Api": { "PermitLimit": 200, "WindowSeconds": 60 },
    "Auth": { "PermitLimit": 10, "WindowSeconds": 60 }
  }
}
```

---

## 3. Redis Cache Distribuido

**Archivo:** `src/cima.Blazor/Infrastructure/Caching/RedisCacheConfiguration.cs`

### Uso:
```csharp
private readonly ICacheService _cache;

var data = await _cache.GetOrSetAsync(
    "key",
    async () => await GetDataAsync(),
    TimeSpan.FromMinutes(5));
```

### Configuracion:
```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "localhost:6379",
    "InstanceName": "cima_"
  }
}
```

---

## 9. Paginacion por Cursor

**Archivos:**
- `src/cima.Application.Contracts/Common/CursorPagedResultDto.cs`
- `src/cima.Application.Contracts/Listings/GetListingsCursorInput.cs`
- `src/cima.Application/Listings/ListingCursorPaginationService.cs`

### Ventajas sobre offset:
| Aspecto | Offset | Cursor |
|---------|--------|--------|
| Rendimiento | O(n) | O(1) |
| Deep pagination | Lento | Constante |
| Consistencia | Puede fallar | Consistente |

### Uso:
```
GET /api/listings/cursor?pageSize=20&cursor=eyJ0IjoiMjAyNC...
```

---

## 15. Tests de Integracion con Testcontainers

**Archivos:**
- `test/cima.Application.Tests/Infrastructure/PostgresTestcontainerFixture.cs`
- `test/cima.Application.Tests/Integration/ListingIntegrationTests.cs`

### Requisitos:
- Docker instalado y corriendo

### Ejecutar:
```bash
dotnet test test/cima.Application.Tests
```

---

## 17 & 18. OpenTelemetry y Metricas de Negocio

**Archivo:** `src/cima.Blazor/Infrastructure/Observability/OpenTelemetryConfiguration.cs`

### Metricas disponibles:
- `cima.listings.created` - Listings creados
- `cima.listings.published` - Listings publicados
- `cima.contact_requests.received` - Solicitudes de contacto
- `cima.searches.performed` - Busquedas realizadas
- `cima.searches.duration` - Duracion de busquedas

### Endpoint Prometheus:
```
GET /metrics
```

### Uso de metricas:
```csharp
private readonly CimaMetrics _metrics;

_metrics.RecordListingCreated("residential");
_metrics.RecordSearch(durationMs: 45.5, searchType: "advanced", resultsCount: 23);
```

---

## Integracion en cimaBlazorModule.cs

### 1. Agregar using:
```csharp
using cima.Blazor.Infrastructure;
```

### 2. En ConfigureServices:
```csharp
// Mejoras: Rate Limiting, Redis Cache, OpenTelemetry
context.Services.AddCimaImprovements(configuration, hostingEnvironment);
```

### 3. En OnApplicationInitialization (despues de UseRouting):
```csharp
app.UseCimaImprovements(configuration);
```

---

## Checklist de Implementacion

- [ ] Ejecutar script de instalacion de paquetes
- [ ] Copiar configuracion a appsettings.json
- [ ] Agregar using e integracion en cimaBlazorModule.cs
- [ ] Compilar y verificar
- [ ] Ejecutar tests de integracion (requiere Docker)
