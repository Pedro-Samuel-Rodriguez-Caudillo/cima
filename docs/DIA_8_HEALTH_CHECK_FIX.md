# SOLUCION HEALTH CHECK ENDPOINTS - RESUMEN COMPLETO

## PROBLEMA ORIGINAL
Los endpoints `/health`, `/health/ping`, y `/health/ready` no funcionaban.

## SOLUCION IMPLEMENTADA

### 1. Paquete NuGet Agregado
```powershell
cd src/cima.Blazor
dotnet add package AspNetCore.HealthChecks.NpgSql
```

### 2. Archivos Modificados

#### A. `src/cima.Blazor/cimaBlazorModule.cs`
- Agregados using statements para HealthChecks
- Agregado método `ConfigureHealthChecks` con configuración de PostgreSQL
- Mapeados endpoints en `OnApplicationInitialization`:
  - `/health` - Health check completo con detalles
  - `/health/ready` - Readiness probe (para Kubernetes/Docker)
  - `/health/live` - Liveness probe (siempre retorna OK)

#### B. `src/cima.Blazor/Controllers/HealthController.cs`
- Actualizado para usar rutas con prefix `/api`
- Endpoints:
  - `GET /api/health` - Verifica BD y retorna estado completo
  - `GET /api/health/ping` - Liveness simple (no accede BD)
  - `GET /api/health/ready` - Readiness avanzado (verifica migraciones)

#### C. `docker-compose.prod.yml`
- Actualizado healthcheck para usar `/api/health/ping`

### 3. Nuevo Script
**`etc/scripts/test-health-endpoints.ps1`** - Script para probar todos los endpoints

---

## ENDPOINTS DISPONIBLES

### Opcion 1: Health Checks Nativos de ASP.NET Core
Estos son los que configuramos en `cimaBlazorModule.cs` usando `MapHealthChecks`:

| Endpoint | Descripción | Uso |
|----------|-------------|-----|
| `/health` | Health check completo con detalles de BD | Monitoreo general |
| `/health/ready` | Verifica que BD esté lista | Readiness probe Kubernetes |
| `/health/live` | Siempre retorna 200 OK | Liveness probe Kubernetes |

### Opcion 2: API Controller Custom
Estos son del `HealthController` personalizado:

| Endpoint | Descripción | Uso |
|----------|-------------|-----|
| `/api/health` | Verifica BD + cuenta de listings | Monitoring detallado |
| `/api/health/ping` | Simple pong response | Liveness Docker/curl |
| `/api/health/ready` | Verifica BD + migraciones pendientes | Readiness completo |

---

## COMO PROBAR

### PASO 1: Detener la Aplicación
```powershell
# Si la aplicación está corriendo, detenerla
# En Visual Studio: Stop debugging
# O en terminal donde corre: Ctrl+C
```

### PASO 2: Compilar
```powershell
# En la raíz del proyecto
dotnet build
```

### PASO 3: Iniciar la Aplicación
```powershell
cd src/cima.Blazor
dotnet run
```

### PASO 4: Probar Endpoints (en otra terminal)

#### Opción A: Usar el Script PowerShell
```powershell
./etc/scripts/test-health-endpoints.ps1
```

#### Opción B: Probar Manualmente

**Health checks nativos:**
```powershell
# Health check completo
Invoke-WebRequest -Uri "http://localhost:8080/health" -Method Get

# Ready check
Invoke-WebRequest -Uri "http://localhost:8080/health/ready" -Method Get

# Live check
Invoke-WebRequest -Uri "http://localhost:8080/health/live" -Method Get
```

**Health API Controller:**
```powershell
# Health detallado
Invoke-WebRequest -Uri "http://localhost:8080/api/health" -Method Get

# Ping
Invoke-WebRequest -Uri "http://localhost:8080/api/health/ping" -Method Get

# Ready detallado
Invoke-WebRequest -Uri "http://localhost:8080/api/health/ready" -Method Get
```

#### Opción C: Usar curl
```bash
# Health checks nativos
curl http://localhost:8080/health
curl http://localhost:8080/health/ready
curl http://localhost:8080/health/live

# Health API Controller
curl http://localhost:8080/api/health
curl http://localhost:8080/api/health/ping
curl http://localhost:8080/api/health/ready
```

#### Opción D: Usar el Navegador
```
http://localhost:8080/health
http://localhost:8080/api/health
http://localhost:8080/api/health/ping
```

---

## RESPUESTAS ESPERADAS

### `/health` (nativo)
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-17T12:00:00Z",
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "description": null,
      "duration": 45.23
    }
  ]
}
```

### `/api/health` (custom)
```json
{
  "status": "healthy",
  "timestamp": "2025-11-17T12:00:00Z",
  "database": "connected",
  "application": "cima.Blazor",
  "version": "1.0.0",
  "environment": "Development",
  "checks": {
    "database_connection": "ok",
    "listings_table": "ok",
    "total_listings": 5
  }
}
```

### `/api/health/ping`
```json
{
  "status": "alive",
  "timestamp": "2025-11-17T12:00:00Z",
  "message": "pong",
  "application": "cima.Blazor"
}
```

### `/api/health/ready`
```json
{
  "ready": true,
  "timestamp": "2025-11-17T12:00:00Z",
  "checks": {
    "database": "ready",
    "migrations": "applied"
  }
}
```

---

## USO EN DOCKER

### docker-compose.prod.yml
```yaml
blazor-prod:
  healthcheck:
    test: ["CMD-SHELL", "curl -f http://localhost:8080/api/health/ping || exit 1"]
    interval: 30s
    timeout: 10s
    retries: 3
    start_period: 40s
