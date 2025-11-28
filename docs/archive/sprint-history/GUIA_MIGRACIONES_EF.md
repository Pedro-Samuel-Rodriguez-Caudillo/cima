# Guia de Migraciones Entity Framework Core

## Comandos Rapidos

### Crear Nueva Migracion

```powershell
# Opcion 1: Con script automatizado (RECOMENDADO)
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarCampoX"

# Opcion 2: Manual
cd src\cima.EntityFrameworkCore
dotnet ef migrations add AgregarCampoX
cd ..\..
```

### Crear y Aplicar Migracion

```powershell
# Con script automatizado
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarCampoX" -Aplicar
```

### Ver Migraciones Actuales

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef migrations list
```

### Eliminar Ultima Migracion

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef migrations remove
```

### Limpiar y Recrear Todas las Migraciones

```powershell
.\etc\scripts\actualizar-migraciones.ps1 -Limpiar -NombreMigracion "InitialCreate" -Aplicar
```

### Ver SQL de una Migracion

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef migrations script
```

### Aplicar Migraciones a BD

```powershell
# Opcion 1: Con DbMigrator (RECOMENDADO)
cd src\cima.DbMigrator
dotnet run

# Opcion 2: Directamente con EF
cd src\cima.EntityFrameworkCore
dotnet ef database update
```

## Flujo de Trabajo Recomendado

### Escenario 1: Agregar Nueva Entidad

```powershell
# 1. Crear la entidad en src/cima.Domain/Entities/
# 2. Configurar en cimaDbContext.cs
# 3. Crear migracion
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarEntidadX"

# 4. Revisar archivos generados en Migrations/
# 5. Aplicar a BD
cd src\cima.DbMigrator
dotnet run
```

### Escenario 2: Modificar Entidad Existente

```powershell
# 1. Modificar la entidad
# 2. Actualizar configuracion en cimaDbContext.cs si es necesario
# 3. Crear migracion
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "ModificarEntidadX"

# 4. Aplicar
cd src\cima.DbMigrator
dotnet run
```

### Escenario 3: Resetear Todo (Desarrollo)

```powershell
# Elimina BD, crea nueva, aplica migraciones
.\etc\scripts\reset-database.ps1
```

## Estado Actual de Migraciones

### Migraciones Existentes

```
src/cima.EntityFrameworkCore/Migrations/
??? 20251117221607_InitialCreate.cs
??? 20251118170041_FixPhoneNullableInContactRequest.cs
??? 20251119190311_CorrigiendoPropertiesAListings.cs
??? cimaDbContextModelSnapshot.cs
```

### Entidades Configuradas

| Entidad | Tabla | Configuracion |
|---------|-------|---------------|
| `Listing` | `Listings` | Owned: `Images` |
| `Architect` | `Architects` | Indice: `UserId` (unique) |
| `ContactRequest` | `ContactRequests` | FK: `ListingId`, `ArchitectId` |

## Configuracion de Entidades

### Ejemplo: Listing

```csharp
builder.Entity<Listing>(b =>
{
    b.ToTable("Listings");
    b.HasKey(x => x.Id);
    
    // Propiedades
    b.Property(x => x.Title).IsRequired().HasMaxLength(200);
    b.Property(x => x.Description).HasMaxLength(5000);
    b.Property(x => x.Price).HasPrecision(18, 2);
    
    // Indices
    b.HasIndex(x => new { x.Status, x.ArchitectId });
    b.HasIndex(x => x.CreatedAt);
    
    // Relaciones
    b.HasOne(x => x.Architect)
        .WithMany(a => a.Listings)
        .HasForeignKey(x => x.ArchitectId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // Owned entities
    b.OwnsMany(x => x.Images, ib =>
    {
        ib.ToTable("ListingImages");
        ib.WithOwner().HasForeignKey("ListingId");
        ib.HasKey("ListingId", "ImageId");
    });
});
```

## Convenciones de Nombres

### Nombres de Migraciones

Usar PascalCase descriptivo en español:

- ? `AgregarCampoDescripcionAListing`
- ? `CrearTablaComentarios`
- ? `ModificarTipoColumnaPrecio`
- ? `AgregarIndiceEmailUsuario`
- ? `Migration1`, `Update`, `Fix`

### Ejemplos Reales

```powershell
# Agregar campo
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarCampoFechaPublicacionAListing"

# Crear tabla
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "CrearTablaComentarios"

# Modificar tipo
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "CambiarPrecioADecimal18_2"

# Agregar indice
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarIndiceEmailEnContactRequest"
```

## Troubleshooting

### Error: "No changes were detected"

```powershell
# Verificar que compilaste despues de modificar entidades
cd src\cima.EntityFrameworkCore
dotnet build

# Reintentar
dotnet ef migrations add MiMigracion
```

### Error: "Build failed"

```powershell
# Compilar todo el proyecto primero
cd C:\Users\rodri\Documents\Inmobiliaria\cima
dotnet build
```

### Error: "A migration has already been applied"

```powershell
# Ver estado actual
cd src\cima.EntityFrameworkCore
dotnet ef migrations list

# Opciones:
# 1. Eliminar ultima migracion
dotnet ef migrations remove

# 2. O resetear BD completa
cd ..\..
.\etc\scripts\reset-database.ps1
```

### Error: "Pending model changes"

```powershell
# Hay cambios en el modelo sin migrar
# Crear migracion para esos cambios
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "SincronizarCambiosModelo"
```

## Scripts Relacionados

| Script | Proposito |
|--------|-----------|
| `actualizar-migraciones.ps1` | Crear/aplicar migraciones |
| `reset-database.ps1` | Resetear BD completa |
| `ver-logs.ps1` | Ver logs de migraciones |

## Mejores Practicas

### 1. Siempre Compilar Antes

```powershell
dotnet build
# LUEGO crear migracion
```

### 2. Revisar Migracion Generada

```powershell
# Despues de crear migracion, revisar archivos en Migrations/
# Asegurate que los cambios son los esperados
```

### 3. Probar en Desarrollo Primero

```powershell
# Nunca aplicar migraciones directamente en produccion
# Siempre probar primero localmente
.\etc\scripts\reset-database.ps1
```

### 4. Backup en Produccion

```powershell
# Antes de aplicar migraciones en produccion:
# 1. Backup de BD
# 2. Probar en staging
# 3. Aplicar en produccion
# 4. Verificar
```

### 5. Commits Atomicos

```powershell
# Un commit por migracion
git add src/cima.EntityFrameworkCore/Migrations/
git commit -m "feat(domain): agregar campo Descripcion a Listing"
```

## Comandos EF Core Avanzados

### Ver SQL sin Aplicar

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef migrations script > migration.sql
notepad migration.sql
```

### Revertir a Migracion Especifica

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef database update NombreMigracionAnterior
```

### Ver Info de BD Actual

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef dbcontext info
```

### Generar Script de Migracion Especifica

```powershell
cd src\cima.EntityFrameworkCore
dotnet ef migrations script MigracionInicio MigracionFin
```

## Referencias

- EF Core Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- ABP EF Core: https://docs.abp.io/en/abp/latest/Entity-Framework-Core
- Convenciones de Commits: `docs/COMMIT_CONVENTIONS.md`
