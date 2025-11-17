# ?? COMANDOS PARA RECREAR PROYECTO SIN MULTI-TENANCY

## ?? REGLA CRÍTICA EN TODOS LOS AGENTS

**Ningún agente debe usar emojis en:**
- Código C# (comentarios, strings)
- Logs, output, error messages
- Commits
- Nombres de variables/métodos/clases
- JSON files o configuración

---

## ?? PASOS PARA RECREAR CIMA SIN MULTI-TENANCY

### **Paso 1: Crear nuevo proyecto desde template ABP**

```powershell
# Opción 1: Crear proyecto limpio (recomendado)
# Versión: ABP 9.3.6, .NET 9, sin multi-tenancy

abp new cima-clean --template web-app --version 9.3.6 --create-solution-folder false

# Opciones:
# --template: web-app (Blazor Web App)
# --version: 9.3.6 (versión de ABP)
# --create-solution-folder: false (no crear carpeta adicional)
```

### **Paso 2: Desactivar Multi-Tenancy**

Archivo: `src/cima.Domain.Shared/MultiTenancy/MultiTenancyConsts.cs`

```csharp
namespace cima.MultiTenancy;

public static class MultiTenancyConsts
{
    // Cambiar de true a false
    public const bool IsEnabled = false;
}
```

### **Paso 3: Limpiar módulos relacionados con Multi-Tenancy**

Archivo: `src/cima.Domain/cimaModuleDependency.cs`

```csharp
// Remover:
// typeof(AbpTenantManagementDomainModule)
// typeof(AbpTenantManagementDomainSharedModule)
```

Archivo: `src/cima.Application/cimaApplicationModule.cs`

```csharp
// Remover:
// typeof(AbpTenantManagementApplicationModule)
// typeof(AbpTenantManagementApplicationContractsModule)
```

Archivo: `src/cima.HttpApi/cimaHttpApiModule.cs`

```csharp
// Remover:
// typeof(AbpTenantManagementHttpApiModule)
```

Archivo: `src/cima.Blazor/cimaBlazorModule.cs`

```csharp
// Remover:
// typeof(AbpTenantManagementBlazorModule)
```

### **Paso 4: Actualizar DbContext**

Archivo: `src/cima.EntityFrameworkCore/CimaDbContext.cs`

```csharp
// Eliminar propiedades relacionadas con tenants
// DbSet<Tenant> - ELIMINAR
// DbSet<TenantConnectionString> - ELIMINAR

// Remover en OnModelCreating():
// builder.ConfigureTenantManagement();
```

### **Paso 5: Limpiar Migrations**

```powershell
# Eliminar migration anterior
cd src/cima.EntityFrameworkCore
dotnet ef migrations remove

# Crear nueva migration limpia sin tenants
dotnet ef migrations add InitialCreate -o Migrations
```

### **Paso 6: Ejecutar Migrations**

```powershell
cd src/cima.DbMigrator
dotnet run
```

---

## ?? FULL SCRIPT: Recrear desde cero sin multi-tenancy

```powershell
# 1. Crear proyecto base
abp new cima --template web-app --version 9.3.6 --create-solution-folder false

# 2. Navegar a carpeta
cd cima

# 3. Desactivar multi-tenancy
# Editar: src/cima.Domain.Shared/MultiTenancy/MultiTenancyConsts.cs
# Cambiar: IsEnabled = true ? IsEnabled = false

# 4. Eliminar referencia a TenantManagement
# En estos archivos, comentar o remover módulos relacionados:
# - src/cima.Domain/cimaModuleDependency.cs
# - src/cima.Application/cimaApplicationModule.cs
# - src/cima.HttpApi/cimaHttpApiModule.cs
# - src/cima.Blazor/cimaBlazorModule.cs

# 5. Compilar
dotnet build

# 6. Crear migration inicial
cd src/cima.EntityFrameworkCore
dotnet ef migrations add InitialCreate -o Migrations

# 7. Ejecutar migration
cd ../cima.DbMigrator
dotnet run

# 8. Verificar
# Abre pgAdmin/DBeaver y verifica que NO haya tablas de Tenant

echo "Proyecto CIMA recreado sin multi-tenancy"
```

---

## ?? VERIFICAR QUE MULTI-TENANCY ESTÁ DESACTIVADO

### Checklist:

```powershell
# 1. Verificar const
grep -r "IsEnabled = false" src/cima.Domain.Shared/MultiTenancy/

# 2. Verificar que no hay refs a TenantManagement
grep -r "TenantManagement" src/cima.Domain/ | grep -v "MultiTenancy"
grep -r "TenantManagement" src/cima.Application/ | grep -v "MultiTenancy"
grep -r "TenantManagement" src/cima.HttpApi/ | grep -v "MultiTenancy"
grep -r "TenantManagement" src/cima.Blazor/ | grep -v "MultiTenancy"

# 3. Compilar
dotnet build

# 4. En pgAdmin - NO deberías ver tablas:
# - AbpTenants
# - AbpTenantConnectionStrings
```

---

## ?? COMPARACIÓN: CON vs SIN Multi-Tenancy

### CON Multi-Tenancy (Current):
```
Tablas: AbpTenants, AbpTenantConnectionStrings
Módulos: TenantManagementDomain, TenantManagementApplication
DbSets: Tenant, TenantConnectionString
Features: Isolación de datos por tenant
```

### SIN Multi-Tenancy (Después de cambios):
```
Tablas: Ninguna relacionada con tenants
Módulos: Solo Identity, Permissions, Settings
DbSets: Solo usuarios, roles, permisos
Features: Aplicación simple de usuario único
```

---

## ?? MIGRACIÓN DE DATOS (si ya tienes datos)

Si ya tienes datos en la BD con multi-tenancy y quieres migrar:

```sql
-- 1. Backup primero
pg_dump -U postgres cima > cima_backup.sql

-- 2. Crear migration custom en EF Core
-- Que no incluya tablas de Tenant

-- 3. Transferir datos necesarios
-- (omitir datos de Tenant)

-- 4. Ejecutar migration limpia
dotnet ef database update
```

---

## ? RESULTADO ESPERADO

Después de estos cambios:

- Proyecto compilable sin advertencias
- Multi-tenancy completamente desactivado
- BD con esquema limpio (sin tablas de tenants)
- Todos los servicios funcionando sin tenant context
- Próximo: Crear entidades de dominio (Property, Architect, etc.)

---

## ?? REGLA PARA TODOS LOS AGENTS

Cuando creen código, comentarios o logs relacionado con estos cambios:

```csharp
// CORRECTO: Comentario claro sin emojis
public class PropertyRepository : RepositoryBase<Property, Guid>
{
    // Retrieve published properties from database
    public async Task<List<Property>> GetPublishedAsync()
    {
        return await (await GetQueryableAsync())
            .Where(p => p.Status == PropertyStatus.Published)
            .ToListAsync();
    }
}

// INCORRECTO: Con emojis
// Get published properties ??
public async Task<List<Property>> GetPublishedAsync() { }
```

No usar emojis en:
- Comentarios de código
- Logs (Logger.LogInformation, etc.)
- Strings en código
- Commits
- Variable names
- Method names

---

**Versión:** 1.0  
**Actualización:** Setup sin multi-tenancy  
**Para:** Copilot, Gemini, Codex
