# Quick Start - Tailwind CSS en CIMA

## ?? Inicio Rápido (2 minutos)

### 1. Levantar el Watcher (Terminal 1)

```powershell
.\etc\scripts\start-tailwind-watch.ps1
```

Dejar corriendo. Este proceso detecta cambios en archivos `.razor` y recompila automáticamente el CSS.

### 2. Levantar la Aplicación (Terminal 2)

```powershell
dotnet run --project src/cima.Blazor
```

### 3. Ver Ejemplo

Navegar a: `https://localhost:44350`

## ?? Clases Más Usadas

### Cards y Contenedores

```razor
<!-- Card básica -->
<div class="cima-card">
    <div class="cima-card-header">
        <h3>Título</h3>
    </div>
    <div class="cima-card-body">
        Contenido...
    </div>
</div>
```

### Botones

```razor
<!-- Botón primario -->
<button class="cima-btn-primary">Guardar</button>

<!-- Botón secundario -->
<button class="cima-btn-secondary">Cancelar</button>

<!-- Botón de éxito -->
<button class="cima-btn-success">Enviar</button>

<!-- Botón de peligro -->
<button class="cima-btn-danger">Eliminar</button>
```

### Formularios

```razor
<!-- Input de texto -->
<input type="text" class="cima-input" placeholder="Nombre" />

<!-- Select -->
<select class="cima-input">
    <option>Opción 1</option>
    <option>Opción 2</option>
</select>

<!-- Textarea -->
<textarea class="cima-input" rows="4"></textarea>
```

### Badges (Estados)

```razor
<span class="cima-badge cima-badge-draft">Borrador</span>
<span class="cima-badge cima-badge-published">Publicado</span>
<span class="cima-badge cima-badge-archived">Archivado</span>
```

### Precios

```razor
<p class="cima-price">$2,500,000</p>
```

### Loading Spinner

```razor
<div class="cima-spinner"></div>
```

## ?? Colores de Marca

Usar en clases de Tailwind:

```razor
<!-- Textos -->
<p class="text-cima-primary">Texto azul</p>
<p class="text-cima-success">Texto verde</p>
<p class="text-cima-danger">Texto rojo</p>

<!-- Fondos -->
<div class="bg-cima-primary">Fondo azul</div>
<div class="bg-cima-secondary">Fondo gris</div>

<!-- Bordes -->
<div class="border-2 border-cima-primary">Con borde</div>
```

## ?? Grid Layout Común

```razor
<!-- 3 columnas responsive -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <div class="cima-card">Card 1</div>
    <div class="cima-card">Card 2</div>
    <div class="cima-card">Card 3</div>
</div>
```

## ?? Ejemplo Completo: Card de Propiedad

```razor
<div class="cima-card">
    <div class="cima-card-header">
        <div class="flex justify-between items-center">
            <h3 class="text-xl font-bold">@Property.Title</h3>
            <span class="cima-badge cima-badge-published">Activa</span>
        </div>
    </div>
    
    <div class="cima-card-body">
        <!-- Imagen -->
        <img src="@Property.ImageUrl" 
             class="w-full h-48 object-cover rounded-lg mb-4" />
        
        <!-- Precio -->
        <p class="cima-price">$@Property.Price.ToString("N0")</p>
        
        <!-- Características -->
        <div class="grid grid-cols-3 gap-2 my-4">
            <div class="text-center">
                <p class="font-bold">@Property.Bedrooms</p>
                <p class="text-sm text-gray-600">Recámaras</p>
            </div>
            <div class="text-center">
                <p class="font-bold">@Property.Bathrooms</p>
                <p class="text-sm text-gray-600">Baños</p>
            </div>
            <div class="text-center">
                <p class="font-bold">@Property.SurfaceArea</p>
                <p class="text-sm text-gray-600">m²</p>
            </div>
        </div>
        
        <!-- Acciones -->
        <div class="flex gap-2">
            <button class="cima-btn-primary flex-1" 
                    @onclick="() => ViewDetails(Property.Id)">
                Ver Detalles
            </button>
            <button class="cima-btn-success" 
                    @onclick="() => Contact(Property.Id)">
                Contactar
            </button>
        </div>
    </div>
</div>

@code {
    [Parameter] public PropertyDto Property { get; set; }
    
    void ViewDetails(Guid id) { /* ... */ }
    void Contact(Guid id) { /* ... */ }
}
```

## ?? Tips Útiles

### 1. Animaciones Suaves

```razor
<!-- Fade in al cargar -->
<div class="fade-in">Contenido...</div>

<!-- Hover -->
<div class="hover:shadow-xl transition-shadow duration-300">
    Hover me
</div>
```

### 2. Responsive

```razor
<!-- Padding responsive -->
<div class="p-4 md:p-6 lg:p-8">...</div>

<!-- Texto responsive -->
<h1 class="text-2xl md:text-4xl lg:text-5xl">Título</h1>

<!-- Grid responsive -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
    <!-- Items -->
</div>
```

### 3. Espaciado Consistente

```razor
<!-- Espacios verticales -->
<div class="space-y-4">
    <div>Item 1</div>
    <div>Item 2</div>
</div>

<!-- Espacios horizontales -->
<div class="flex space-x-4">
    <button>Botón 1</button>
    <button>Botón 2</button>
</div>
```

### 4. Formularios con Plugin @tailwindcss/forms

```razor
<form class="space-y-4">
    <div>
        <label class="block text-sm font-medium mb-2">
            Nombre
        </label>
        <input type="text" 
               class="form-input rounded-lg w-full" 
               placeholder="Tu nombre" />
    </div>
    
    <div>
        <label class="block text-sm font-medium mb-2">
            Email
        </label>
        <input type="email" 
               class="form-input rounded-lg w-full" 
               placeholder="tu@email.com" />
    </div>
    
    <button type="submit" class="cima-btn-primary w-full">
        Enviar
    </button>
</form>
```

## ? Comandos Rápidos

```powershell
# Compilar CSS (producción)
.\etc\scripts\build-tailwind.ps1

# Modo watch (desarrollo)
.\etc\scripts\start-tailwind-watch.ps1

# Reinstalar dependencias
cd src/cima.Blazor.Client
npm install
```

## ?? Solución Rápida de Problemas

### El CSS no se actualiza

```powershell
# Reiniciar el watcher
Ctrl+C  # Detener
.\etc\scripts\start-tailwind-watch.ps1  # Reiniciar
```

### Clases no funcionan

1. Verificar que el watcher esté corriendo
2. Verificar que `app.min.css` exista en `wwwroot/css/`
3. Hacer hard refresh en el navegador (Ctrl+Shift+R)

### Build para producción

```powershell
# 1. Compilar CSS
.\etc\scripts\build-tailwind.ps1

# 2. Publicar aplicación
dotnet publish src/cima.Blazor -c Release
```

## ?? Recursos

- **Documentación completa:** `docs/CONFIGURACION_TAILWIND.md`
- **Ejemplo visual:** `src/cima.Blazor.Client/Components/Examples/TailwindExample.razor`
- **Clases definidas:** `src/cima.Blazor.Client/wwwroot/css/app.css`
- **Tailwind Docs:** https://tailwindcss.com/docs

## ?? Siguiente Paso

Crear tu primer componente:

```powershell
# Crear archivo
New-Item src/cima.Blazor.Client/Components/PropertyCard.razor

# Usar las clases CIMA
# Ver ejemplo completo arriba ?
```

¡Listo para crear interfaces increíbles! ??
