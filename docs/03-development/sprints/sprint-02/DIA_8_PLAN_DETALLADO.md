# ?? DÍA 8 - Detalle de Propiedad, Portafolio y SEO

**Fecha Estimada**: 26 de Noviembre de 2024  
**Duración Estimada**: 4-5 horas  
**Prioridad**: ALTA

---

## ?? Objetivos del Día

### 1. Página de Detalle de Propiedad (2 horas)
- Galería de imágenes con lightbox
- Información completa de la propiedad
- Formulario de contacto integrado
- Mapa de ubicación (Google Maps)
- Propiedades relacionadas
- Botones de compartir (WhatsApp, Facebook, Email)

### 2. Página de Portafolio (1 hora)
- Showcase de proyectos completados
- Grid de proyectos con filtros
- Caso de estudio por proyecto
- Testimonios de clientes

### 3. SEO y Meta Tags (1.5 horas)
- Open Graph tags dinámicos
- Schema.org markup para propiedades
- Sitemap.xml automático
- robots.txt
- Canonical URLs

### 4. Optimizaciones (0.5 horas)
- Compresión de imágenes
- Preload de recursos críticos
- Service Worker básico

---

## ?? Tareas Detalladas

### Fase 1: Página de Detalle de Propiedad

#### 1.1 Backend - Endpoint de Detalle
```csharp
// Ya existe GetAsync(Guid id) en ListingAppService
// Necesitamos agregar:
Task<List<ListingDto>> GetRelatedPropertiesAsync(Guid listingId, int count = 4);
```

**Archivos a modificar:**
- `src/cima.Application.Contracts/Listings/IListingAppService.cs`
- `src/cima.Application/Listings/ListingAppService.cs`

#### 1.2 Componente ImageGallery
Actualizar `src/cima.Blazor.Client/Components/Public/ImageGallery.razor`:
- Thumbnails navegables
- Lightbox con navegación
- Zoom en hover
- Swipe en mobile

#### 1.3 Componente PropertyDetailInfo
Crear `src/cima.Blazor.Client/Components/Public/PropertyDetailInfo.razor`:
- Información completa de la propiedad
- Características en grid
- Descripción expandible
- Información del arquitecto

#### 1.4 Integración Google Maps
Crear `src/cima.Blazor.Client/Components/Public/PropertyMap.razor`:
- Mapa embebido de Google Maps
- Marcador de ubicación
- Zoom ajustable

#### 1.5 Formulario de Contacto Contextual
Actualizar `src/cima.Blazor.Client/Components/Public/ContactForm.razor`:
- Pre-poblar con información de la propiedad
- Validación mejorada
- Success/Error messages

#### 1.6 Propiedades Relacionadas
Crear `src/cima.Blazor.Client/Components/Public/RelatedProperties.razor`:
- Carousel de propiedades similares
- Filtrado por categoría y precio
- Máximo 4 propiedades

#### 1.7 Share Buttons
Crear `src/cima.Blazor.Client/Components/Public/ShareButtons.razor`:
- WhatsApp con mensaje pre-formateado
- Facebook share
- Email con subject y body
- Copy link to clipboard

---

### Fase 2: Página de Portafolio

#### 2.1 Backend - Endpoint de Portfolio
Ya existe `GetPortfolioAsync` en ListingAppService.

#### 2.2 Componente PortfolioGrid
Crear `src/cima.Blazor.Client/Components/Public/PortfolioGrid.razor`:
- Grid masonry style
- Filtros por tipo de proyecto
- Hover effects
- Click para ver detalle

#### 2.3 Página Portfolio
Actualizar `src/cima.Blazor.Client/Pages/Public/Portfolio/Index.razor`:
- Hero section específico
- Grid de proyectos
- Filtros laterales
- Paginación

#### 2.4 Caso de Estudio
Crear `src/cima.Blazor.Client/Pages/Public/Portfolio/CaseStudy.razor`:
- Detalle del proyecto
- Galería de imágenes
- Proceso de diseño
- Testimonios

---

### Fase 3: SEO y Meta Tags

#### 3.1 Meta Tags Component
Crear `src/cima.Blazor.Client/Components/SEO/MetaTags.razor`:
```razor
<HeadContent>
    <title>@Title</title>
    <meta name="description" content="@Description" />
    
    <!-- Open Graph -->
    <meta property="og:title" content="@Title" />
    <meta property="og:description" content="@Description" />
    <meta property="og:image" content="@ImageUrl" />
    <meta property="og:url" content="@Url" />
    
    <!-- Twitter Card -->
    <meta name="twitter:card" content="summary_large_image" />
    <meta name="twitter:title" content="@Title" />
    <meta name="twitter:description" content="@Description" />
    <meta name="twitter:image" content="@ImageUrl" />
    
    <!-- Canonical -->
    <link rel="canonical" href="@Url" />
</HeadContent>
```

