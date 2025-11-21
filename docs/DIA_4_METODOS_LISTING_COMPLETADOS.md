# Día 4: Métodos Especiales de Listing - COMPLETADO

## Fecha de Implementación
**Fecha**: [Actualiza con la fecha actual]

---

## ? Objetivos Completados

### 1. Métodos Especiales de ListingAppService
- ? `PublishAsync(Guid id)` - Publicar propiedad de Draft ? Published
- ? `ArchiveAsync(Guid id)` - Archivar propiedad
- ? `UnarchiveAsync(Guid id)` - Restaurar propiedad archivada ? Published
- ? `UnpublishAsync(Guid id)` - Revertir publicación ? Draft
- ? `GetPublishedAsync(GetListingsInput)` - Obtener solo propiedades publicadas (público)
- ? `GetByArchitectAsync(Guid, int, int)` - Obtener propiedades de un arquitecto

### 2. Mejoras en Filtros
- ? Filtro por `PropertyType` (House, Apartment, Commercial, Land, Office)
- ? Filtro por `TransactionType` (Sale, Rent, Lease)
- ? Método helper `ApplySorting()` para reutilización de lógica

### 3. Validaciones de Negocio
- ? Solo el propietario o admin puede modificar propiedades
- ? Validación de estados al publicar/archivar
- ? Warning en logs si se publica sin imágenes
- ? Verificación de permisos en cada operación

---

## ?? Métodos Implementados

### Gestión de Estados

#### 1. PublishAsync
```csharp
[Authorize(cimaPermissions.Listings.Publish)]
public async Task<ListingDto> PublishAsync(Guid id)
```
**Funcionalidad**:
- Cambia estado de `Draft` ? `Published`
- Valida que solo el propietario o admin puede publicar
- Registra warning si no tiene imágenes (pero permite publicar)
- Previene re-publicación si ya está publicada

**Validaciones**:
- ? Usuario es dueño o admin
- ? Estado actual no es `Published`
- ?? Warning (no bloquea) si no tiene imágenes

---

#### 2. ArchiveAsync
```csharp
[Authorize(cimaPermissions.Listings.Archive)]
public async Task<ListingDto> ArchiveAsync(Guid id)
```
**Funcionalidad**:
- Cambia estado a `Archived`
- Oculta la propiedad de listados públicos
- Mantiene datos históricos

**Uso**:
- Propiedades vendidas
- Propiedades temporalmente fuera del mercado

---

#### 3. UnarchiveAsync (NUEVO)
```csharp
[Authorize(cimaPermissions.Listings.Publish)]
public async Task<ListingDto> UnarchiveAsync(Guid id)
```
**Funcionalidad**:
- Reactiva propiedad archivada ? `Published`
- Valida que está en estado `Archived` antes de restaurar

**Caso de Uso**:
- Propiedad vuelve al mercado
- Transacción cancelada

---

#### 4. UnpublishAsync (NUEVO)
```csharp
[Authorize(cimaPermissions.Listings.Edit)]
public async Task<ListingDto> UnpublishAsync(Guid id)
```
**Funcionalidad**:
- Revierte publicación: `Published` ? `Draft`
- Útil para ediciones masivas sin afectar catálogo público

**Caso de Uso**:
- Correcciones extensas
- Actualización de imágenes

---

### Consultas Públicas

#### 5. GetPublishedAsync (NUEVO)
```csharp
[AllowAnonymous]
public async Task<PagedResultDto<ListingDto>> GetPublishedAsync(GetListingsInput input)
```
**Funcionalidad**:
- **Sin autenticación** (público)
- Solo retorna propiedades con `Status = Published`
- Filtros completos: precio, ubicación, tipo, transacción
- Paginación y ordenamiento

**Filtros Disponibles**:
- `SearchTerm`: Búsqueda en título, descripción, ubicación
- `MinPrice` / `MaxPrice`: Rango de precios
- `MinBedrooms` / `MinBathrooms`: Mínimos de habitaciones/baños
- `PropertyType`: Tipo de propiedad (0-4)
- `TransactionType`: Tipo de transacción (0-2)
- `Sorting`: Ordenamiento personalizado

**Opciones de Ordenamiento**:
- `price` / `pricedesc` ? Precio ascendente/descendente
- `area` / `areadesc` ? Área ascendente/descendente
- `createdat` / `createdat desc` ? Fecha de creación
- `title` / `title desc` ? Título alfabético

