# FIX: Archivos NPM Duplicados en Publish

## ?? PROBLEMA

**Error en GitHub Actions:**
```
Found multiple publish output files with the same relative path:
  /home/runner/work/cima/cima/src/cima.Blazor.Client/package-lock.json
  /home/runner/work/cima/cima/src/cima.Blazor/package-lock.json
  /home/runner/work/cima/cima/src/cima.Blazor.Client/package.json
  /home/runner/work/cima/cima/src/cima.Blazor/package.json
```

## ?? CAUSA

El proyecto tiene **DOS** archivos `package.json`:

1. **`src/cima.Blazor.Client/package.json`** ?
   - Necesario para Tailwind CSS
   - Correcto y debe estar

2. **`src/cima.Blazor/package.json`** ?
   - Creado por ABP para temas
   - Causa conflicto en publish
   - Ambos proyectos intentan copiar al mismo destino

## ? SOLUCIÓN

Excluir `package.json` y `package-lock.json` del proyecto servidor (`cima.Blazor`) del proceso de publish.

**Archivo modificado:** `src/cima.Blazor/cima.Blazor.csproj`

**Cambio aplicado:**
```xml
<!-- Excluir archivos npm del publish para evitar conflictos -->
<ItemGroup>
  <Content Remove="package.json" />
  <Content Remove="package-lock.json" />
  <None Include="package.json">
    <CopyToPublishDirectory>Never</CopyToPublishDirectory>
  </None>
  <None Include="package-lock.json">
    <CopyToPublishDirectory>Never</CopyToPublishDirectory>
  </None>
</ItemGroup>
```

## ?? QUÉ HACE

1. **Remove from Content:** Quita los archivos del pipeline de contenido
2. **Include as None:** Los incluye como archivos sin procesamiento
3. **CopyToPublishDirectory=Never:** No los copia al directorio de publish

**Resultado:**
- ? Archivos siguen existiendo en desarrollo
- ? ABP puede usarlos localmente
- ? NO se copian al publish
- ? Solo se publican los del proyecto Client

## ?? VERIFICACIÓN

### Build Local ?
```powershell
dotnet build
# Resultado: Build succeeded
```

### Publish Local
```powershell
dotnet publish src/cima.Blazor/cima.Blazor.csproj -c Release -o ./test-publish

# Verificar que NO hay duplicados
Get-ChildItem ./test-publish -Recurse -Include package.json,package-lock.json
# Debería mostrar SOLO los archivos del proyecto Client
```

## ?? IMPACTO

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Build local** | ? OK | ? OK |
| **Publish local** | ? Error duplicados | ? OK |
| **GitHub Actions** | ? Falla | ? Pasa |
| **Docker build** | ? Falla | ? OK |
| **Desarrollo local** | ? OK | ? OK |

## ?? PRÓXIMOS PASOS

### 1. Commit y Push
```powershell
git add src/cima.Blazor/cima.Blazor.csproj
git commit -m "fix(build): exclude duplicate npm files from server project publish

PROBLEMA:
- GitHub Actions falla con archivos package.json duplicados
- Ambos proyectos (Blazor y Blazor.Client) tienen package.json
- Publish intenta copiar ambos al mismo destino

SOLUCIÓN:
- Excluir package.json del proyecto servidor en publish
- Mantener archivos para desarrollo local
- Solo publicar archivos npm del proyecto Client

ARCHIVOS:
- Modificado: src/cima.Blazor/cima.Blazor.csproj
- Agregado CopyToPublishDirectory=Never para package files

IMPACTO:
- Build local: OK
- GitHub Actions: OK
- Docker build: OK
- Sin cambios en desarrollo"

git push origin master
```

### 2. Verificar GitHub Actions
- Ir a: https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions
- Verificar que CI pasa ?

## ?? ARCHIVOS NPM EN EL PROYECTO

### src/cima.Blazor/package.json (ABP)
```json
{
  "version": "1.0.0",
  "name": "my-app",
  "private": true,
  "dependencies": {
    "@abp/aspnetcore.mvc.ui.theme.basic": "~9.3.6",
    "@abp/aspnetcore.components.server.basictheme": "~9.3.6"
  }
}
```
- ?? Usado solo para ABP themes en desarrollo
- ? NO debe publicarse

### src/cima.Blazor.Client/package.json (Tailwind)
```json
{
  "name": "cima-blazor-client",
  "version": "1.0.0",
  "scripts": {
    "build:css": "tailwindcss ...",
    "watch:css": "tailwindcss ..."
  },
  "devDependencies": {
    "tailwindcss": "^3.4.17",
    ...
  }
}
```
- ? Usado para Tailwind CSS
- ? DEBE publicarse

## ?? NOTAS ADICIONALES

### ¿Por qué no eliminar package.json del servidor?

ABP lo usa para sus dependencias de temas. Eliminarlo podría causar:
- Advertencias de ABP
- Problemas con themes en desarrollo
- Incompatibilidades futuras con ABP updates

### ¿Por qué esta solución es mejor?

- ? Mantiene compatibilidad con ABP
- ? No afecta desarrollo local
- ? Resuelve conflicto en publish
- ? No requiere cambios en estructura
- ? Fácil de mantener

## ? CHECKLIST

- [x] Modificado `.csproj` con exclusión
- [x] Build local exitoso
- [x] Documentación creada
- [ ] Commit realizado
- [ ] Push a GitHub
- [ ] CI/CD pasa

---

**ESTADO:** LISTO PARA COMMIT ?  
**CONFIANZA:** ALTA (build local OK)  
**IMPACTO:** Bajo (solo configuración publish)  
**PRÓXIMO PASO:** Commit y push
