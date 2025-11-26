# ?? GUÍA DE VERIFICACIÓN - MEJORAS UX/UI

## ?? CHECKLIST DE PRUEBAS

### ? 1. ContactForm - Validación ListingId

**Escenario 1: Sin propiedad seleccionada**
```
1. Navegar a /contact (si existe ruta directa)
2. Verificar que se muestra warning amarillo
3. Texto esperado: "Este formulario requiere una propiedad seleccionada..."
4. Formulario NO debe mostrarse
```

**Escenario 2: Con propiedad seleccionada**
```
1. Ir a /properties
2. Clic en "Ver Detalles" de cualquier propiedad
3. Scroll a formulario de contacto
4. Formulario debe estar visible y funcional
5. Llenar todos los campos:
   - Nombre: "Juan Pérez"
   - Email: "juan@test.com"
   - Teléfono: "55 1234 5678"
   - Mensaje: "Estoy interesado en esta propiedad"
6. Clic en "Enviar consulta"
7. Verificar mensaje de éxito
8. Backend debe recibir ListingId (verificar en logs o BD)
```

**Query para verificar en PostgreSQL:**
```sql
-- Conectar a PostgreSQL
docker exec -it cima-postgres psql -U postgres -d cima

-- Ver últimas solicitudes de contacto
SELECT 
    "Id",
    "ListingId",
    "Name",
    "Email",
    "Message",
    "CreatedAt"
FROM "ContactRequests"
ORDER BY "CreatedAt" DESC
LIMIT 5;

-- Verificar que ListingId NO sea NULL
SELECT COUNT(*) 
FROM "ContactRequests" 
WHERE "ListingId" IS NULL;
-- Esperado: 0
```

---

### ? 2. ContactForm - Localización

**Prueba de textos localizados:**
```
1. Abrir detalle de propiedad
2. Verificar textos del formulario:
   ? Título: "¿Interesado en esta propiedad?"
   ? Campo Nombre: "Nombre completo"
   ? Placeholder: "Juan Pérez"
   ? Campo Email: "Correo electrónico"
   ? Placeholder: "juan@ejemplo.com"
   ? Campo Teléfono: "Teléfono"
   ? Placeholder: "55 1234 5678"
   ? Campo Mensaje: "Mensaje"
   ? Botón: "Enviar consulta"
```

**Prueba de mensajes dinámicos:**
```
1. Ver mensaje pre-llenado en campo "Mensaje"
2. Debe decir: "Estoy interesado en la propiedad: [Título de la propiedad]"
3. Ejemplo: "Estoy interesado en la propiedad: Casa Moderna con Vista al Mar"
```

**Prueba de mensajes de éxito:**
```
1. Enviar formulario válido
2. Verificar mensaje de éxito:
   ? Título: "¡Mensaje enviado!"
   ? Mensaje: "Gracias por tu interés. Nos pondremos en contacto contigo pronto."
   ? Botón: "Enviar otro mensaje"
```

**Verificar en código fuente (DevTools):**
```html
<!-- Debe tener @L["..."] en lugar de texto hardcodeado -->
<h3>@L["ContactForm:Title"]</h3>
<label>@L["ContactForm:Name"]</label>
```

---

### ? 3. Index.razor - Sin CSS Embebido

**Inspección visual:**
```
1. Navegar a página de inicio /
2. Abrir DevTools (F12)
3. Ir a pestaña "Elements" / "Inspector"
4. Buscar <style> dentro del componente Index.razor
5. NO debe existir bloque <style>
```

**Verificar clases Tailwind:**
```html
<!-- Esperado en Index.razor -->
<div class="m-0 p-0 space-y-0">
  <section class="cima-hero m-0 p-0">
  <section id="featured-properties" class="cima-section-fullscreen cima-bg-light m-0 p-0">
  <section class="cima-section-fullscreen bg-white m-0 p-0">
</div>
```

**Prueba de espaciado:**
```
1. Verificar que NO haya gaps entre secciones
2. Hero Section debe conectar directamente con Propiedades Destacadas
3. Propiedades Destacadas debe conectar con Sección de Contacto
4. Sin espacios blancos innecesarios
```