#### 3.2 Schema.org Markup
Crear `src/cima.Blazor.Client/Components/SEO/PropertySchema.razor`:
```json
{
  "@context": "https://schema.org",
  "@type": "RealEstateListing",
  "name": "...",
  "description": "...",
  "price": "...",
  "image": [...],
  "address": {...}
}
```

#### 3.3 Sitemap Generator
Crear `src/cima.Application/SEO/SitemapService.cs`:
- Generar sitemap.xml automáticamente
- Incluir todas las propiedades publicadas
- Incluir páginas estáticas
- Actualización automática

#### 3.4 robots.txt
Crear `src/cima.Blazor/wwwroot/robots.txt`:
```
User-agent: *
Allow: /
Sitemap: https://cimaimmobiliaria.com/sitemap.xml
```

---

### Fase 4: Optimizaciones

#### 4.1 Image Optimization
Crear `src/cima.Application/Images/ImageOptimizationService.cs`:
- Redimensionar imágenes
- Convertir a WebP
- Generar thumbnails
- Lazy loading mejorado

#### 4.2 Preload Critical Resources
Actualizar `src/cima.Blazor/Components/App.razor`:
```html
<HeadContent>
    <link rel="preload" href="/css/app.min.css" as="style" />
    <link rel="preload" href="/fonts/main.woff2" as="font" crossorigin />
</HeadContent>
```

#### 4.3 Service Worker (Opcional)
Crear `src/cima.Blazor/wwwroot/service-worker.js`:
- Caché de assets estáticos
- Offline fallback
- Background sync

---

## ??? Estructura de Archivos

### Nuevos Archivos
```
src/cima.Blazor.Client/
??? Components/
?   ??? Public/
?   ?   ??? PropertyDetailInfo.razor (NUEVO)
?   ?   ??? PropertyMap.razor (NUEVO)
?   ?   ??? RelatedProperties.razor (NUEVO)
?   ?   ??? ShareButtons.razor (NUEVO)
?   ?   ??? PortfolioGrid.razor (NUEVO)
?   ??? SEO/
?       ??? MetaTags.razor (NUEVO)
?       ??? PropertySchema.razor (NUEVO)
??? Pages/
?   ??? Public/
?       ??? Portfolio/
?           ??? Index.razor (NUEVO)
?           ??? CaseStudy.razor (NUEVO)

src/cima.Application/
??? SEO/
?   ??? SitemapService.cs (NUEVO)
??? Images/
    ??? ImageOptimizationService.cs (NUEVO)

src/cima.Blazor/
??? wwwroot/
    ??? robots.txt (NUEVO)
    ??? service-worker.js (NUEVO - Opcional)
```

### Archivos a Modificar
```
src/cima.Application.Contracts/Listings/IListingAppService.cs
src/cima.Application/Listings/ListingAppService.cs
src/cima.Blazor.Client/Components/Public/ImageGallery.razor
src/cima.Blazor.Client/Components/Public/ContactForm.razor
src/cima.Blazor.Client/Pages/Public/Properties/Detail.razor
src/cima.Blazor/Components/App.razor
```

---

## ?? Checklist de Implementación

### Fase 1: Detalle de Propiedad
- [ ] Agregar método `GetRelatedPropertiesAsync` al backend
- [ ] Actualizar `ImageGallery.razor` con lightbox
- [ ] Crear `PropertyDetailInfo.razor`
- [ ] Crear `PropertyMap.razor` con Google Maps
- [ ] Actualizar `ContactForm.razor` contextual
- [ ] Crear `RelatedProperties.razor`
- [ ] Crear `ShareButtons.razor`
- [ ] Actualizar `Detail.razor` con todos los componentes
- [ ] Probar navegación completa
- [ ] Validar responsive design

### Fase 2: Portafolio
- [ ] Crear `PortfolioGrid.razor`
- [ ] Crear página `Portfolio/Index.razor`
- [ ] Crear `Portfolio/CaseStudy.razor`
- [ ] Agregar navegación en menú
- [ ] Probar filtros
- [ ] Validar carga de proyectos

### Fase 3: SEO
- [ ] Crear `MetaTags.razor` component
- [ ] Crear `PropertySchema.razor`
- [ ] Implementar `SitemapService.cs`
- [ ] Crear `robots.txt`
- [ ] Integrar meta tags en todas las páginas
- [ ] Validar con Facebook Debugger
- [ ] Validar con Google Rich Results Test
- [ ] Probar canonical URLs

