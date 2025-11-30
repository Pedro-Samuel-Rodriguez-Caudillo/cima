# ?? REFACTORIZACIÓN DE COMPONENTES BLAZOR

## ?? **OBJETIVO**

Actualizar los componentes Blazor que usan `ListingImageDto.DisplayOrder` para usar la nueva estructura de lista enlazada.

---

## ?? **ARCHIVOS A MODIFICAR**

### 1?? **ListingCard.razor** (1 error)
**Ubicación:** `src/cima.Blazor.Client/Components/Public/ListingCard.razor`
**Línea:** 24

#### ? ANTES:
```razor
@listing.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url
```

#### ? DESPUÉS:
```razor
@listing.Images.FirstOrDefault(i => i.PreviousImageId == null)?.Url
```

**Explicación:** La primera imagen de la lista enlazada es la que tiene `PreviousImageId == null`.

---

### 2?? **ImageUploader.razor** (7 errores)
**Ubicación:** `src/cima.Blazor.Client/Components/Admin/ImageUploader.razor`

Este componente necesita refactorización completa porque maneja reordenamiento de imágenes.

#### **Cambios necesarios:**

##### **A. Mostrar imágenes en orden (línea 36):**
```razor
@* ANTES *@
@foreach (var image in Images.OrderBy(i => i.DisplayOrder))

@* DESPUÉS *@
@foreach (var image in GetImagesInOrder())

@code {
    private List<ListingImageDto> GetImagesInOrder()
    {
        var ordered = new List<ListingImageDto>();
        var current = Images.FirstOrDefault(i => i.PreviousImageId == null);
        
        while (current != null)
        {
            ordered.Add(current);
            current = Images.FirstOrDefault(i => i.ImageId == current.NextImageId);
        }
        
        return ordered;
    }
}
```

##### **B. Botones de reordenamiento (líneas 195, 223, 224, 241, 242, 254):**

```razor
@* ANTES - Botón "Mover arriba" *@
<button @onclick="() => MoveUp(image)" disabled="@(image.DisplayOrder == 1)">
    ? Mover arriba
</button>

@* DESPUÉS *@
<button @onclick="() => MoveUp(image)" disabled="@(image.PreviousImageId == null)">
    ? Mover arriba
</button>

@* ANTES - Botón "Mover abajo" *@
<button @onclick="() => MoveDown(image)" disabled="@(image.DisplayOrder == Images.Count)">
    ? Mover abajo
</button>

@* DESPUÉS *@
<button @onclick="() => MoveDown(image)" disabled="@(image.NextImageId == null)">
    ? Mover abajo
</button>
```

##### **C. Métodos de reordenamiento:**

```csharp
@code {
    // ? ELIMINAR métodos antiguos:
    // private void MoveUp(ListingImageDto image)
    // private void MoveDown(ListingImageDto image)
    
    // ? NUEVOS métodos con lista enlazada:
    
    private void MoveUp(ListingImageDto image)
    {
        // Si es la primera, no se puede mover
        if (image.PreviousImageId == null) return;
        
        var previous = Images.First(i => i.ImageId == image.PreviousImageId);
        var previousPrevious = Images.FirstOrDefault(i => i.ImageId == previous.PreviousImageId);
        var next = Images.FirstOrDefault(i => i.ImageId == image.NextImageId);
        
        // Reconfigurar enlaces
        // 1. La imagen anterior apunta a la siguiente de la imagen actual
        previous.NextImageId = image.NextImageId;
        
        // 2. Si existe siguiente, su anterior ahora es la imagen anterior
        if (next != null)
        {
            next.PreviousImageId = previous.ImageId;
        }
        
        // 3. La imagen actual apunta a la anterior-anterior
        image.PreviousImageId = previousPrevious?.ImageId;
        image.NextImageId = previous.ImageId;
        
        // 4. La imagen anterior ahora apunta a la actual
        previous.PreviousImageId = image.ImageId;
        
        // 5. Si existe anterior-anterior, su siguiente es la imagen actual
        if (previousPrevious != null)
        {
            previousPrevious.NextImageId = image.ImageId;
        }
        
        // TODO: Llamar a API para guardar cambios
        StateHasChanged();
    }
    
    private void MoveDown(ListingImageDto image)
    {
        // Si es la última, no se puede mover
        if (image.NextImageId == null) return;
        
        var next = Images.First(i => i.ImageId == image.NextImageId);
        
        // Intercambiar con la siguiente (similar a MoveUp pero invertido)
        MoveUp(next); // Mover la siguiente hacia arriba = mover actual hacia abajo
        
        StateHasChanged();
    }
}
```