---

#### 6. GetByArchitectAsync
```csharp
public async Task<PagedResultDto<ListingDto>> GetByArchitectAsync(
    Guid architectId, int skipCount, int maxResultCount)
```
**Funcionalidad**:
- Obtiene todas las propiedades de un arquitecto específico
- Ordenadas por fecha de creación (más recientes primero)
- Paginación configurable

**Uso**:
- Portafolio de arquitecto
- Dashboard del arquitecto

---

## ?? Mejoras Técnicas

### Método Helper: ApplySorting
```csharp
private IQueryable<Listing> ApplySorting(IQueryable<Listing> queryable, string? sorting)
{
    return sorting?.ToLower() switch
    {
        "price" => queryable.OrderBy(p => p.Price),
        "pricedesc" => queryable.OrderByDescending(p => p.Price),
        "area" => queryable.OrderBy(p => p.Area),
        "areadesc" => queryable.OrderByDescending(p => p.Area),
        "createdat" => queryable.OrderBy(p => p.CreatedAt),
        "createdat desc" => queryable.OrderByDescending(p => p.CreatedAt),
        "title" => queryable.OrderBy(p => p.Title),
        "title desc" => queryable.OrderByDescending(p => p.Title),
        _ => queryable.OrderByDescending(p => p.CreatedAt)
    };
}
```

**Beneficios**:
- ? Reutilización en múltiples métodos
- ? Código DRY (Don't Repeat Yourself)
- ? Fácil mantenimiento
- ? Ordenamiento por defecto: más recientes primero

---

## ?? Diagrama de Flujo de Estados

```
???????????
?  DRAFT  ? ??????????????????
???????????                  ?
     ? PublishAsync()        ? UnpublishAsync()
     ?                       ?
?????????????                ?
? PUBLISHED ? ????????????????
?????????????
      ? ArchiveAsync()
      ?
????????????
? ARCHIVED ?
????????????
      ? UnarchiveAsync()
      ?
      ??????? PUBLISHED
```

---

## ?? Permisos Requeridos

| Método | Permiso | Validación Adicional |
|--------|---------|---------------------|
| `PublishAsync` | `cimaPermissions.Listings.Publish` | Solo dueño o admin |
| `ArchiveAsync` | `cimaPermissions.Listings.Archive` | Solo dueño o admin |
| `UnarchiveAsync` | `cimaPermissions.Listings.Publish` | Solo dueño o admin |
| `UnpublishAsync` | `cimaPermissions.Listings.Edit` | Solo dueño o admin |
| `GetPublishedAsync` | `[AllowAnonymous]` | Ninguna |
| `GetByArchitectAsync` | `cimaPermissions.Listings.Default` | Ninguna |

---

## ?? Cambios en la Interfaz

### Antes:
```csharp
public interface IListingAppService : IApplicationService
{
    Task<PagedResultDto<ListingDto>> GetListAsync(GetListingsInput input);
    Task<ListingDto> GetAsync(Guid id);
    Task<ListingDto> CreateAsync(CreateUpdateListingDto input);
    Task<ListingDto> UpdateAsync(Guid id, CreateUpdateListingDto input);
    Task DeleteAsync(Guid id);
    Task<ListingDto> PublishAsync(Guid id);
    Task<ListingDto> ArchiveAsync(Guid id);
}
```

### Después:
```csharp
public interface IListingAppService : IApplicationService
{
    // Métodos CRUD existentes
    Task<PagedResultDto<ListingDto>> GetListAsync(GetListingsInput input);
    Task<ListingDto> GetAsync(Guid id);
    Task<ListingDto> CreateAsync(CreateUpdateListingDto input);
    Task<ListingDto> UpdateAsync(Guid id, CreateUpdateListingDto input);
    Task DeleteAsync(Guid id);
    
    // Métodos de gestión de estado
    Task<ListingDto> PublishAsync(Guid id);
    Task<ListingDto> ArchiveAsync(Guid id);
    Task<ListingDto> UnarchiveAsync(Guid id);        // ? NUEVO
    Task<ListingDto> UnpublishAsync(Guid id);        // ? NUEVO
    
    // Consultas especializadas
    Task<PagedResultDto<ListingDto>> GetPublishedAsync(GetListingsInput input); // ? NUEVO
    Task<PagedResultDto<ListingDto>> GetByArchitectAsync(
        Guid architectId, int skipCount, int maxResultCount);
}
```

---

## ?? Nuevos Filtros en GetListingsInput

### Antes:
```csharp
public class GetListingsInput : PagedAndSortedResultRequestDto
{
    public string? SearchTerm { get; set; }
    public int? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public Guid? ArchitectId { get; set; }
}
```

### Después:
```csharp
public class GetListingsInput : PagedAndSortedResultRequestDto
{
    public string? SearchTerm { get; set; }
    public int? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public Guid? ArchitectId { get; set; }
    public int? PropertyType { get; set; }        // ? NUEVO
    public int? TransactionType { get; set; }     // ? NUEVO
}
```

---

## ?? Ejemplos de Uso

### 1. Publicar una propiedad
```http
POST /api/app/listing/{id}/publish
Authorization: Bearer {token}
```

**Respuesta exitosa**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": 1,  // Published
  "lastModifiedAt": "2024-01-15T10:30:00Z",
  "lastModifiedBy": "user-guid"
}
```

---

### 2. Obtener propiedades publicadas (público)
```http
GET /api/app/listing/published?
    SearchTerm=casa&
    MinPrice=100000&
    MaxPrice=500000&
    PropertyType=0&
    TransactionType=0&
    Sorting=pricedesc&
    SkipCount=0&
    MaxResultCount=10