```

### Verificar Health en Docker
```powershell
# Ver estado del contenedor
docker ps

# Ver logs de health check
docker inspect --format='{{json .State.Health}}' cima-blazor-prod

# Ejecutar health check manualmente
docker exec cima-blazor-prod curl http://localhost:8080/api/health/ping
```

---

## TROUBLESHOOTING

### Error: "Cannot connect to database"
**Solución:**
1. Verificar que PostgreSQL está corriendo
2. Verificar connection string en `appsettings.json`
3. Verificar que DbMigrator ejecutó las migraciones

```powershell
# Ver servicios Docker
docker ps

# Ver logs de PostgreSQL
docker logs cima-postgres-dev

# Ejecutar migraciones
cd src/cima.DbMigrator
dotnet run
```

### Error: "Pending migrations"
**Solución:**
```powershell
cd src/cima.DbMigrator
dotnet run
```

### Error: "404 Not Found"
**Causas posibles:**
1. La aplicación no está corriendo
2. Puerto incorrecto (debería ser 8080)
3. Endpoint URL incorrecto

**Solución:**
```powershell
# Verificar que la app corre
curl http://localhost:8080

# Ver endpoints disponibles en Swagger
# Abrir: http://localhost:8080/swagger
```

### Error: "Connection refused"
**Solución:**
1. Iniciar la aplicación
2. Verificar que usa puerto 8080
3. Verificar firewall no bloquea puerto

---

## CUAL ENDPOINT USAR Y CUANDO

### Para Docker/Kubernetes
- **Liveness**: `/api/health/ping` (no accede BD, más rápido)
- **Readiness**: `/health/ready` (verifica BD está lista)

### Para Monitoreo/Dashboards
- `/health` - Vista completa del estado
- `/api/health` - Información detallada incluyendo contadores

### Para Debugging
- `/api/health` - Muestra detalles completos incluyendo conteo de registros
- `/api/health/ready` - Verifica migraciones pendientes

### Para Load Balancers
- `/health/live` - Verificación rápida sin lógica
- `/api/health/ping` - Simple pong response

---

## PROXIMO PASO

### 1. DETENER LA APLICACION
Si está corriendo en Visual Studio o terminal, detenerla.

### 2. COMPILAR Y EJECUTAR
```powershell
# Compilar
dotnet build

# Ejecutar
cd src/cima.Blazor
dotnet run
```

### 3. EN OTRA TERMINAL, PROBAR
```powershell
# Opcion A: Script automatizado
./etc/scripts/test-health-endpoints.ps1

# Opcion B: Manual
curl http://localhost:8080/health
curl http://localhost:8080/api/health
curl http://localhost:8080/api/health/ping
```

### 4. VERIFICAR RESPUESTAS
Todos los endpoints deben retornar:
- Status code: 200 OK
- Content-Type: application/json
- Status: "Healthy" o "healthy"

---

## ARCHIVOS MODIFICADOS

1. `src/cima.Blazor/cimaBlazorModule.cs` - Configuración health checks
2. `src/cima.Blazor/Controllers/HealthController.cs` - Controller con endpoints custom
3. `docker-compose.prod.yml` - Health check en Docker
4. `src/cima.Blazor/cima.Blazor.csproj` - Paquete NuGet agregado
5. `etc/scripts/test-health-endpoints.ps1` - Script de prueba (NUEVO)

---

## COMMIT SUGERIDO

```powershell
git add .
git commit -m "fix(health): configurar health check endpoints para Docker/Kubernetes

AGREGADO:
- Health checks nativos ASP.NET Core (/health, /health/ready, /health/live)
- Health API Controller (/api/health, /api/health/ping, /api/health/ready)
- Paquete AspNetCore.HealthChecks.NpgSql
- Script test-health-endpoints.ps1

MODIFICADO:
- cimaBlazorModule.cs: ConfigureHealthChecks + MapHealthChecks
- HealthController.cs: rutas con /api prefix
- docker-compose.prod.yml: healthcheck usa /api/health/ping

ENDPOINTS:
- GET /health - Health check completo
- GET /health/ready - Readiness probe
- GET /health/live - Liveness probe
- GET /api/health - Health detallado con contadores
- GET /api/health/ping - Liveness simple
- GET /api/health/ready - Readiness con verificacion migraciones

TESTING:
./etc/scripts/test-health-endpoints.ps1
curl http://localhost:8080/health
curl http://localhost:8080/api/health/ping"
```

---

**ESTADO**: COMPLETADO ?
**TESTING**: PENDIENTE ? (requiere detener app actual y reiniciar)