---

## ?? **OPCIÓN ALTERNATIVA: SIMPLIFICADA**

Si no necesitas reordenamiento por ahora, puedes **temporalmente deshabilitar** esa funcionalidad:

```razor
@* En ImageUploader.razor *@

@* Comentar botones de reordenamiento *@
@* TODO: Refactorizar para lista enlazada *@
@* 
<button @onclick="() => MoveUp(image)">?</button>
<button @onclick="() => MoveDown(image)">?</button>
*@

@* Mostrar solo en orden *@
@foreach (var image in GetImagesInOrder())
{
    <div class="image-item">
        <img src="@image.Url" alt="@image.AltText" />
        <button @onclick="() => RemoveImage(image)">Eliminar</button>
    </div>
}
```

---

## ?? **PASOS PARA APLICAR**

### **Paso 1: Corregir ListingCard.razor**
```bash
# Abrir archivo
code src/cima.Blazor.Client/Components/Public/ListingCard.razor

# Buscar línea 24 y cambiar:
# OrderBy(i => i.DisplayOrder) ? FirstOrDefault(i => i.PreviousImageId == null)
```

### **Paso 2: Corregir ImageUploader.razor**

#### **Opción A: Implementación completa**
1. Agregar método `GetImagesInOrder()`
2. Actualizar foreach para usar `GetImagesInOrder()`
3. Actualizar condiciones de botones disabled
4. Refactorizar métodos `MoveUp` y `MoveDown`

#### **Opción B: Temporal - Sin reordenamiento**
1. Agregar método `GetImagesInOrder()`
2. Comentar botones de reordenamiento
3. Mostrar imágenes solo en lectura

### **Paso 3: Compilar**
```powershell
dotnet build src/cima.Blazor.Client/
```

### **Paso 4: Ejecutar aplicación**
```powershell
cd src/cima.Blazor
dotnet run
```

---

## ? **QUICK FIX - PARA COMPILAR YA**

Si solo quieres que compile para poder continuar:

```razor
@* En ListingCard.razor línea 24 *@
@listing.Images.FirstOrDefault()?.Url

@* En ImageUploader.razor - comentar sección de reordenamiento *@
@* TODO: Refactorizar DisplayOrder a lista enlazada
@if (false)  // Temporalmente deshabilitado
{
    <div class="reorder-buttons">
        ... botones de reordenamiento ...
    </div>
}
*@
```

---

## ? **VERIFICACIÓN**

Después de aplicar los cambios, verifica:

```powershell
# 1. Compilar sin errores
dotnet build

# 2. Ejecutar aplicación
cd src/cima.Blazor
dotnet run

# 3. Navegar a:
https://localhost:7200/

# 4. Verificar que:
- [ ] Las imágenes se muestran
- [ ] No hay errores en consola de navegador
- [ ] La aplicación carga correctamente
```

---

## ?? **DOCUMENTACIÓN DE REFERENCIA**

### **Cómo funciona la lista enlazada:**

```
Imagen 1 (First)          Imagen 2 (Middle)         Imagen 3 (Last)
???????????????????      ???????????????????      ???????????????????
? PreviousImageId ???????? PreviousImageId ???????? PreviousImageId ?
?      = null     ?      ?   = Image1.Id   ?      ?   = Image2.Id   ?
???????????????????      ???????????????????      ???????????????????
?  NextImageId    ????????  NextImageId    ????????  NextImageId    ?
?   = Image2.Id   ?      ?   = Image3.Id   ?      ?      = null     ?
???????????????????      ???????????????????      ???????????????????
```

### **Ventajas:**
- ? No hay conflictos de orden (DisplayOrder duplicados)
- ? Inserción/eliminación O(1)
- ? Reordenamiento eficiente
- ? Orden consistente

---

**Creado:** $(Get-Date -Format "dd/MM/yyyy HH:mm")
**Prioridad:** ?? MEDIA (componentes Blazor no críticos)
**Tiempo estimado:** 30-60 minutos (completo) / 5 minutos (quick fix)
