# ? REFACTORIZACIÓN COMPLETADA - RESUMEN Y PENDIENTES

## ?? **ESTADO ACTUAL**

### ? **COMPLETADO:**
1. ? Entidades del Dominio actualizadas
2. ? Tests del Dominio corregidos y compilando
3. ? DbContext actualizado
4. ? Migración EF Core creada: `Refactorizar_Entidades_Nullability_ListaEnlazada`
5. ? DTOs básicos actualizados (`FeaturedListingDto`)
6. ? Interfaces actualizadas (`IFeaturedListingAppService`)

### ?? **PARCIALMENTE COMPLETADO:**
- ?? `FeaturedListingAppService` - Actualizado pero faltan algunos ajustes
- ?? `ListingAppService` - Métodos de imágenes comentados temporalmente

### ?? **PENDIENTE (9 errores de compilación):**

---

## ?? **ERRORES PENDIENTES POR CORREGIR**

### 1. **ArchitectAppService.cs** (4 errores)
**Archivo:** `src/cima.Application/Architects/ArchitectAppService.cs`

#### Línea 156 - Falta inicializar `Name`:
```csharp
// ? ANTES
var architect = new Architect();

// ? CORRECCIÓN
var architect = new Architect 
{ 
    Name = input.Name  // Agregar
};
```

#### Línea 160 - Eliminar `PortfolioUrl`:
```csharp
// ? ANTES
architect.PortfolioUrl = input.PortfolioUrl;

// ? CORRECCIÓN
// Eliminar esta línea - PortfolioUrl ya no existe
```

#### Línea 194 - Eliminar `PortfolioUrl`:
```csharp
// ? ANTES
architect.PortfolioUrl = input.PortfolioUrl;

// ? CORRECCIÓN
// Eliminar esta línea - PortfolioUrl ya no existe
```

---

### 2. **DevelopmentDataSeeder.cs** (3 errores)

**Archivo:** `src/cima.Application/Data/DevelopmentDataSeeder.cs`

#### Línea 79 - Agregar `Name` al arquitecto:
```csharp
// ? ANTES
var architect = new Architect
{
    UserId = adminUserId,
    Bio = "Arquitecto principal de CIMA...",
    PortfolioUrl = "https://cima.mx/arquitectos/admin"
};

// ? CORRECCIÓN
var architect = new Architect
{
    UserId = adminUserId,
    Name = "Arquitecto CIMA",  // ? Agregar
    Bio = "Arquitecto principal de CIMA con más de 15 años de experiencia..."
    // PortfolioUrl eliminado - portafolio es interno
};
```

#### Línea 297 - Actualizar `ListingImage` para lista enlazada:
```csharp
// ? ANTES
new ListingImage(
    imageId: Guid.NewGuid(),
    url: $"https://picsum.photos/800/600?random={i}",
    displayOrder: i + 1,  // ? Ya no existe
    altText: $"Imagen {i + 1} de {listing.Title}",
    fileSize: 1024000,
    contentType: "image/jpeg"
)

// ? CORRECCIÓN
new ListingImage(
    imageId: Guid.NewGuid(),
    url: $"https://picsum.photos/800/600?random={i}",
    altText: $"Imagen {i + 1} de {listing.Title}",
    fileSize: 1024000,
    contentType: "image/jpeg",
    previousImageId: i > 0 ? imageIds[i - 1] : null,  // ? Lista enlazada
    nextImageId: null  // Se actualizará con la siguiente
)
```

#### Línea 321 - Actualizar `FeaturedListing`:
```csharp
// ? ANTES
new FeaturedListing(
    listingId: listing.Id,
    displayOrder: i + 1,  // ? Ya no existe
    createdBy: adminUserId
)

// ? CORRECCIÓN
new FeaturedListing(
    listingId: listing.Id,
    createdBy: adminUserId
)
```

---

### 3. **cimaApplicationAutoMapperProfile.cs** (1 error)

**Archivo:** `src/cima.Application/cimaApplicationAutoMapperProfile.cs`

#### Línea 20 - Actualizar mapeo de `ListingImage`:
```csharp
// ? ANTES
CreateMap<ListingImage, ListingImageDto>()
    .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder));

// ? CORRECCIÓN
CreateMap<ListingImage, ListingImageDto>()
    .ForMember(dest => dest.PreviousImageId, opt => opt.MapFrom(src => src.PreviousImageId))
    .ForMember(dest => dest.NextImageId, opt => opt.MapFrom(src => src.NextImageId));
```

---

### 4. **FeaturedListingClientProxy.Generated.cs** (1 error)

**Archivo:** `src/cima.HttpApi.Client/ClientProxies/cima/Listings/FeaturedListingClientProxy.Generated.cs`

Este archivo se **GENERA AUTOMÁTICAMENTE**. Se corregirá solo después de:
1. Eliminar el archivo actual
2. Ejecutar: `dotnet build`
3. ABP regenerará el proxy sin `UpdateOrderAsync`

---

## ?? **INSTRUCCIONES DETALLADAS DE CORRECCIÓN**

### **Paso 1: Actualizar ArchitectAppService**

<function_calls>
<invoke name="file_search">
<parameter name="maxResults">2