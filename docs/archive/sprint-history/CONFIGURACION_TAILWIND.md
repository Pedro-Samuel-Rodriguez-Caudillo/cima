# Configuración de Tailwind CSS - CIMA

## Estado Actual

? Tailwind CSS configurado y funcionando en el proyecto Blazor WebAssembly

## Archivos Configurados

### 1. `src/cima.Blazor.Client/package.json`
Contiene las dependencias de Tailwind y los scripts npm:
```json
{
  "scripts": {
    "build:css": "tailwindcss -i ./wwwroot/css/app.css -o ./wwwroot/css/app.min.css --minify",
    "watch:css": "tailwindcss -i ./wwwroot/css/app.css -o ./wwwroot/css/app.min.css --watch",
    "dev": "npm run watch:css"
  },
  "devDependencies": {
    "@tailwindcss/forms": "^0.5.10",
    "@tailwindcss/typography": "^0.5.19",
    "autoprefixer": "^10.4.22",
    "postcss": "^8.5.6",
    "tailwindcss": "^3.4.0"
  }
}
```

### 2. `src/cima.Blazor.Client/tailwind.config.js`
Configuración de Tailwind con colores personalizados CIMA:
```javascript
module.exports = {
  content: [
    './Pages/**/*.{razor,html}',
    './Components/**/*.{razor,html}',
    './Shared/**/*.{razor,html}',
    './*.razor',
    './wwwroot/index.html'
  ],
  theme: {
    extend: {
      colors: {
        'cima-primary': '#2563eb',
        'cima-secondary': '#64748b',
        'cima-success': '#10b981',
        'cima-danger': '#ef4444',
        'cima-warning': '#f59e0b',
        'cima-info': '#06b6d4',
      },
      // ...
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}
```

### 3. `src/cima.Blazor.Client/postcss.config.js`
Configuración de PostCSS para optimización:
```javascript
module.exports = {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
```

### 4. `src/cima.Blazor.Client/wwwroot/css/app.css`
Archivo fuente con directivas Tailwind y clases personalizadas:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer components {
  .cima-card { /* ... */ }
  .cima-btn-primary { /* ... */ }
  /* más clases personalizadas */
}
```

### 5. `src/cima.Blazor/Components/App.razor`
Referencia al CSS compilado:
```html
<link href="css/app.min.css" rel="stylesheet"/>
```

## Clases Personalizadas CIMA

### Cards
- `.cima-card` - Card base con sombra y hover
- `.cima-card-header` - Header con gradiente azul
- `.cima-card-body` - Body del card con padding

### Badges (Estados)
- `.cima-badge` - Badge base
- `.cima-badge-draft` - Estado borrador (gris)
- `.cima-badge-published` - Estado publicado (verde)
- `.cima-badge-archived` - Estado archivado (amarillo)

### Botones
- `.cima-btn` - Botón base
- `.cima-btn-primary` - Botón primario (azul)
- `.cima-btn-secondary` - Botón secundario (gris)
- `.cima-btn-success` - Botón éxito (verde)
- `.cima-btn-danger` - Botón peligro (rojo)

### Formularios
- `.cima-input` - Input de texto estilizado

### Utilidades
- `.cima-stat-box` - Caja de estadísticas
- `.cima-price` - Formato de precio
- `.cima-spinner` - Spinner de carga
- `.fade-in` - Animación de entrada

## Comandos Disponibles

### NPM Directo (desde src/cima.Blazor.Client)

```bash
# Instalar dependencias
npm install

# Compilar CSS para producción (minificado)
npm run build:css

# Modo watch para desarrollo
npm run watch:css

# Alias de watch
npm run dev
```

### Scripts PowerShell (desde raíz del proyecto)

```powershell
# Compilar CSS para producción
.\etc\scripts\build-tailwind.ps1

