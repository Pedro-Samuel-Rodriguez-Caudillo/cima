# Comandos de Verificación Rápida - Día 4

## ?? Inicio Rápido

### 1. Compilar y Verificar
```powershell
# Compilar toda la solución
dotnet build C:\Users\rodri\Documents\Inmobiliaria\cima\cima.sln

# Compilar solo Application layer
dotnet build C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.Application\cima.Application.csproj

# Verificar errores de compilación
dotnet build --no-incremental --verbosity detailed
```

---

### 2. Ejecutar la Aplicación
```powershell
# Iniciar PostgreSQL (si no está corriendo)
docker start cima-postgres

# Aplicar migraciones (si hay pendientes)
cd C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.DbMigrator
dotnet run

# Iniciar la aplicación Blazor
cd C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.Blazor
dotnet run
```

**URLs disponibles**:
- Blazor UI: https://localhost:44307
- Swagger: https://localhost:44307/swagger
- API: https://localhost:44307/api

---

## ?? Probar Nuevos Endpoints

### 1. Obtener Propiedades Publicadas (SIN AUTENTICACIÓN)
```powershell
# PowerShell
Invoke-WebRequest -Uri "https://localhost:44307/api/app/listing/published?SkipCount=0&MaxResultCount=10" `
    -Method GET `
    -UseBasicParsing

# o con curl
curl -X GET "https://localhost:44307/api/app/listing/published?SkipCount=0&MaxResultCount=10" `
     -k
```

**Filtros disponibles**:
```
?SearchTerm=casa
&MinPrice=100000
&MaxPrice=500000
&MinBedrooms=2
&PropertyType=0        ? NUEVO (0=House, 1=Apartment, etc.)
&TransactionType=0     ? NUEVO (0=Sale, 1=Rent, 2=Lease)
&Sorting=pricedesc
&SkipCount=0
&MaxResultCount=20
```

---

### 2. Publicar Propiedad (REQUIERE TOKEN)
```powershell
# Primero, obtener token de login
$loginBody = @{
    username = "admin"
    password = "1q2w3E*"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "https://localhost:44307/api/account/login" `
    -Method POST `
    -Body $loginBody `
    -ContentType "application/json"

$token = $loginResponse.access_token

# Publicar propiedad
Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/{LISTING_ID}/publish" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" } `
    -ContentType "application/json"
```

---

### 3. Desarchivar Propiedad (NUEVO)
```powershell
Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/{LISTING_ID}/unarchive" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" } `
    -ContentType "application/json"
```

---

### 4. Despublicar Propiedad (NUEVO)
```powershell
Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/{LISTING_ID}/unpublish" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" } `
    -ContentType "application/json"
```

---

### 5. Obtener Propiedades de un Arquitecto
```powershell
Invoke-WebRequest -Uri "https://localhost:44307/api/app/listing/by-architect/{ARCHITECT_ID}?skipCount=0&maxResultCount=20" `
    -Method GET `
    -Headers @{ Authorization = "Bearer $token" }
```

---

### 6. Estadísticas del Dashboard (ADMIN)
```powershell
Invoke-RestMethod -Uri "https://localhost:44307/api/app/statistics/dashboard" `
    -Method GET `
    -Headers @{ Authorization = "Bearer $token" }
```

---

## ?? Verificar Base de Datos

### Conectar a PostgreSQL
```powershell
# PowerShell
docker exec -it cima-postgres psql -U postgres -d cima
```

```sql
-- Ver propiedades por estado
SELECT 
    "Status",
    "Type",
    "TransactionType",
    COUNT(*) as "Count"
FROM "Listings"
GROUP BY "Status", "Type", "TransactionType";

-- Ver arquitectos con sus propiedades
SELECT 
    a."Id",
    a."Bio",
    COUNT(l."Id") as "TotalListings"
FROM "Architects" a
LEFT JOIN "Listings" l ON l."ArchitectId" = a."Id"
GROUP BY a."Id", a."Bio";

-- Ver solicitudes de contacto por estado
SELECT 
    "Status",
    COUNT(*) as "Count",
    MAX("CreatedAt") as "MostRecent"
FROM "ContactRequests"
GROUP BY "Status";

-- Verificar campo RepliedAt agregado
SELECT 
    "Id",
    "Status",
    "CreatedAt",
    "RepliedAt",
    "RepliedAt" - "CreatedAt" as "ResponseTime"
