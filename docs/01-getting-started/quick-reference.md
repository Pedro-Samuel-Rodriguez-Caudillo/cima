# COMANDOS RÁPIDA REFERENCIA

## Setup Proyecto CIMA Sin Multi-Tenancy

### Crear Proyecto Nuevo

```powershell
# Crear proyecto con ABP template
abp new cima --template web-app --version 9.3.6 --create-solution-folder false

# Navegar a carpeta
cd cima

# Compilar y verificar
dotnet build
```

---

## Desactivar Multi-Tenancy

### 1. Cambiar Constante

Archivo: `src/cima.Domain.Shared/MultiTenancy/MultiTenancyConsts.cs`

```csharp
public static class MultiTenancyConsts
{
    public const bool IsEnabled = false;  // Cambiar de true a false
}
```

### 2. Limpiar Módulos

En cada archivo, comentar o remover referencias a TenantManagement:

```powershell
# Buscar referencias
grep -r "TenantManagement" src/

# Archivos a editar:
# - src/cima.Domain/cimaModuleDependency.cs
# - src/cima.Application/cimaApplicationModule.cs  
# - src/cima.HttpApi/cimaHttpApiModule.cs
# - src/cima.Blazor/cimaBlazorModule.cs
```

### 3. Actualizar DbContext

Archivo: `src/cima.EntityFrameworkCore/CimaDbContext.cs`

```csharp
// Remover estas líneas:
// public DbSet<Tenant> Tenants { get; set; }
// public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

// Remover en OnModelCreating():
// builder.ConfigureTenantManagement();
```

### 4. Generar Migrations

```powershell
# Ir a proyecto EntityFrameworkCore
cd src/cima.EntityFrameworkCore

# Remover migration anterior (si existe)
dotnet ef migrations remove

# Crear nueva migration limpia
dotnet ef migrations add InitialCreate -o Migrations

# Verificar que se creó
ls Migrations/
```

### 5. Ejecutar Migrations

```powershell
# Ir a DbMigrator
cd ../cima.DbMigrator

# Ejecutar
dotnet run

# Output esperado:
# Migrating database...
# Migration completed successfully!
```

---

## Setup PostgreSQL Local

### Con Docker

```powershell
# Crear y levantar contenedor
docker run --name cima-postgres `
  -e POSTGRES_DB=cima `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -p 5432:5432 `
  -d postgres:16-alpine

# Verificar que está corriendo
docker ps

# Si ya existe, reiniciar
docker start cima-postgres
```

### Conexión

```
Host: localhost
Port: 5432
Database: cima
User: postgres
Password: postgres
```

---

## Archivos Secretos

### Crear: `appsettings.secrets.json`

```powershell
# Ubicación: src/cima.DbMigrator/appsettings.secrets.json

# Contenido:
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cima;Username=postgres;Password=postgres"
  }
}
```

### Agregar a .gitignore

```powershell
# Ubicación: .gitignore (raíz del proyecto)

# Agregar estas líneas:
appsettings.secrets.json
appsettings.Production.secrets.json
*.secrets.json
```

---

## Compilación y Build

```powershell
# Compilar solución completa
dotnet build

# Compilar proyecto específico
dotnet build src/cima.Domain

# Clean antes de build
dotnet clean
dotnet build

# Restore de dependencias
dotnet restore
```

---

## Entity Framework Core

```powershell
# Crear migration
cd src/cima.EntityFrameworkCore
dotnet ef migrations add NombreMigration

# Remover última migration
dotnet ef migrations remove

# Actualizar BD
dotnet ef database update

# Ver migrations
dotnet ef migrations list

# Ver estado actual
dotnet ef database info
```

---

## Git Comandos Básicos

```powershell
# Verificar estado
git status

# Ver cambios
git diff

# Agregar cambios
git add .
git add src/cima.Domain/

# Commit (formato semántico)
git commit -m "feat(domain): agregar agregado ContactRequest"

# Push
git push origin master
git push origin feature-branch

# Ver historial
git log --oneline

# Crear rama
git checkout -b feat/contact-requests

# Cambiar rama
git checkout master
```

---

## Docker (Desarrollo)

```powershell
# Construir imagen
docker build -t cima:latest .

# Construir sin cache
docker build --no-cache -t cima:latest .

# Ver imágenes
docker images

# Ejecutar contenedor
docker run -p 8080:8080 cima:latest

# Detener contenedor
docker stop container-id
docker stop $(docker ps -q)

# Ver logs
docker logs container-id
docker logs -f container-id

# Ver estadísticas
docker stats
```

---

## Docker Compose (Desarrollo Local)

```powershell
# Levantar servicios
docker-compose up -d

# Levantar sin cache
docker-compose build --no-cache
docker-compose up -d

# Ver logs
docker-compose logs -f app
docker-compose logs -f postgres

# Detener
docker-compose down

# Remover volúmenes (CUIDADO: elimina datos)
docker-compose down -v

# Entrar a contenedor
docker exec -it cima-app /bin/sh
docker exec -it cima-postgres psql -U postgres
```

---

## DBeaver / pgAdmin (BD Management)

```powershell
# Descargar DBeaver Community
# https://dbeaver.io/download/

# Descargar pgAdmin
# https://www.pgadmin.org/download/

# Conexión SQL desde psql
docker exec -it cima-postgres psql -U postgres -d cima

# Ver tablas
SELECT table_name FROM information_schema.tables WHERE table_schema='public';

# Ver contenido tabla
SELECT * FROM "Properties" LIMIT 10;

# Backup BD
docker exec -it cima-postgres pg_dump -U postgres cima > backup.sql

# Restore BD
docker exec -i cima-postgres psql -U postgres cima < backup.sql
```

---

## IMPORTANTE: Regla SIN EMOJIS

Cuando uses estos comandos:

**CORRECTO:**
```csharp
// Comentario sin emojis
git commit -m "feat(domain): agregar ContactRequest"
Logger.LogInformation("Database migrated successfully");
```

**INCORRECTO:**
```csharp
// Comentario con emoji ??
git commit -m "feat(domain): agregar ContactRequest ?"
Logger.LogInformation("Database migrated successfully ??");
```

---

**Versión:** 1.0  
**Última actualización:** Setup sin multi-tenancy  
**Para:** Todos los agents (Copilot, Gemini, Codex)