---

### ? 4. ImageGallery - Responsive

**Mobile (< 640px):**
```
1. Redimensionar ventana a 375px de ancho (iPhone)
2. Verificar galería de imágenes:
   ? Altura imagen principal: ~256px (h-64)
   ? Miniaturas: 2 columnas
   ? Botones navegación: pequeños (p-2)
   ? Iconos: pequeños (text-sm)
   ? Contador: compacto
```

**Tablet (640px - 1024px):**
```
1. Redimensionar ventana a 768px (iPad)
2. Verificar galería:
   ? Altura imagen principal: ~320px (h-80)
   ? Miniaturas: 4 columnas
   ? Botones navegación: medianos (p-3)
   ? Iconos: medianos (text-base)
```

**Desktop (> 1024px):**
```
1. Redimensionar ventana a 1280px
2. Verificar galería:
   ? Altura imagen principal: ~384px (h-96)
   ? Miniaturas: 6 columnas
   ? Botones navegación: medianos (p-3)
   ? Iconos: medianos (text-base)
```

**Aspect Ratio:**
```
1. Abrir DevTools > Inspector
2. Seleccionar imagen principal
3. Verificar clase: aspect-video
4. Verificar que imágenes mantengan proporción 16:9
5. NO deben verse distorsionadas (estiradas o aplastadas)
```

**Prueba de navegación:**
```
1. Clic en botón "Siguiente" (chevron derecho)
2. Imagen debe cambiar suavemente
3. Contador debe actualizarse (ej: 2 / 5)
4. Clic en miniatura
5. Imagen principal debe cambiar a la seleccionada
6. Miniatura activa debe tener borde azul (ring-2 ring-blue-600)
```

---

### ? 5 & 6. HeroSection - Scroll Suave

**Prueba Desktop:**
```
1. Navegar a página de inicio /
2. Scroll hasta arriba (Hero Section visible)
3. Observar icono de flecha abajo (?)
4. Verificar animación "bounce" (rebote sutil)
5. Clic en icono de flecha
6. Página debe hacer scroll suave hacia "Propiedades Destacadas"
7. Scroll debe ser animado (smooth), no instantáneo
8. Debe detenerse al inicio de la sección
```

**Prueba Mobile:**
```
1. Redimensionar ventana a 375px (iPhone)
2. Repetir pasos de prueba Desktop
3. Scroll debe funcionar igual de suave
4. Icono debe ser visible y clickeable
```

**Prueba de precisión:**
```
1. Abrir DevTools > Console
2. Ejecutar:
   const section = document.getElementById('featured-properties');
   console.log(section); // Debe mostrar el elemento
3. Verificar que sección tiene ID correcto
4. Clic en flecha de scroll
5. Sección "featured-properties" debe quedar alineada al top
```

**Prueba de accesibilidad:**
```
1. Usar Tab para navegar con teclado
2. Llegar al botón de scroll
3. Verificar que tiene foco visible
4. Presionar Enter o Espacio
5. Scroll debe activarse
6. Lector de pantalla debe leer: "Scroll to properties" o "Ver propiedades destacadas"
```

**Verificar en DevTools (Console):**
```javascript
// Verificar que IJSRuntime funciona
document.getElementById('featured-properties').scrollIntoView({ 
    behavior: 'smooth', 
    block: 'start' 
});
// Debe hacer scroll suave
```

---

## ?? RESULTADOS ESPERADOS

### ? Todas las pruebas pasan

| Prueba | Resultado Esperado | Estado |
|--------|-------------------|--------|
| ContactForm sin ListingId | Warning visible | ? |
| ContactForm con ListingId | Formulario funcional | ? |
| ListingId en backend | Valor correcto en BD | ? |
| Textos localizados | 17+ textos en español | ? |
| Mensajes dinámicos | Título de propiedad correcto | ? |
| Sin CSS embebido | No existe <style> | ? |
| Espaciado correcto | Sin gaps entre secciones | ? |
| Responsive mobile | h-64, 2 columnas | ? |
| Responsive tablet | h-80, 4 columnas | ? |
| Responsive desktop | h-96, 6 columnas | ? |
| Aspect ratio | Imágenes 16:9 | ? |
| Scroll desktop | Suave, animado | ? |
| Scroll mobile | Funcional, suave | ? |
| Animación bounce | Visible en icono | ? |
| Accesibilidad | Tab + Enter funciona | ? |