# Iniciar modo watch para desarrollo
.\etc\scripts\start-tailwind-watch.ps1
```

## Flujo de Trabajo

### Desarrollo

1. **Iniciar el watcher** (en una terminal):
   ```powershell
   .\etc\scripts\start-tailwind-watch.ps1
   ```
   Esto detectará cambios automáticamente en archivos `.razor` y recompilará el CSS.

2. **Desarrollar componentes** usando clases de Tailwind:
   ```razor
   <div class="cima-card">
       <div class="cima-card-header">
           <h2 class="text-2xl font-bold">Mi Card</h2>
       </div>
       <div class="cima-card-body">
           <button class="cima-btn-primary">Acción</button>
       </div>
   </div>
   ```

3. **Hot Reload de Blazor** se encargará de refrescar la UI.

### Producción

1. **Compilar CSS optimizado** antes de deploy:
   ```powershell
   .\etc\scripts\build-tailwind.ps1
   ```

2. El archivo `app.min.css` se genera con:
   - CSS purgado (solo clases usadas)
   - Minificado
   - Autoprefixer aplicado
   - Tamaño optimizado (~10-15 KB típicamente)

## Colores CIMA

| Variable | Color | Uso |
|----------|-------|-----|
| `cima-primary` | #2563eb (Azul) | Acciones principales, links |
| `cima-secondary` | #64748b (Gris) | Acciones secundarias |
| `cima-success` | #10b981 (Verde) | Confirmaciones, éxitos |
| `cima-danger` | #ef4444 (Rojo) | Errores, eliminaciones |
| `cima-warning` | #f59e0b (Naranja) | Advertencias |
| `cima-info` | #06b6d4 (Cyan) | Información |

Uso:
```html
<div class="bg-cima-primary text-white">Fondo azul</div>
<button class="text-cima-danger">Texto rojo</button>
```

## Plugins Instalados

### @tailwindcss/forms
Mejora el estilo de formularios HTML nativos.

```html
<input type="text" class="form-input rounded-md" />
```

### @tailwindcss/typography
Estiliza contenido de texto (útil para blogs, descripciones).

```html
<article class="prose lg:prose-xl">
  <!-- Contenido markdown/HTML se estiliza automáticamente -->
</article>
```

## Troubleshooting

### El CSS no se actualiza

1. Verificar que el watcher esté corriendo:
   ```powershell
   .\etc\scripts\start-tailwind-watch.ps1
   ```

2. Si usas clases dinámicas, asegúrate de que el patrón esté en `content`:
   ```javascript
   // NO usar interpolación dinámica
   <div class={`bg-${color}-500`}> ?
   
   // Usar clases completas
   <div class="bg-blue-500"> ?
   ```

3. Limpiar cache y recompilar:
   ```powershell
   cd src/cima.Blazor.Client
   Remove-Item wwwroot/css/app.min.css
   npm run build:css
   ```

### Clases Tailwind no funcionan

1. Verificar que `app.min.css` está referenciado en `App.razor`:
   ```html
   <link href="css/app.min.css" rel="stylesheet"/>
   ```

2. Verificar que el archivo existe:
   ```powershell
   Test-Path src/cima.Blazor.Client/wwwroot/css/app.min.css
   ```

3. Revisar la consola del navegador para errores de carga.

### Warning sobre node_modules

Si ves warnings sobre `node_modules`, verifica que `tailwind.config.js` tenga patrones específicos:
```javascript
content: [
  './Pages/**/*.{razor,html}',  // ? Específico
  './**/*.html',                 // ? Muy amplio
]
```

## Próximos Pasos

### Día 4 - Componentes UI

1. **Crear componentes reutilizables** en `src/cima.Blazor.Client/Components/`:
   - `PropertyCard.razor` - Card de propiedad
   - `PropertyFilters.razor` - Filtros de búsqueda
   - `ContactForm.razor` - Formulario de contacto

2. **Implementar páginas principales**:
   - `Home.razor` - Landing page
   - `Properties.razor` - Listado de propiedades
   - `PropertyDetail.razor` - Detalle de propiedad

3. **Agregar animaciones**:
   ```css
   @layer components {
     .slide-in {
       animation: slideIn 0.3s ease-out;
     }
   }
   ```

### Integración con ABP

Las clases CIMA coexisten con las de ABP:
```html
<!-- Usar ABP para layout general -->
<AbpDynamicLayoutComponent />

<!-- Usar CIMA para componentes custom -->
<div class="cima-card">
  <PropertyCard Property="@property" />
</div>
```

## Referencias

- [Tailwind CSS Docs](https://tailwindcss.com/docs)
- [Tailwind Forms Plugin](https://github.com/tailwindlabs/tailwindcss-forms)
- [Tailwind Typography Plugin](https://tailwindcss.com/docs/typography-plugin)
- [Blazor CSS Isolation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation)

## Checklist de Configuración

- [x] Dependencias npm instaladas
- [x] `tailwind.config.js` configurado
- [x] `postcss.config.js` creado
- [x] `app.css` con directivas Tailwind
- [x] CSS compilado a `app.min.css`
- [x] Referencia en `App.razor`
- [x] Clases personalizadas CIMA definidas
- [x] Scripts de PowerShell creados
- [x] Documentación completa

¡Tailwind CSS está listo para usar! ??
