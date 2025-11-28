# ?? PRÓXIMOS PASOS - POST CRUD LISTINGS

**Estado actual:** ? CRUD Listings completado al 100%  
**Compilación:** ? Exitosa  
**Siguiente fase:** Completar panel de administración

---

## ?? ACCIONES INMEDIATAS (ALTA PRIORIDAD)

### 1. Implementar GetListAsync en ArchitectAppService
**Problema:** Create/Edit no pueden cargar lista de arquitectos

**Acción:**
```csharp
// src/cima.Application.Contracts/Architects/IArchitectAppService.cs
Task<PagedResultDto<ArchitectDto>> GetListAsync(PagedAndSortedResultRequestDto input);

// src/cima.Application/Architects/ArchitectAppService.cs
public async Task<PagedResultDto<ArchitectDto>> GetListAsync(PagedAndSortedResultRequestDto input)
{
    var query = await Repository.GetQueryableAsync();
    var totalCount = await AsyncExecuter.CountAsync(query);
    
    query = query
        .Skip(input.SkipCount)
        .Take(input.MaxResultCount);
    
    var items = await AsyncExecuter.ToListAsync(query);
    var dtos = ObjectMapper.Map<List<Architect>, List<ArchitectDto>>(items);
    
    return new PagedResultDto<ArchitectDto>(totalCount, dtos);
}
```

**Actualizar en páginas:**
```csharp
// Create.razor y Edit.razor
private async Task LoadArchitects()
{
    var result = await ArchitectAppService.GetListAsync(
        new PagedAndSortedResultRequestDto { MaxResultCount = 100 }
    );
    architects = result.Items.ToList();
}
```

**Tiempo estimado:** 30 minutos

---

### 2. Implementar Upload de Imágenes
**Problema:** No se pueden publicar listings sin imágenes

**Opciones:**

#### Opción A: Componente Simple (Rápido)
```razor
<!-- ImageUploader.razor -->
<InputFile OnChange="HandleFileSelected" multiple accept="image/*" />

@code {
    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles(10))
        {
            // Subir a wwwroot/images/listings/
            var path = $"images/listings/{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
            await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            // Guardar archivo...
        }
    }
}
```

#### Opción B: Servicio de Almacenamiento (Producción)
- Azure Blob Storage
- AWS S3
- Cloudinary

**Crear servicio:**
```csharp
public interface IImageStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
    Task DeleteImageAsync(string imageUrl);
}
```

**Tiempo estimado:** 2-4 horas (según opción)

---

## ?? SIGUIENTE DÍA (MEDIA PRIORIDAD)

### 3. Páginas Admin Restantes

#### Dashboard (/admin/dashboard)
**Características:**
- KPIs: Total listings, por estado, arquitectos activos
- Gráfico de listings por mes
- Tabla de solicitudes recientes
- Top arquitectos por propiedades

**Archivos a crear:**
- `Pages/Admin/Dashboard/Index.razor`
- Usar `IStatisticsAppService` (ya implementado)

**Tiempo estimado:** 2 horas

---

#### ContactRequests (/admin/contact-requests)
**Características:**
- Listado paginado con filtros (estado, fecha)
- Ver detalle de solicitud
- Marcar como respondida/cerrada
- Filtrar por propiedad o arquitecto

**Archivos a crear:**
- `Pages/Admin/ContactRequests/Index.razor`
- `Pages/Admin/ContactRequests/Detail.razor`

**Tiempo estimado:** 1.5 horas

---

#### Architects (/admin/architects)
**Características:**
- Listado de arquitectos
- Editar perfil (bio, portfolio URL)
- Ver propiedades asociadas
- Estadísticas por arquitecto

**Archivos a crear:**
- `Pages/Admin/Architects/Index.razor`
- `Pages/Admin/Architects/Edit.razor`

**Tiempo estimado:** 1.5 horas

---

### 4. Navegación y Layout Admin

**Crear menú lateral:**
```razor
<!-- Components/Admin/AdminSidebar.razor -->
<Nav>
    <NavLink href="/admin/dashboard">
        <Icon Name="IconName.Dashboard" /> Dashboard
    </NavLink>
    <NavLink href="/admin/listings">
        <Icon Name="IconName.Home" /> Propiedades
    </NavLink>
    <NavLink href="/admin/architects">
        <Icon Name="IconName.User" /> Arquitectos
    </NavLink>
    <NavLink href="/admin/contact-requests">
        <Icon Name="IconName.Envelope" /> Solicitudes
    </NavLink>
</Nav>
```

**Layout específico:**
```razor
<!-- Layouts/AdminLayout.razor -->
@inherits LayoutComponentBase

<div class="admin-layout">
    <AdminSidebar />
    <main class="admin-content">
        @Body
    </main>
</div>
```

**Tiempo estimado:** 1 hora

---

## ?? MEJORAS FUTURAS (BAJA PRIORIDAD)

### 5. Validaciones FluentValidation Backend
```csharp
public class CreateUpdateListingDtoValidator : AbstractValidator<CreateUpdateListingDto>
{
    public CreateUpdateListingDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor a 0");
        
        // etc...
    }
}
```

**Tiempo estimado:** 1 hora

---