---

## ?? PROBLEMAS COMUNES Y SOLUCIONES

### Problema 1: Formulario no muestra warning
**Solución:**
```razor
// Verificar que @if (!ListingId.HasValue) esté ANTES de else if (!submitted)
@if (!ListingId.HasValue)
{
    @* Warning *@
}
else if (!submitted)
{
    @* Formulario *@
}
```

### Problema 2: Textos no localizados
**Solución:**
```csharp
// Verificar inyección de IStringLocalizer
@using Microsoft.Extensions.Localization
@inject IStringLocalizer<cimaResource> L

// Verificar que archivo es.json tiene las claves
"ContactForm:Title": "¿Interesado en esta propiedad?"
```

### Problema 3: Scroll no funciona
**Solución:**
```csharp
// Verificar inyección de IJSRuntime
@inject IJSRuntime JS

// Verificar que sección tiene ID
<section id="featured-properties">

// Probar en Console:
document.getElementById('featured-properties')
// Debe retornar el elemento, no null
```

### Problema 4: Responsive no funciona
**Solución:**
```html
<!-- Verificar breakpoints de Tailwind -->
<img class="h-64 sm:h-80 lg:h-96" />
<!-- sm: min-width 640px -->
<!-- lg: min-width 1024px -->

<!-- Verificar que no hay CSS inline sobreescribiendo -->
<img style="height: 400px" /> <!-- ? MALO -->
```

---

## ?? DISPOSITIVOS RECOMENDADOS PARA TESTING

### Mobile
- iPhone SE (375px)
- iPhone 12/13/14 (390px)
- Samsung Galaxy S20 (360px)

### Tablet
- iPad (768px)
- iPad Pro (1024px)

### Desktop
- Laptop 13" (1280px)
- Desktop HD (1920px)
- Desktop 4K (3840px)

---

## ?? COMANDOS ÚTILES

```powershell
# Iniciar aplicación
cd src/cima.Blazor
dotnet run

# Ver logs en tiempo real
cd src/cima.Blazor
dotnet run --verbosity detailed

# Limpiar y rebuild
dotnet clean
dotnet build

# Verificar base de datos
docker exec -it cima-postgres psql -U postgres -d cima
SELECT * FROM "ContactRequests" ORDER BY "CreatedAt" DESC LIMIT 5;

# Ver commits recientes
git log --oneline -5

# Ver cambios de archivos
git show 5e7aa0f --stat
```

---

## ? CRITERIOS DE ACEPTACIÓN

Para considerar las mejoras como **APROBADAS**, todas estas condiciones deben cumplirse:

1. ? Formulario de contacto envía `ListingId` correcto al backend
2. ? Formulario muestra warning si falta propiedad
3. ? Todos los textos UI están localizados (no hardcodeados)
4. ? No existe CSS embebido en `Index.razor`
5. ? Galería de imágenes es responsive en 3 breakpoints
6. ? Miniaturas muestran 2/4/6 columnas según pantalla
7. ? Scroll suave funciona en mobile y desktop
8. ? Icono de scroll tiene animación bounce
9. ? Aplicación compila sin errores
10. ? Todas las pruebas manuales pasan

---

## ?? SOPORTE

Si encuentras problemas:
1. Revisar `docs/MEJORAS_UX_UI_COMPLETADAS.md` (guía técnica completa)
2. Verificar logs de compilación
3. Verificar logs de aplicación (Console)
4. Verificar base de datos (PostgreSQL)
5. Crear issue en repositorio

---

**Fecha:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
**Versión:** 1.0.0  
**Estado:** ? LISTO PARA TESTING