### Fase 4: Optimizaciones
- [ ] Implementar `ImageOptimizationService.cs`
- [ ] Agregar preload de recursos críticos
- [ ] Configurar lazy loading mejorado
- [ ] (Opcional) Implementar Service Worker
- [ ] Medir performance con Lighthouse
- [ ] Optimizar Core Web Vitals

---

## ?? Configuración Requerida

### Google Maps API Key
1. Ir a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear nuevo proyecto o usar existente
3. Habilitar "Maps JavaScript API"
4. Crear API Key
5. Restringir por dominio
6. Agregar a `appsettings.json`:
```json
{
  "GoogleMaps": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### Open Graph Debugger
- Facebook: https://developers.facebook.com/tools/debug/
- LinkedIn: https://www.linkedin.com/post-inspector/
- Twitter: https://cards-dev.twitter.com/validator

### SEO Tools
- Google Search Console
- Google Rich Results Test
- Lighthouse (Chrome DevTools)

---

## ?? Scripts y Comandos

### Generar Sitemap
```powershell
# Agregar endpoint en API
curl https://localhost:44365/api/sitemap/generate
```

### Validar SEO
```powershell
# Lighthouse CLI
npm install -g lighthouse
lighthouse https://localhost:44365 --view
```

### Optimizar Imágenes
```powershell
# Usando ImageMagick
magick convert input.jpg -quality 85 -resize 1200x output.webp
```

---

## ?? Métricas de Éxito

### Performance
- [ ] Lighthouse Performance Score > 90
- [ ] First Contentful Paint < 1.5s
- [ ] Largest Contentful Paint < 2.5s
- [ ] Cumulative Layout Shift < 0.1

### SEO
- [ ] Lighthouse SEO Score > 95
- [ ] Todas las páginas tienen meta tags
- [ ] Schema.org markup válido
- [ ] Sitemap.xml accesible
- [ ] robots.txt configurado

### UX
- [ ] Galería de imágenes fluida
- [ ] Mapa carga correctamente
- [ ] Share buttons funcionan
- [ ] Formulario valida inputs
- [ ] Responsive en mobile

---

## ?? Problemas Anticipados

### Google Maps
**Problema**: Clave API no funciona  
**Solución**: Verificar que está habilitada Maps JavaScript API y que el dominio está en la lista blanca

### Lightbox
**Problema**: No abre en mobile  
**Solución**: Usar librería compatible con touch events (PhotoSwipe)

### Meta Tags
**Problema**: No se actualizan en redes sociales  
**Solución**: Usar Facebook Debugger para invalidar caché

### Sitemap
**Problema**: No se genera automáticamente  
**Solución**: Implementar endpoint y job scheduler

---

## ?? Recursos y Referencias

### Librerías Recomendadas
- **Lightbox**: PhotoSwipe, GLightbox
- **Maps**: Google Maps JavaScript API
- **Share**: Web Share API nativo
- **SEO**: MetaTags NuGet package

### Documentación
- [Open Graph Protocol](https://ogp.me/)
- [Schema.org - RealEstateListing](https://schema.org/RealEstateListing)
- [Google Maps JavaScript API](https://developers.google.com/maps/documentation/javascript)
- [Web Share API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Share_API)

---

## ? Entregables

1. **Página de Detalle Completa**
   - Con galería, mapa, formulario y shares
   
2. **Página de Portafolio**
   - Grid de proyectos y casos de estudio
   
3. **SEO Implementado**
   - Meta tags, Schema.org, sitemap, robots.txt
   
4. **Optimizaciones**
   - Imágenes, preload, caché

5. **Documentación**
   - `DIA_8_COMPLETADO.md`
   - Guía de configuración de Google Maps
   - Guía de SEO

6. **Commits Atómicos**
   - Mínimo 8 commits organizados
   - Cada fase en commits separados

---

## ?? Siguientes Pasos (Día 9)

1. **Admin Dashboard Completo**
   - Estadísticas visuales
   - Gráficas con Chart.js
   - Reportes exportables

2. **Featured Management UI**
   - CRUD visual de featured
   - Drag & drop para reordenar
   - Preview en tiempo real

3. **Analytics Integration**
   - Google Analytics 4
   - Event tracking
   - Conversion funnels

4. **Email Templates**
   - Notificaciones automáticas
   - Templates responsive
   - SendGrid/SMTP

---

**Tiempo Estimado Total**: 4-5 horas  
**Complejidad**: Media-Alta  
**Prioridad**: ALTA ???

---

¿Deseas comenzar con el Día 8 ahora o prefieres hacer push de los commits del Día 7 primero?
