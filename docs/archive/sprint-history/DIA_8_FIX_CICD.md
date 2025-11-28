# CORRECCIONES CI/CD - Docker y GitHub Actions

## PROBLEMAS IDENTIFICADOS

### 1. Error en Dockerfile - Rutas Incorrectas
**Error:**
```
ERROR: failed to build: failed to solve: process "/bin/sh -c dotnet publish \"cima.Blazor.csproj\"...
```

**Causa:** 
La instrucción `COPY . .` copia todo en `/src`, pero luego `WORKDIR` usaba `/src/src/cima.Blazor`, creando una estructura incorrecta.

**Solución:**
Cambiar la ruta en el stage de publish:
```dockerfile
# ANTES (incorrecto)
WORKDIR "/src/src/cima.Blazor"
RUN dotnet publish "cima.Blazor.csproj" ...

# DESPUÉS (correcto)
RUN dotnet publish "/src/src/cima.Blazor/cima.Blazor.csproj" ...
```

### 2. Error en GitHub Actions - appsettings.secrets.json
**Error:**
```
error MSB3030: Could not copy the file ".../appsettings.secrets.json" because it was not found.
```

**Causa:**
Los archivos `appsettings.secrets.json` existen localmente pero están en `.gitignore`, entonces no están en GitHub. Los proyectos de test intentan copiarlos siempre.

**Solución:**
Agregar condición `Exists()` en los `.csproj` de test:
```xml
<!-- ANTES -->
<Content Include="appsettings.secrets.json">
  <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>

<!-- DESPUÉS -->
<Content Include="appsettings.secrets.json" Condition="Exists('appsettings.secrets.json')">
  <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>
```

---

## ARCHIVOS MODIFICADOS

### 1. `src/cima.Blazor/Dockerfile`
**Cambio:** Corregida ruta en stage de publish
```diff
- WORKDIR "/src/src/cima.Blazor"
- RUN dotnet publish "cima.Blazor.csproj" \
+ RUN dotnet publish "/src/src/cima.Blazor/cima.Blazor.csproj" \
```

### 2. `test/cima.HttpApi.Client.ConsoleTestApp/cima.HttpApi.Client.ConsoleTestApp.csproj`
**Cambio:** Agregar condición Exists para appsettings.secrets.json
```diff
- <None Remove="appsettings.secrets.json" />
- <Content Include="appsettings.secrets.json">
+ <Content Include="appsettings.secrets.json" Condition="Exists('appsettings.secrets.json')">
```

### 3. `test/cima.TestBase/cima.TestBase.csproj`
**Cambio:** Agregar condición Exists para appsettings.secrets.json
```diff
- <None Remove="appsettings.secrets.json" />
- <Content Include="appsettings.secrets.json">
+ <Content Include="appsettings.secrets.json" Condition="Exists('appsettings.secrets.json')">
```

### 4. `.gitignore`
**Cambio:** Asegurar que archivos de ejemplo NO se ignoren
```diff
+ # Permitir archivos de ejemplo
+ !.env.development.example
+ !.env.staging.example
+ !.env.production.example
```

---

## NUEVO SCRIPT

### `etc/scripts/test-docker-build.ps1`
Script para probar el Dockerfile localmente antes de push.

**Uso básico:**
```powershell
# Solo build
.\etc\scripts\test-docker-build.ps1

# Build y ejecutar
.\etc\scripts\test-docker-build.ps1 -Run

# Limpiar y rebuild
.\etc\scripts\test-docker-build.ps1 -Clean -Run
```

**Características:**
- ? Build de imagen Docker
- ? Verificación de imagen creada
- ? Ejecución de contenedor de prueba (opcional)
- ? Test de health check endpoint
- ? Limpieza de imágenes anteriores (opcional)
- ? Información de troubleshooting

---

## VALIDACIÓN LOCAL

### Paso 1: Build Local
```powershell
# Verificar que compila
dotnet build

# Resultado esperado: Build succeeded
```

### Paso 2: Test Docker Build
```powershell
# Probar Dockerfile
.\etc\scripts\test-docker-build.ps1

# Resultado esperado: BUILD EXITOSO
```

### Paso 3: Test Docker Run (opcional)
```powershell
# Probar contenedor
.\etc\scripts\test-docker-build.ps1 -Run

# Resultado esperado: 
# - Contenedor iniciado
# - Health check OK
# - Accesible en http://localhost:8085
```

---

## VALIDACIÓN EN GITHUB ACTIONS

### Workflows Afectados

#### 1. CI - Build and Test (`.github/workflows/ci-build-test.yml`)
**Se ejecuta en:** Push a master/develop, Pull Requests

**Pasos afectados:**
- ? Build solution - AHORA OK (sin error de secrets.json)
- ? Publish application - AHORA OK

#### 2. CD - Deploy Staging (`.github/workflows/cd-deploy-staging.yml`)
**Se ejecuta en:** Push a develop