FROM "ContactRequests"
WHERE "RepliedAt" IS NOT NULL
ORDER BY "CreatedAt" DESC
LIMIT 5;
```

---

## ?? Verificar Swagger

### Abrir Swagger UI
```powershell
Start-Process "https://localhost:44307/swagger"
```

### Endpoints a verificar en Swagger:

#### Listings
- ? `GET /api/app/listing/published` (público)
- ? `GET /api/app/listing`
- ? `POST /api/app/listing/{id}/publish`
- ? `POST /api/app/listing/{id}/archive`
- ? `POST /api/app/listing/{id}/unarchive` ? NUEVO
- ? `POST /api/app/listing/{id}/unpublish` ? NUEVO
- ? `GET /api/app/listing/by-architect/{architectId}`

#### Architects
- ? `GET /api/app/architect/{id}` (público)
- ? `GET /api/app/architect/by-user-id/{userId}` (público)
- ? `POST /api/app/architect`
- ? `PUT /api/app/architect/{id}`
- ? `DELETE /api/app/architect/{id}`

#### Statistics
- ? `GET /api/app/statistics/dashboard`
- ? `GET /api/app/statistics/listing-stats`
- ? `GET /api/app/statistics/contact-request-stats`

---

## ?? Limpiar y Resetear (si es necesario)

### Limpiar Binarios
```powershell
# Limpiar todos los proyectos
dotnet clean C:\Users\rodri\Documents\Inmobiliaria\cima\cima.sln

# Eliminar carpetas bin y obj
Get-ChildItem -Path "C:\Users\rodri\Documents\Inmobiliaria\cima" -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
```

### Resetear Base de Datos
```powershell
# Detener y eliminar contenedor
docker stop cima-postgres
docker rm cima-postgres

# Recrear contenedor limpio
docker run --name cima-postgres `
  -e POSTGRES_DB=cima `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  -d postgres:16-alpine

# Esperar 5 segundos
Start-Sleep -Seconds 5

# Aplicar migraciones
cd C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.DbMigrator
dotnet run
```

---

## ?? Verificar Logs

### Ver logs de la aplicación
```powershell
# Logs más recientes
Get-Content "C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.Blazor\Logs\logs.txt" -Tail 50

# Logs con errores
Select-String -Path "C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.Blazor\Logs\*.txt" -Pattern "ERROR" | Select-Object -Last 10

# Logs con warnings de publicación sin imágenes
Select-String -Path "C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.Blazor\Logs\*.txt" -Pattern "Publicando propiedad" | Select-Object -Last 5
```

---

## ?? Gestión de Permisos

### Verificar permisos del rol admin
```sql
-- En psql
SELECT 
    pg."Name" as "Permission",
    pg."ProviderName",
    pg."ProviderKey"
FROM "AbpPermissionGrants" pg
WHERE pg."ProviderKey" = 'admin'
ORDER BY pg."Name";
```

### Verificar que existan todos los permisos de CIMA
```sql
SELECT 
    "Name",
    "GroupName",
    "DisplayName"
FROM "AbpPermissions"
WHERE "GroupName" = 'cima'
ORDER BY "Name";
```

---

## ?? Solución de Problemas Comunes

### Problema 1: "Connection refused" al conectar a PostgreSQL
```powershell
# Verificar que el contenedor está corriendo
docker ps | Select-String cima-postgres

# Si no está corriendo, iniciarlo
docker start cima-postgres

# Ver logs del contenedor
docker logs cima-postgres
```

### Problema 2: "Migration already applied"
```powershell
# Ver migraciones aplicadas
cd C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.EntityFrameworkCore
dotnet ef migrations list

# Eliminar última migración (si es necesario)
dotnet ef migrations remove
```

### Problema 3: "Unauthorized" al llamar endpoint
```powershell
# Verificar que el token no haya expirado
# Los tokens de ABP expiran en 1 hora por defecto

