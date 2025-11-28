# ?? GUÍA RÁPIDA DE CLASES CSS CIMA

## ?? Índice
1. [Secciones y Layouts](#secciones-y-layouts)
2. [Cards y Componentes](#cards-y-componentes)
3. [Botones](#botones)
4. [Forms](#forms)
5. [Tipografía](#tipografía)
6. [Backgrounds](#backgrounds)
7. [Utilidades](#utilidades)

---

## Secciones y Layouts

### Secciones Fullscreen
```html
<!-- Sección de altura completa (100vh) -->
<section class="cima-section-fullscreen">
    <!-- Contenido -->
</section>

<!-- Sección con padding estándar -->
<section class="cima-section">
    <!-- Contenido con py-16 px-4 -->
</section>

<!-- Hero section -->
<div class="cima-hero">
    <div class="cima-hero-overlay"></div>
    <div class="cima-hero-content">
        <!-- Contenido del hero -->
    </div>
</div>
```

### Contenedores
```html
<!-- Contenedor responsivo centrado -->
<div class="cima-container">
    <!-- max-w-7xl mx-auto -->
</div>
```

### Grids
```html
<!-- Grid para propiedades (3 cols en desktop) -->
<div class="cima-grid-properties">
    <!-- Contenido -->
</div>

<!-- Grid para portfolio (2 cols) -->
<div class="cima-grid-portfolio">
    <!-- Contenido -->
</div>
```

---

## Cards y Componentes

### Cards Básicos
```html
<!-- Card estándar -->
<div class="cima-card">
    <div class="cima-card-header">
        <h3>Título</h3>
    </div>
    <div class="cima-card-body">
        <p>Contenido</p>
    </div>
</div>

<!-- Card con efecto glass -->
<div class="cima-card-glass p-8">
    <!-- Contenido con glassmorphism -->
</div>
```

### Badges
```html
<!-- Badges de estado -->
<span class="cima-badge cima-badge-draft">Borrador</span>
<span class="cima-badge cima-badge-published">Publicada</span>
<span class="cima-badge cima-badge-archived">Archivada</span>
<span class="cima-badge cima-badge-portfolio">Portfolio</span>
```

### Stat Box
```html
<div class="cima-stat-box">
    <h3>100</h3>
    <p>Propiedades</p>
</div>
```

---

## Botones

### Botones Principales
```html
<!-- Botón primario (gradiente azul) -->
<button class="cima-btn-primary">
    Acción Principal
</button>

<!-- Botón secundario (gris) -->
<button class="cima-btn-secondary">
    Acción Secundaria
</button>

<!-- Botón success (verde) -->
<button class="cima-btn-success">
    Guardar
</button>

<!-- Botón danger (rojo) -->
<button class="cima-btn-danger">
    Eliminar
</button>
```

### Botones Outline y Ghost
```html
<!-- Botón outline -->
<button class="cima-btn-outline">
    Ver más
</button>

<!-- Botón ghost -->
<button class="cima-btn-ghost">
    Cancelar
</button>
```

---

## Forms

### Inputs y Labels
```html
<div class="cima-input-group">
    <label class="cima-label">
        Nombre
    </label>
    <input type="text" class="cima-input" />
</div>
```

---

## Tipografía

### Headings
```html
<!-- Heading 1 (4xl-6xl) -->
<h1 class="cima-heading-1">
    Título Principal
</h1>

<!-- Heading 2 (3xl-4xl) -->
<h2 class="cima-heading-2">
    Título Secundario
</h2>

<!-- Heading 3 (2xl-3xl) -->
<h3 class="cima-heading-3">
    Subtítulo
</h3>

<!-- Subtitle (lg-xl gray) -->
<p class="cima-subtitle">
    Descripción o subtítulo
</p>

<!-- Precio destacado -->
<div class="cima-price">
    $1,250,000
</div>
```

---

## Backgrounds

### Fondos de Gradiente
```html
<!-- Gradiente primario (azul) -->
<div class="cima-bg-gradient-primary">
    <!-- Contenido -->
</div>

<!-- Gradiente secundario (gris) -->
<div class="cima-bg-gradient-secondary">
    <!-- Contenido -->
</div>
```

### Fondos Sólidos
```html
<!-- Fondo claro (gray-50) -->
<div class="cima-bg-light">
    <!-- Contenido -->
</div>

<!-- Fondo oscuro (gray-900) -->
<div class="cima-bg-dark">
    <!-- Contenido -->
</div>
```

---

## Utilidades

### Animaciones
```html
<!-- Fade in -->
<div class="cima-fade-in">
    <!-- Se desvanece al aparecer -->
</div>

<!-- Slide up -->
<div class="cima-slide-up">
    <!-- Se desliza hacia arriba -->
</div>
```

### Estados de Carga
```html
<!-- Spinner de carga -->
<div class="cima-spinner"></div>

<!-- Skeleton loader -->
<div class="cima-skeleton h-8 w-full"></div>
```

### Scroll Indicator
```html
<div class="cima-scroll-indicator">
    <button>
        <i class="fas fa-chevron-down"></i>
    </button>
</div>
```

### Sin Gaps
```html
<!-- Eliminar márgenes entre secciones -->
<div class="no-section-gap">
    <section>...</section>
    <section>...</section>
</div>
```

### Full Bleed
```html
<!-- Ancho completo sin padding -->
<div class="full-bleed">
    <!-- Ocupa 100vw -->
</div>
```

---

## ?? Aspect Ratios

```html
<!-- Video (16:9) -->
<div class="aspect-video">
    <img src="..." />
</div>

<!-- Cuadrado (1:1) -->
<div class="aspect-square">
    <img src="..." />
</div>

<!-- Retrato (3:4) -->
<div class="aspect-portrait">
    <img src="..." />
</div>
```

---

## ?? Ejemplos Completos

### Hero Section
```html
<div class="cima-hero">
    <div class="absolute inset-0">
        <img src="bg.jpg" class="w-full h-full object-cover" />
        <div class="cima-hero-overlay"></div>
    </div>
    
    <div class="cima-hero-content">
        <div class="cima-card-glass p-12">
            <h1 class="cima-heading-1 mb-4">
                Bienvenido
            </h1>
            <p class="cima-subtitle mb-8">
                Encuentra tu hogar ideal
            </p>
            <button class="cima-btn-primary">
                Explorar
            </button>
        </div>
    </div>
    
    <div class="cima-scroll-indicator">
        <button>
            <i class="fas fa-chevron-down"></i>
        </button>
    </div>
</div>
```

### Sección de Propiedades
```html
<section class="cima-section-fullscreen cima-bg-light">
    <div class="cima-container">
        <div class="text-center mb-12 cima-fade-in">
            <h2 class="cima-heading-2 mb-4">
                Propiedades Destacadas
            </h2>
            <p class="cima-subtitle">
                Encuentra la propiedad perfecta
            </p>
        </div>
        
        <div class="cima-grid-properties cima-slide-up">
            <!-- Cards de propiedades -->
        </div>
    </div>
</section>
```

### Formulario de Contacto
```html
<div class="cima-card">
    <div class="cima-card-header">
        <h3>Contáctanos</h3>
    </div>
    <div class="cima-card-body">
        <div class="cima-input-group">
            <label class="cima-label">Nombre</label>
            <input type="text" class="cima-input" />
        </div>
        
        <div class="cima-input-group">
            <label class="cima-label">Email</label>
            <input type="email" class="cima-input" />
        </div>
        
        <div class="flex gap-4">
            <button class="cima-btn-primary flex-1">
                Enviar
            </button>
            <button class="cima-btn-ghost">
                Cancelar
            </button>
        </div>
    </div>
</div>
```

---

## ?? Tips de Uso

### ? Buenas Prácticas
1. **Usar clases semánticas**: Preferir `cima-btn-primary` sobre combinar clases de Tailwind
2. **Combinar con Tailwind**: Las clases CIMA son compatibles con Tailwind
3. **Responsive**: Todas las clases son responsive por defecto
4. **Animaciones**: Usar `cima-fade-in` y `cima-slide-up` para transiciones

### ? Evitar
1. No mezclar estilos inline con clases CIMA
2. No duplicar estilos (usar solo la clase CIMA)
3. No olvidar `no-section-gap` para eliminar espacios

---

## ?? Personalización

### Variables de Color (tailwind.config.js)
```js
colors: {
  'cima-primary': '#2563eb',    // Azul
  'cima-secondary': '#374151',  // Gris oscuro
  'cima-success': '#10b981',    // Verde
  'cima-danger': '#ef4444',     // Rojo
}
```

### Compilar CSS
```bash
cd src/cima.Blazor.Client
npm run build:css
```

---

## ?? Responsive

Todas las clases son responsive:
- Mobile first approach
- Breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px)
- Las secciones fullscreen se adaptan automáticamente

---

**Última actualización**: Día 7 - Sistema CSS Centralizado