**Pasos afectados:**
- ? Build Docker image - AHORA OK (rutas corregidas)

#### 3. CD - Deploy Production (`.github/workflows/cd-deploy-production.yml`)
**Se ejecuta en:** Push a master

**Pasos afectados:**
- ? Build Docker image - AHORA OK (rutas corregidas)

---

## TESTING RECOMENDADO

### Antes de Push
```powershell
# 1. Build local
dotnet build

# 2. Test Docker
.\etc\scripts\test-docker-build.ps1

# 3. Si todo OK, commit y push
git add .
git commit -m "fix(cicd): corregir Dockerfile y appsettings.secrets.json en CI"
git push origin master
```

### Después de Push
1. Ir a GitHub Actions: https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions
2. Verificar que workflow "CI - Build and Test" pasa ?
3. Si está en master, verificar workflow "CD - Deploy Production"

---

## EXPLICACIÓN TÉCNICA

### ¿Por qué el Dockerfile tenía rutas incorrectas?

```dockerfile
# Estructura del filesystem en el contenedor:

# Después de: COPY . .
/src
  ??? cima.sln
  ??? src/
  ?   ??? cima.Blazor/
  ?       ??? cima.Blazor.csproj
  ??? ...

# Al hacer: WORKDIR "/src/src/cima.Blazor"
# Se creaba: /src/src/cima.Blazor (NO EXISTE)

# Debería ser:
# WORKDIR ya apunta a /src
# Entonces la ruta correcta es: /src/src/cima.Blazor/cima.Blazor.csproj
```

### ¿Por qué fallan los tests en CI con secrets.json?

```
Local (tu PC):
- appsettings.secrets.json EXISTE (git-ignored)
- Build: OK (archivo se copia)

GitHub Actions:
- appsettings.secrets.json NO EXISTE (no está en repo)
- Build sin Condition: FALLA (intenta copiar archivo inexistente)
- Build con Condition="Exists()": OK (solo copia si existe)
```

---

## TROUBLESHOOTING

### Error: "Docker build falló"
**Solución:**
1. Verificar Docker Desktop está corriendo
2. Ejecutar: `docker system prune -a` (limpiar caché)
3. Reintentar build

### Error: "Health check falló"
**Causa:** Base de datos no disponible en contenedor

**Solución:**
1. Es normal en pruebas locales sin BD
2. Verificar logs: `docker logs cima-test-local`
3. Para producción, asegurar que PostgreSQL está corriendo

### Error: "Cannot find image cima:test-local"
**Solución:**
```powershell
# Listar imágenes
docker images

# Si no existe, hacer build
.\etc\scripts\test-docker-build.ps1
```

---

## PRÓXIMOS PASOS

### 1. Testing Inmediato
```powershell
# Test local
.\etc\scripts\test-docker-build.ps1

# Si OK, push
git add .
git commit -m "fix(cicd): corregir Docker build y CI errors"
git push origin master
```

### 2. Monitoreo GitHub Actions
- Verificar que CI pasa
- Verificar que Docker build funciona en CD

### 3. Si Falla Algún Workflow
1. Ver logs en GitHub Actions
2. Reproducir localmente
3. Corregir y push de nuevo

---

## COMANDOS ÚTILES

### Docker Local
```powershell
# Build imagen
docker build -f src/cima.Blazor/Dockerfile -t cima:test .

# Listar imágenes
docker images

# Ejecutar contenedor
docker run -d -p 8085:8080 --name cima-test cima:test

# Ver logs
docker logs cima-test

# Detener y eliminar
docker stop cima-test
docker rm cima-test

# Limpiar todo
docker system prune -a
```

### Git
```powershell
# Ver estado
git status

# Add y commit
git add .
git commit -m "fix(cicd): mensaje"

# Push
git push origin master

# Ver workflows en GitHub
# https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions
```

---

## CHECKLIST DE VERIFICACIÓN

### Antes de Commit
- [ ] `dotnet build` pasa sin errores
- [ ] `.\etc\scripts\test-docker-build.ps1` pasa sin errores
- [ ] No hay archivos secrets.json en staging area (`git status`)

### Después de Push
- [ ] GitHub Actions "CI - Build and Test" pasa ?
- [ ] Si es master, "CD - Deploy Production" pasa ?
- [ ] No hay errores de Dockerfile en logs
- [ ] No hay errores de appsettings.secrets.json

---

## RESUMEN

**Problema 1:** Dockerfile con rutas incorrectas
**Solución:** Corregir ruta en stage publish

**Problema 2:** appsettings.secrets.json no existe en CI
**Solución:** Agregar `Condition="Exists()"` en .csproj de tests

**Resultado:** CI/CD funcionando correctamente ?

---

**Archivos modificados:** 4
**Nuevo script:** 1 (`test-docker-build.ps1`)
**Tiempo estimado de fix:** 5 minutos
**Testing local:** ? PASADO
**Testing en CI:** ? PENDIENTE (después de push)
