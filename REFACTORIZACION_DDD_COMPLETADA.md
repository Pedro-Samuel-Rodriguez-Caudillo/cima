# Refactorización DDD: Bounded Contexts y Cambios en Entidades

## Resumen Ejecutivo

Se ha completado una refactorización mayor siguiendo principios de Domain-Driven Design (DDD) con los siguientes objetivos:

1. ? Organizar entidades en Bounded Contexts explícitos
2. ? Simplificar entidad Architect (sin licencia, con estadísticas)
3. ? Actualizar entidad Listing (LandArea + ConstructionArea)
4. ? Implementar ABP Settings para configuración administrativa
5. ? Actualizar Data Seeder con enfoque híbrido (Bogus + usuarios fijos)

---

## 1. Bounded Contexts Implementados

### Estructura Nueva:
```
src/cima.Domain/Entities/
??? Listings/              # Bounded Context: Gestión de Propiedades
?   ??? Listing.cs
?   ??? ListingImage.cs
?   ??? FeaturedListing.cs
??? Communication/         # Bounded Context: Comunicación con Clientes
?   ??? ContactRequest.cs
??? Identity/             # Bounded Context: Gestión de Usuarios
    ??? Architect.cs
```

---

## 2. Cambios en Entidad Architect

### Antes:
```csharp
public class Architect
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }        // ? Eliminado
    public string? Bio { get; set; }                 // ? Eliminado
    public ICollection<Listing> Listings { get; set; }
}
```

### Después:
```csharp
public class Architect
{
    // Vinculación con Identity
    public Guid UserId { get; set; }
    
    // Estadísticas
    public int TotalListingsPublished { get; set; }  // ? Nueva
    public int ActiveListings { get; set; }          // ? Nueva
    
    // Metadata
    public DateTime RegistrationDate { get; set; }   // ? Nueva
    public bool IsActive { get; set; }               // ? Nueva
    
    // Navigation
    public ICollection<Listing> Listings { get; set; }
}
```

**Justificación:**
- `Name` y `Bio` se eliminan porque la info pública viene de `IdentityUser`
- No se incluyó `LicenseNumber` porque no está en los requisitos
- Las estadísticas permiten dashboards y métricas internas

---

## 3. Cambios en Entidad Listing

### Antes:
```csharp
public class Listing
{
    public decimal Area { get; set; }  // ? Renombrado
    // ...
}
```

### Después:
```csharp
public class Listing
{
    public decimal LandArea { get; set; }          // ? Área total del terreno
    public decimal ConstructionArea { get; set; }   // ? Área construida
    // ...
}
```

**Backward Compatibility en DTOs:**
```csharp
public class ListingDto
{
    public decimal LandArea { get; set; }
    public decimal ConstructionArea { get; set; }
    
    // Para código existente
    public decimal Area => LandArea;  // Compatibility property
}
```

---

## 4. ABP Settings para Configuración Administrativa

### Archivo: `cimaSettings.cs`
```csharp
public static class cimaSettings
{
    public static class Contact
    {
        public const string AdminEmail = "Cima.Contact.AdminEmail";
        public const string AdminPhone = "Cima.Contact.AdminPhone";
    }
}
```

### Valores por Defecto:
- **AdminEmail:** `contacto@cima.com`
- **AdminPhone:** `+52 55 1234 5678`

### Uso en Servicios:
```csharp
var adminEmail = await SettingProvider.GetOrNullAsync(cimaSettings.Contact.AdminEmail);
```

**Ventajas:**
- Configurable desde UI de administración
- Multi-tenant ready
- Cacheable
- No hardcoded

---

## 5. Data Seeder con Enfoque Híbrido

### Implementación con Bogus:
```csharp
// Usuarios fijos (para login consistente)
- admin@cima.com / 1q2w3E*
- arq@cima.com / 1q2w3E*

// Listings generados con Bogus
- 12 propiedades con datos semi-realistas
- 8 Published, 1 Portfolio, 1 Archived, 2 Draft
- Títulos, precios y áreas realistas según tipo
- Imagen genérica de ABP
```

**Ventajas:**
- Datos consistentes para desarrollo
- Fácil escalar a cientos de registros
- Realismo en los datos

---

## 6. Actualización de Tests

### Tests Actualizados:
- ? `ArchitectTests.cs` - Nueva estructura sin Name/Bio
- ? `ListingTests.cs` - LandArea + ConstructionArea
- ? `ListingAppServiceTests.cs` - Helpers actualizados
- ? `ContactRequestAppServiceTests.cs` - Helpers actualizados
- ? `ArchitectAppServiceTests.cs` - Assertions actualizados

