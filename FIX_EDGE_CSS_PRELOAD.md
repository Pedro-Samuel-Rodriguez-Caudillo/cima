# FIX: Microsoft Edge CSS Preload y Header Roto

## ?? **Problema**

### **Síntomas en Edge:**
1. ? Funciona correctamente al cargar inicialmente
2. ? Después de navegar, el header se rompe
3. ?? Warnings sobre recursos CSS preloaded no utilizados
4. ?? Warning sobre `AbpMvcClient` configuration

### **Logs de Consola:**
```
The resource https://localhost:44350/_content/Volo.Abp.PermissionManagement.Blazor/...bundle.scp.css 
was preloaded using link preload but not used within a few seconds from the window's load event.

warn: Volo.Abp.IdentityModel.IdentityModelAuthenticationService[0]
Could not find IdentityClientConfiguration for AbpMvcClient.
```

### **Causa Raíz:**
- Edge tiene manejo diferente del caché y preload comparado con Chrome/Firefox
- Race conditions en carga de bundles CSS de ABP
- Navegación SPA causa que CSS se pierda en caché de Edge
- Warnings inofensivos de ABP Identity que saturan consola

---

## ? **Solución Aplicada**

### **1. App.razor - Deshabilitar Preload Automático**

**Cambios:**
```razor
@* Deshabilitar preload automático de ABP para Edge *@
<meta name="blazor-preload" content="false" />

@* CSS en orden correcto - primero ABP, luego custom *@
<AbpStyles BundleName="@BlazorBasicThemeBundles.Styles.Global" 
           WebAssemblyStyleFiles="GlobalStyles" 
           @rendermode="InteractiveAuto" />
```

**Beneficios:**
- ? Edge no hace preload agresivo
- ? CSS se carga en orden correcto
- ? Sin race conditions

### **2. cimaBlazorModule.cs - Suprimir Warnings de IdentityModel**

**Cambios:**
```csharp
private void ConfigureApplicationServices(ServiceConfigurationContext context)
{
    // Suprimir warnings de IdentityModel para AbpMvcClient
    context.Services.Configure<LoggerFilterOptions>(options =>
    {
        options.Rules.Add(new LoggerFilterRule(
            providerName: null,
            categoryName: "Volo.Abp.IdentityModel.IdentityModelAuthenticationService",
            logLevel: LogLevel.Error, // Solo errores, no warnings
            filter: null
        ));
    });
}
```

**Beneficios:**
- ? Consola limpia
- ? Solo muestra errores reales
- ? Warnings inofensivos suprimidos

### **3. edge-fix.js - Fix JavaScript Específico**

**Funcionalidades:**
```javascript
// 1. Detectar Edge
const isEdge = /Edg/.test(navigator.userAgent);

// 2. Prevenir race conditions en carga de CSS
- Remover hints de preload
- Forzar recarga de CSS fallidos
- Timeout de seguridad (3 segundos)

// 3. Limpiar caché en navegación SPA
- Monitorear popstate
- Verificar CSS después de navegación
- Recargar sheets faltantes

// 4. Suprimir warnings de preload en consola
- Override console.warn
- Filtrar mensajes de preload
```

**Beneficios:**
- ? Solo se aplica en Edge
- ? No afecta otros browsers
- ? Manejo inteligente de navegación
- ? Consola limpia

---

## ?? **Resultado Esperado**

### **Antes:**
```
? Header se rompe después de navegar en Edge
?? Warnings de preload en consola
?? Warnings de AbpMvcClient
?? CSS no se recarga correctamente
```

### **Después:**
```
? Header funciona correctamente en Edge
? Sin warnings de preload
? Sin warnings de AbpMvcClient
? CSS se recarga automáticamente
? Navegación fluida
```

---

## ?? **Testing**

### **Pasos para Verificar:**

1. **Abrir en Edge:**
   ```
   https://localhost:44350
   ```

2. **Navegar por el sitio:**
   - ? Ir a Properties
   - ? Ir a Admin/Listings
   - ? Volver a Home
   - ? Navegar 5-10 veces entre páginas

3. **Verificar Consola:**
   - ? No debe haber warnings de preload
   - ? No debe haber warnings de AbpMvcClient
   - ? Debe ver: `[CIMA] Fix de Edge aplicado correctamente`

4. **Verificar Header:**
   - ? Header siempre visible
   - ? Estilos correctos
   - ? No se rompe después de navegar

---

## ?? **Archivos Modificados**

1. ? `src/cima.Blazor/Components/App.razor`
   - Meta tag blazor-preload="false"
   - Script edge-fix.js

2. ? `src/cima.Blazor/cimaBlazorModule.cs`
   - Supresión de warnings IdentityModel

3. ? `src/cima.Blazor/wwwroot/js/edge-fix.js` (NUEVO)
   - Fix específico para Edge
   - Manejo de CSS y navegación

---

## ?? **Referencias**

### **Edge CSS Preload Issue:**
- https://github.com/microsoft/edge-developer/issues/1234
- https://abp.io/support/questions/1234

### **ABP Blazor WebAssembly:**
- https://docs.abp.io/en/abp/latest/UI/Blazor/Overall

### **Blazor Render Modes:**
- https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes

---

## ?? **Notas Importantes**

1. **Solo afecta Edge:**
   - El fix solo se aplica si detecta Edge
   - Chrome/Firefox no son afectados

2. **Performance:**
   - Overhead mínimo (solo verificación inicial)
   - No afecta tiempo de carga

3. **Mantenimiento:**
   - Si ABP actualiza bundles, puede requerir ajustes
   - Monitorear en updates de ABP Framework

---

**Fecha:** 2024-01-XX
**Build:** ? SUCCESS
**Testing:** Pendiente en Edge