```

**Sin token** (público)

---

### 3. Desarchivar propiedad
```http
POST /api/app/listing/{id}/unarchive
Authorization: Bearer {token}
```

---

### 4. Portafolio de arquitecto
```http
GET /api/app/listing/by-architect/{architectId}?
    skipCount=0&
    maxResultCount=20
```

---

##  Próximos Pasos (Día 5)

1. **Configurar CORS mejorado**
   - Permitir orígenes específicos
   - Configurar headers permitidos
   
2. **Mejorar Swagger**
   - Agregar ejemplos de request/response
   - Documentar códigos de error
   
3. **Implementar FluentValidation**
   - Validadores para CreateUpdateListingDto
   - Validadores para filtros
   
4. **Rate Limiting**
   - Limitar requests públicos
   - Proteger endpoints sensibles

5. **Tests Unitarios**
   - Tests para métodos de estado
   - Tests para filtros
   - Tests para validaciones

---

## ?? Estadísticas de Implementación

- **Nuevos métodos**: 4 (Unarchive, Unpublish, GetPublished, ApplySorting)
- **Métodos mejorados**: 3 (Publish, Archive, GetList)
- **Nuevos filtros**: 2 (PropertyType, TransactionType)
- **Líneas de código agregadas**: ~150
- **Tiempo de compilación**: 6.3s
- **Errores corregidos**: 3 (ILogger import, GetByArchitect signature, duplicate sorting)

---

## ? Checklist Final

- [x] Métodos de estado implementados (Publish, Archive, Unarchive, Unpublish)
- [x] Consulta pública GetPublishedAsync
- [x] Filtros por PropertyType y TransactionType
- [x] Método helper ApplySorting
- [x] Validaciones de permisos
- [x] Compilación exitosa
- [x] Logging configurado
- [x] Interfaz actualizada
- [x] DTOs actualizados
- [ ] Tests unitarios (Día 5)
- [ ] Documentación Swagger (Día 5)
- [ ] FluentValidation (Día 5)

---

## ?? Estado del Proyecto

### Completado (Días 1-4)
- ? Setup & Infraestructura (Día 1)
- ? EF Core, Migraciones, Seeders (Día 2)
- ? Application Services, Validación (Día 3)
- ? ArchitectAppService completo (Día 4)
- ? StatisticsAppService completo (Día 4)
- ? **ListingAppService con métodos especiales (Día 4)** ? TÚ ESTÁS AQUÍ

### Pendiente (Semana 2)
- ?? API REST, Swagger, CORS (Día 5)
- ?? Auth, OpenIddict, Seguridad (Día 5)
- ?? Catálogo público UI (Día 6)
- ?? Admin CRUD, Upload imágenes (Día 7)

---

**Última actualización**: [Fecha]  
**Compilación**: ? Exitosa  
**Tests**: ?? Pendiente Día 5  
**Documentación**: ? Completa