# Obtener nuevo token
$loginBody = @{
    username = "admin"
    password = "1q2w3E*"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "https://localhost:44307/api/account/login" `
    -Method POST `
    -Body $loginBody `
    -ContentType "application/json"

$token = $loginResponse.access_token
Write-Host "Nuevo token: $token"
```

### Problema 4: "Warning sin imágenes" al publicar
```powershell
# Este es un warning esperado, no un error
# Para verificar que se registró en logs:

Select-String -Path "C:\Users\rodri\Documents\Inmobiliaria\cima\src\cima.Blazor\Logs\*.txt" `
    -Pattern "Publicando propiedad .* sin imágenes"
```

---

## ?? Scripts de Testing Rápido

### Test completo de flujo de estados
```powershell
# 1. Crear propiedad (Draft)
$createBody = @{
    title = "Test Property"
    description = "Test description"
    location = "Test City"
    price = 150000
    area = 100
    bedrooms = 3
    bathrooms = 2
    architectId = "{ARCHITECT_GUID}"
} | ConvertTo-Json

$newListing = Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" } `
    -Body $createBody `
    -ContentType "application/json"

$listingId = $newListing.id
Write-Host "Created Listing: $listingId with Status: $($newListing.status)" # Should be 0 (Draft)

# 2. Publicar (Draft ? Published)
$published = Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/$listingId/publish" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" }

Write-Host "Published Listing Status: $($published.status)" # Should be 1 (Published)

# 3. Archivar (Published ? Archived)
$archived = Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/$listingId/archive" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" }

Write-Host "Archived Listing Status: $($archived.status)" # Should be 2 (Archived)

# 4. Desarchivar (Archived ? Published)
$unarchived = Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/$listingId/unarchive" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" }

Write-Host "Unarchived Listing Status: $($unarchived.status)" # Should be 1 (Published)

# 5. Despublicar (Published ? Draft)
$unpublished = Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/$listingId/unpublish" `
    -Method POST `
    -Headers @{ Authorization = "Bearer $token" }

Write-Host "Unpublished Listing Status: $($unpublished.status)" # Should be 0 (Draft)

# 6. Limpiar (eliminar propiedad de prueba)
Invoke-RestMethod -Uri "https://localhost:44307/api/app/listing/$listingId" `
    -Method DELETE `
    -Headers @{ Authorization = "Bearer $token" }

Write-Host "Test completed successfully!"
```

---

## ?? Checklist de Verificación Post-Día 4

### Compilación
- [ ] `dotnet build` sin errores
- [ ] Solo warnings esperados (nullable references, etc.)
- [ ] Tiempo de compilación < 10s

### Base de Datos
- [ ] PostgreSQL corriendo
- [ ] Migraciones aplicadas
- [ ] Seeders ejecutados
- [ ] Campo `RepliedAt` existe en `ContactRequests`

### Endpoints
- [ ] `/api/app/listing/published` accesible sin token
- [ ] `/api/app/listing/{id}/publish` funciona con token
- [ ] `/api/app/listing/{id}/unarchive` funciona (NUEVO)
- [ ] `/api/app/listing/{id}/unpublish` funciona (NUEVO)
- [ ] Filtros `PropertyType` y `TransactionType` funcionan

### Swagger
- [ ] Swagger UI carga correctamente
- [ ] Todos los endpoints visibles
- [ ] Autenticación con Bearer token funciona
- [ ] Modelos de request/response visibles

### Permisos
- [ ] Rol "admin" tiene todos los permisos
- [ ] Endpoints públicos accesibles sin token
- [ ] Endpoints protegidos rechazan sin token
- [ ] Owner checks funcionan correctamente

### Logging
- [ ] Warning se registra al publicar sin imágenes
- [ ] Logs se guardan en `Logs/` correctamente
- [ ] No hay errores críticos en logs

---

## ?? Recursos Útiles

### Documentación
- **Guía completa Día 4**: `docs/DIA_4_METODOS_LISTING_COMPLETADOS.md`
- **Resumen ejecutivo**: `docs/DIA_4_RESUMEN_EJECUTIVO.md`
- **Este archivo**: `docs/DIA_4_COMANDOS_VERIFICACION.md`

### Comandos de emergencia
```powershell
# Si todo falla, reset completo:
# 1. Detener aplicación (Ctrl+C)
# 2. Limpiar binarios
dotnet clean

# 3. Resetear BD
docker stop cima-postgres; docker rm cima-postgres
docker run --name cima-postgres -e POSTGRES_DB=cima -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine

# 4. Esperar y migrar
Start-Sleep -Seconds 5
cd src/cima.DbMigrator; dotnet run

# 5. Compilar y ejecutar
cd ../cima.Blazor; dotnet build; dotnet run
```

---

**Última actualización**: Día 4 Post-Implementación  
**Siguiente**: Día 5 - CORS, Swagger, FluentValidation  
**Estado**: ? LISTO PARA VERIFICACIÓN
