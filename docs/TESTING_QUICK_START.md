# Quick Start - Testing

## ?? Ejecutar Tests Inmediatamente

### 1. Tests Unitarios (Rápido - 30 segundos)
```bash
# Domain + Application + UI Components
.\etc\scripts\run-all-tests.ps1 -SkipE2E
```

### 2. Tests E2E (Requiere app corriendo - 5 minutos)
```bash
# Terminal 1: Iniciar la aplicación
dotnet run --project src/cima.Blazor/cima.Blazor.csproj

# Terminal 2: Instalar Playwright (solo primera vez)
dotnet build test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj
pwsh test/cima.Blazor.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium

# Terminal 2: Ejecutar E2E tests
dotnet test test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj
```

### 3. Performance Audit (Requiere npm - 2 minutos)
```bash
# Solo primera vez
npm install -g lighthouse

# Con la app corriendo
.\etc\scripts\run-performance-audit.ps1
```

---

## ?? Tests Implementados

### ? Domain Tests (Existentes)
- **15+ tests** de entidades y lógica de negocio
- Cobertura: 80%+

### ? Application Tests (Existentes)
- **15+ tests** de servicios de aplicación
- Cobertura: 75%+

### ? UI Component Tests (NUEVO)
- **18 tests** con bUnit
- Componentes: ListingCard, HeroSection, ImageGallery, ContactForm, PropertySearchFilters

### ? E2E Tests (NUEVO)
- **20 tests** con Playwright
- Sitio público: Home, Properties, Portfolio, Navigation, Mobile
- Admin panel: Authentication, Dashboard, Listings

### ? Performance Tests (NUEVO)
- Lighthouse audit script
- Métricas: Performance, Accessibility, Best Practices, SEO

---

## ?? Tests por Categoría

### Tests de Componentes UI (bUnit)
```bash
dotnet test test/cima.Blazor.UITests/cima.Blazor.UITests.csproj
```

**Tests incluidos:**
- ? ListingCard renderiza correctamente
- ? ListingCard muestra precio
- ? ListingCard muestra detalles (bedrooms, bathrooms, area)
- ? ListingCard maneja listing nulo
- ? HeroSection renderiza
- ? HeroSection tiene call-to-action
- ? HeroSection tiene heading principal
- ? ImageGallery renderiza imágenes
- ? ImageGallery ordena imágenes
- ? ImageGallery maneja lista vacía
- ? ImageGallery muestra botones de navegación
- ? ContactForm renderiza
- ? ContactForm tiene campos requeridos
- ? ContactForm tiene botón submit
- ? ContactForm valida campos vacíos
- ? PropertySearchFilters renderiza
- ? PropertySearchFilters tiene inputs de filtro
- ? PropertySearchFilters tiene botón de búsqueda

### Tests E2E Sitio Público
```bash
# Asegurate que la app esté corriendo primero
dotnet test test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj --filter "FullyQualifiedName~PublicSiteTests"
```

**Tests incluidos:**
- ? Home page carga exitosamente
- ? Home page muestra hero section
- ? Home page muestra propiedades destacadas
- ? Properties page carga
- ? Properties page muestra filtros de búsqueda
- ? Properties page filtra por tipo
- ? Property detail carga al hacer click
- ? Property detail muestra galería de imágenes
- ? Property detail muestra formulario de contacto
- ? Contact form valida campos requeridos
- ? Portfolio page carga
- ? Portfolio page muestra proyectos
- ? Navegación funciona entre páginas
- ? Language switch cambia idioma
- ? Mobile view se muestra correctamente

### Tests E2E Admin Panel
```bash
dotnet test test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj --filter "FullyQualifiedName~AdminPanelTests"
```

**Tests incluidos:**
- ? Admin dashboard requiere autenticación
- ? Admin dashboard carga después de login
- ? Admin listings muestra grid
- ? Admin listings tiene botón create
- ? Admin dashboard muestra estadísticas

---

## ?? Estructura de Archivos

```
test/
??? cima.Blazor.E2ETests/
?   ??? PublicSiteTests.cs      (15 tests E2E)
?   ??? AdminPanelTests.cs      (5 tests E2E)
?   ??? PlaywrightTestBase.cs  (Base class helper)
?   ??? playwright.config.json  (Config)
?
??? cima.Blazor.UITests/
?   ??? Components/
?       ??? ListingCardTests.cs           (4 tests)
?       ??? HeroSectionTests.cs           (3 tests)
?       ??? ImageGalleryTests.cs          (4 tests)
?       ??? ContactFormTests.cs           (4 tests)
?       ??? PropertySearchFiltersTests.cs (3 tests)
?
etc/scripts/
??? run-all-tests.ps1            (Ejecutar todos los tests)
??? run-performance-audit.ps1    (Lighthouse audit)

docs/
??? TESTING_STRATEGY.md          (Guía completa)
??? DIA_12_TESTING_COMPLETADO.md (Quick checklist)
```

---

## ?? Troubleshooting Rápido

### Error: "Playwright not found"
```bash
dotnet build test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj
pwsh test/cima.Blazor.E2ETests/bin/Debug/net10.0/playwright.ps1 install chromium
```

### Error: "Connection refused" en E2E tests
**Solución:** La app debe estar corriendo en http://localhost:5000
```bash
dotnet run --project src/cima.Blazor/cima.Blazor.csproj
```

### Error: "Lighthouse not found"
```bash
npm install -g lighthouse
```

### Error: Tests fallan por timeout
**Solución:** Aumentar timeout en `playwright.config.json`:
```json
{
  "timeout": 60000  // 60 segundos
}
```

---

## ?? Verificación Rápida

### ¿Todo funciona?
```bash
# 1. Compilar proyectos de test
dotnet build test/cima.Blazor.UITests/cima.Blazor.UITests.csproj
dotnet build test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj

# 2. Ejecutar tests sin E2E (no requiere app corriendo)
.\etc\scripts\run-all-tests.ps1 -SkipE2E

# Si todo pasa ? - Implementación exitosa!
```

---

## ?? Próximos Pasos

### Para CI/CD
1. Agregar tests a GitHub Actions workflow
2. Configurar Playwright en pipeline
3. Generar reportes de cobertura

### Para mejorar
1. Aumentar cobertura de UI tests
2. Agregar tests de integración con base de datos
3. Implementar visual regression testing

---

## ?? Documentación Completa

Ver `docs/TESTING_STRATEGY.md` para:
- Guía completa de testing
- Best practices
- Configuración avanzada
- Métricas de cobertura
- CI/CD setup

---

**Tests Totales:** 68+  
**Tiempo de Ejecución:** ~2 minutos (sin E2E), ~7 minutos (con E2E)  
**Estado:** ? READY TO USE