### 6. Auto-save en Edit
```csharp
private Timer autoSaveTimer;

protected override void OnInitialized()
{
    autoSaveTimer = new Timer(AutoSave, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
}

private async void AutoSave(object state)
{
    if (hasChanges)
    {
        await SaveListing(showMessage: false);
    }
}
```

**Tiempo estimado:** 45 minutos

---

### 7. Búsqueda Avanzada
- Búsqueda por múltiples campos
- Filtros guardados (favoritos)
- Exportar resultados a Excel/PDF
- Búsqueda geográfica (mapa)

**Tiempo estimado:** 4+ horas

---

### 8. Historial de Cambios (Auditoría)
```csharp
public interface IAuditLogAppService
{
    Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, Guid entityId);
}
```

Mostrar en Edit:
- Quién creó
- Quién modificó y cuándo
- Cambios de estado
- Historial de precios

**Tiempo estimado:** 3 horas

---

## ?? ROADMAP SUGERIDO

### Esta Semana (Días 5-7)
```
Día 5 ? CRUD Listings (COMPLETADO)
Día 6 ?? GetListAsync Architects + Upload Imágenes
Día 7 ?? Dashboard + ContactRequests + Architects
```

### Próxima Semana (Días 8-10)
```
Día 8: Navegación Admin + Testing Manual
Día 9: FluentValidation + Auto-save
Día 10: Polish + Bug fixes + Deploy
```

---

## ?? TESTING PENDIENTE

### Unit Tests
```csharp
// test/cima.Application.Tests/Listings/ListingAppServiceTests.cs
[Fact]
public async Task Should_Create_Listing_In_Draft_Status()
{
    // Arrange
    var input = new CreateUpdateListingDto { ... };
    
    // Act
    var result = await _listingAppService.CreateAsync(input);
    
    // Assert
    result.Status.ShouldBe(ListingStatus.Draft);
}
```

### Integration Tests
```csharp
// test/cima.EntityFrameworkCore.Tests/Listings/ListingRepositoryTests.cs
[Fact]
public async Task Should_Get_Published_Listings_Only()
{
    // Test repository queries
}
```

### E2E Tests (Playwright)
```javascript
test('should create listing successfully', async ({ page }) => {
  await page.goto('/admin/listings/create');
  await page.fill('[name="Title"]', 'Test Property');
  // ...
  await page.click('button:has-text("Guardar")');
  await expect(page).toHaveURL(/\/edit\//);
});
```

---

## ?? DOCUMENTACIÓN PENDIENTE

### Para Desarrolladores
- [ ] API documentation (Swagger descriptions)
- [ ] Architecture decision records (ADR)
- [ ] Database schema diagram
- [ ] Component library (Storybook?)

### Para Usuarios
- [ ] Manual de administrador
- [ ] Guía de usuario final
- [ ] FAQs
- [ ] Video tutoriales

---

## ?? CONFIGURACIÓN PENDIENTE

### Desarrollo
- [ ] Docker Compose para desarrollo local
- [ ] Seed data para testing
- [ ] Scripts de reset database

### Producción
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Logging centralizado (Serilog + Seq)
- [ ] Monitoring (Application Insights)
- [ ] Backups automáticos BD

---

## ?? OBJETIVOS SEMANA

| Objetivo | Prioridad | Estado |
|----------|-----------|--------|
| CRUD Listings | P0 | ? Completado |
| Upload Imágenes | P0 | ? Pendiente |
| GetListAsync Architects | P0 | ? Pendiente |
| Dashboard Admin | P1 | ?? Planificado |
| ContactRequests CRUD | P1 | ?? Planificado |
| Architects CRUD | P1 | ?? Planificado |
| Layout Admin | P1 | ?? Planificado |
| Testing Manual | P1 | ?? Planificado |
| FluentValidation | P2 | ?? Futuro |
| Auto-save | P2 | ?? Futuro |

---

## ?? CONSEJOS

### Performance
- Usar `IQueryable` en AppServices para consultas eficientes
- Implementar caché para datos estáticos (arquitectos, etc)
- Lazy loading de imágenes en galerías

### Seguridad
- Validar siempre en servidor (no confiar en cliente)
- Sanitizar inputs para prevenir XSS
- Rate limiting en endpoints de upload

### UX
- Loading states en todas las operaciones async
- Confirmaciones para acciones destructivas
- Mensajes de feedback claros y descriptivos
- Breadcrumbs para navegación

### Mantenibilidad
- Componentes reutilizables en `Components/`
- Servicios compartidos en `Services/`
- Constantes en archivos separados
- DTOs versionados si es API pública

---

## ?? SOPORTE

### Documentación del Proyecto
- `docs/DIA_5_CRUD_LISTINGS_COMPLETADO.md` - Detalle completo
- `docs/DIA_5_RESUMEN_RAPIDO.md` - Resumen ejecutivo
- `docs/DIA_5_CHECKLIST_VERIFICACION.md` - Checklist QA

### Referencias Externas
- [ABP Framework Docs](https://docs.abp.io)
- [Blazorise Components](https://blazorise.com/docs)
- [.NET 9 Docs](https://docs.microsoft.com/dotnet)

---

**Actualizado:** Sesión actual  
**Próxima revisión:** Inicio Día 6