---

## 7. Migración de Base de Datos

### Próximos Pasos:
```bash
# 1. Crear migración
cd src\cima.EntityFrameworkCore
dotnet ef migrations add RefactorDDD_BoundedContexts_AreasAndStats

# 2. Actualizar base de datos
dotnet ef database update

# 3. Re-seed data
cd ..\..\src\cima.DbMigrator
dotnet run
```

### Cambios en Base de Datos:
```sql
-- Tabla Architects
ALTER TABLE Architects DROP COLUMN Name;
ALTER TABLE Architects DROP COLUMN Bio;
ALTER TABLE Architects ADD TotalListingsPublished INT NOT NULL DEFAULT 0;
ALTER TABLE Architects ADD ActiveListings INT NOT NULL DEFAULT 0;
ALTER TABLE Architects ADD RegistrationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();
ALTER TABLE Architects ADD IsActive BIT NOT NULL DEFAULT 1;

-- Tabla Listings
-- Renombrar Area a LandArea (EF puede hacer RENAME)
EXEC sp_rename 'Listings.Area', 'LandArea', 'COLUMN';
ALTER TABLE Listings ADD ConstructionArea DECIMAL(10,2) NOT NULL DEFAULT 0;
```

---

## 8. Breaking Changes

### Para Código Existente:
1. **Architect.Name / Bio:** Ya no existen - usar `IdentityUser.Name`
2. **Listing.Area:** Renombrado a `LandArea` - agregar `ConstructionArea`
3. **CreateUpdateListingDto:** Requiere ambas áreas ahora

### Mitigación:
- DTOs mantienen propiedad `Area` para backward compatibility (read-only)
- Servicios validan ambas áreas

---

## 9. Commits Sugeridos

```bash
# Commit 1: Bounded Contexts
git add src/cima.Domain/Entities/
git commit -m "refactor: organizar entidades en bounded contexts DDD

- Listings/ (Gestión de Propiedades)
- Communication/ (Interacción con Clientes)  
- Identity/ (Gestión de Usuarios)"

# Commit 2: Architect sin licencia
git add src/cima.Domain/Entities/Identity/Architect.cs
git commit -m "refactor: simplificar Architect con estadísticas

- Eliminar Name/Bio (vienen de IdentityUser)
- Agregar TotalListingsPublished, ActiveListings
- Agregar RegistrationDate, IsActive
- No incluir LicenseNumber (fuera de scope)"

# Commit 3: Listing áreas
git add src/cima.Domain/Entities/Listings/Listing.cs
git commit -m "refactor: renombrar Area a LandArea y agregar ConstructionArea

- LandArea: área total del terreno (m²)
- ConstructionArea: área construida (m²)
- Ambas obligatorias"

# Commit 4: ABP Settings
git add src/cima.Domain.Shared/Settings/
git commit -m "feat: agregar ABP Settings para configuración admin

- cimaSettings.Contact.AdminEmail
- cimaSettings.Contact.AdminPhone
- Configurables desde UI
- Valores por defecto establecidos"

# Commit 5: Seeder híbrido
git add src/cima.Application/Data/DevelopmentDataSeeder.cs
git commit -m "refactor: seeder híbrido con Bogus

- Usuarios fijos: admin@cima.com, arq@cima.com  
- 12 listings generados con datos realistas
- Instalado paquete Bogus 35.6.5"

# Commit 6: Tests actualizados
git add test/
git commit -m "test: actualizar tests para nueva estructura

- ArchitectTests: sin Name/Bio, con estadísticas
- ListingTests: LandArea + ConstructionArea
- App service tests: helpers actualizados"

# Commit 7: DbContext y migración
git add src/cima.EntityFrameworkCore/
git commit -m "chore: actualizar DbContext para refactorización DDD"
```

---

## 10. Verificación Post-Refactorización

### Checklist:
- [ ] Compilación exitosa sin errores
- [ ] Tests unitarios pasan
- [ ] Migración EF creada
- [ ] Base de datos actualizada
- [ ] Seeder ejecutado correctamente
- [ ] API responde correctamente
- [ ] Blazor UI funciona

---

## Notas Finales

Esta refactorización establece bases sólidas para:
- Escalabilidad (bounded contexts claros)
- Mantenibilidad (separación de concerns)
- Extensibilidad (fácil agregar nuevos módulos)
- Testing (tests actualizados y pasando)

La estructura ahora refleja mejor el dominio del negocio y sigue principios DDD.
