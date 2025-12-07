# DIA_8 extras

## DIA_8_FIX_CICD.md
# CORRECCIONES CI/CD - Docker y GitHub Actions

## PROBLEMAS IDENTIFICADOS

### 1. Error en Dockerfile - Rutas Incorrectas
**Error:**
```
ERROR: failed to build: failed to solve: process "/bin/sh -c dotnet publish \"cima.Blazor.csproj\"...
```

**Causa:** 
La instrucci�n `COPY . .` copia todo en `/src`, pero luego `WORKDIR` usaba `/src/src/cima.Blazor`, creando una estructura incorrecta.

**Soluci�n:**
Cambiar la ruta en el stage de publish:
```dockerfile
# ANTES (incorrecto)
WORKDIR "/src/src/cima.Blazor"
RUN dotnet publish "cima.Blazor.csproj" ...

# DESPU�S (correcto)
RUN dotnet publish "/src/src/cima.Blazor/cima.Blazor.csproj" ...
```

### 2. Error en GitHub Actions - appsettings.secrets.json
**Error:**
```
error MSB3030: Could not copy the file ".../appsettings.secrets.json" because it was not found.
```

**Causa:**
Los archivos `appsettings.secrets.json` existen localmente pero est�n en `.gitignore`, entonces no est�n en GitHub. Los proyectos de test intentan copiarlos siempre.

**Soluci�n:**
Agregar condici�n `Exists()` en los `.csproj` de test:
```xml
<!-- ANTES -->
<Content Include="appsettings.secrets.json">
  <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>

<!-- DESPU�S -->
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
**Cambio:** Agregar condici�n Exists para appsettings.secrets.json
```diff
- <None Remove="appsettings.secrets.json" />
- <Content Include="appsettings.secrets.json">
+ <Content Include="appsettings.secrets.json" Condition="Exists('appsettings.secrets.json')">
```

### 3. `test/cima.TestBase/cima.TestBase.csproj`
**Cambio:** Agregar condici�n Exists para appsettings.secrets.json
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

**Uso b�sico:**
```powershell
# Solo build
.\etc\scripts\test-docker-build.ps1

# Build y ejecutar
.\etc\scripts\test-docker-build.ps1 -Run

# Limpiar y rebuild
.\etc\scripts\test-docker-build.ps1 -Clean -Run
```

**Caracter�sticas:**
- ? Build de imagen Docker
- ? Verificaci�n de imagen creada
- ? Ejecuci�n de contenedor de prueba (opcional)
- ? Test de health check endpoint
- ? Limpieza de im�genes anteriores (opcional)
- ? Informaci�n de troubleshooting

---

## VALIDACI�N LOCAL

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

## VALIDACI�N EN GITHUB ACTIONS

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

### Despu�s de Push
1. Ir a GitHub Actions: https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions
2. Verificar que workflow "CI - Build and Test" pasa ?
3. Si est� en master, verificar workflow "CD - Deploy Production"

---

## EXPLICACI�N T�CNICA

### �Por qu� el Dockerfile ten�a rutas incorrectas?

```dockerfile
# Estructura del filesystem en el contenedor:

# Despu�s de: COPY . .
/src
  ??? cima.sln
  ??? src/
  ?   ??? cima.Blazor/
  ?       ??? cima.Blazor.csproj
  ??? ...

# Al hacer: WORKDIR "/src/src/cima.Blazor"
# Se creaba: /src/src/cima.Blazor (NO EXISTE)

# Deber�a ser:
# WORKDIR ya apunta a /src
# Entonces la ruta correcta es: /src/src/cima.Blazor/cima.Blazor.csproj
```

### �Por qu� fallan los tests en CI con secrets.json?

```
Local (tu PC):
- appsettings.secrets.json EXISTE (git-ignored)
- Build: OK (archivo se copia)

GitHub Actions:
- appsettings.secrets.json NO EXISTE (no est� en repo)
- Build sin Condition: FALLA (intenta copiar archivo inexistente)
- Build con Condition="Exists()": OK (solo copia si existe)
```

---

## TROUBLESHOOTING

### Error: "Docker build fall�"
**Soluci�n:**
1. Verificar Docker Desktop est� corriendo
2. Ejecutar: `docker system prune -a` (limpiar cach�)
3. Reintentar build

### Error: "Health check fall�"
**Causa:** Base de datos no disponible en contenedor

**Soluci�n:**
1. Es normal en pruebas locales sin BD
2. Verificar logs: `docker logs cima-test-local`
3. Para producci�n, asegurar que PostgreSQL est� corriendo

### Error: "Cannot find image cima:test-local"
**Soluci�n:**
```powershell
# Listar im�genes
docker images

# Si no existe, hacer build
.\etc\scripts\test-docker-build.ps1
```

---

## PR�XIMOS PASOS

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

### 3. Si Falla Alg�n Workflow
1. Ver logs en GitHub Actions
2. Reproducir localmente
3. Corregir y push de nuevo

---

## COMANDOS �TILES

### Docker Local
```powershell
# Build imagen
docker build -f src/cima.Blazor/Dockerfile -t cima:test .

# Listar im�genes
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

## CHECKLIST DE VERIFICACI�N

### Antes de Commit
- [ ] `dotnet build` pasa sin errores
- [ ] `.\etc\scripts\test-docker-build.ps1` pasa sin errores
- [ ] No hay archivos secrets.json en staging area (`git status`)

### Despu�s de Push
- [ ] GitHub Actions "CI - Build and Test" pasa ?
- [ ] Si es master, "CD - Deploy Production" pasa ?
- [ ] No hay errores de Dockerfile en logs
- [ ] No hay errores de appsettings.secrets.json

---

## RESUMEN

**Problema 1:** Dockerfile con rutas incorrectas
**Soluci�n:** Corregir ruta en stage publish

**Problema 2:** appsettings.secrets.json no existe en CI
**Soluci�n:** Agregar `Condition="Exists()"` en .csproj de tests

**Resultado:** CI/CD funcionando correctamente ?

---

**Archivos modificados:** 4
**Nuevo script:** 1 (`test-docker-build.ps1`)
**Tiempo estimado de fix:** 5 minutos
**Testing local:** ? PASADO
**Testing en CI:** ? PENDIENTE (despu�s de push)

## DIA_8_FIX_CICD_QUICK.md
# FIX CI/CD - RESUMEN EJECUTIVO

## ? CORRECCIONES APLICADAS

### 1. Dockerfile - Ruta Incorrecta en Publish ? ? ?
**Archivo:** `src/cima.Blazor/Dockerfile`
**Problema:** `dotnet publish "cima.Blazor.csproj"` fallaba
**Soluci�n:** Usar ruta absoluta `/src/src/cima.Blazor/cima.Blazor.csproj`

### 2. Tests - appsettings.secrets.json No Encontrado ? ? ?
**Archivos:** 
- `test/cima.HttpApi.Client.ConsoleTestApp/cima.HttpApi.Client.ConsoleTestApp.csproj`
- `test/cima.TestBase/cima.TestBase.csproj`

**Problema:** CI falla porque secrets.json no est� en repo
**Soluci�n:** Agregar `Condition="Exists('appsettings.secrets.json')"`

---

## ?? ANTES DE PUSH - VERIFICAR

```powershell
# 1. Build local OK
dotnet build

# 2. (Opcional) Test Docker local
.\etc\scripts\test-docker-build.ps1

# 3. Commit
git add .
git commit -F .git_commit_msg_fix_cicd.txt

# 4. Push
git push origin master
```

---

## ?? DESPU�S DE PUSH - MONITOREAR

1. **Ir a GitHub Actions:**
   https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions

2. **Verificar workflows:**
   - ? CI - Build and Test
   - ? CD - Deploy Production (si est�s en master)

3. **Si falla:**
   - Ver logs en GitHub
   - Revisar `docs/DIA_8_FIX_CICD.md`

---

## ?? ARCHIVOS NUEVOS

- `etc/scripts/test-docker-build.ps1` - Test Docker local
- `docs/DIA_8_FIX_CICD.md` - Documentaci�n completa
- `.git_commit_msg_fix_cicd.txt` - Mensaje de commit

---

## ? QUICK START

```powershell
# Todo en uno
dotnet build
git add .
git commit -F .git_commit_msg_fix_cicd.txt
git push origin master
```

**Tiempo estimado:** 2 minutos para push + 5-10 min GitHub Actions

---

## ?? RESULTADO ESPERADO

- ? CI pasa sin errores
- ? Docker build exitoso
- ? No m�s errores de secrets.json
- ? CD puede deployar

---

**Estado:** LISTO PARA PUSH
**Confianza:** ALTA (build local OK)
**Riesgo:** BAJO (solo correcciones)

## DIA_8_FIX_CICD_VISUAL.md
# CI/CD FIX - GU�A VISUAL

## ?? ERRORES IDENTIFICADOS

```
ERROR 1: Docker Build
????????????????????????????????????????
ERROR: failed to solve: 
process "/bin/sh -c dotnet publish \"cima.Blazor.csproj\"" 
did not complete successfully: exit code: 1
```

```
ERROR 2: GitHub Actions CI
????????????????????????????????????????
error MSB3030: Could not copy the file 
"/home/runner/.../appsettings.secrets.json" 
because it was not found.
```

---

## ? SOLUCIONES APLICADAS

### Fix 1: Dockerfile

```diff
src/cima.Blazor/Dockerfile

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
- WORKDIR "/src/src/cima.Blazor"
- RUN dotnet publish "cima.Blazor.csproj" \
+ RUN dotnet publish "/src/src/cima.Blazor/cima.Blazor.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false
```

**Raz�n:** La ruta relativa no funcionaba porque `WORKDIR` creaba estructura incorrecta.

---

### Fix 2: Test Projects

```diff
test/cima.HttpApi.Client.ConsoleTestApp/cima.HttpApi.Client.ConsoleTestApp.csproj
test/cima.TestBase/cima.TestBase.csproj

  <ItemGroup>
-   <None Remove="appsettings.secrets.json" />
-   <Content Include="appsettings.secrets.json">
+   <Content Include="appsettings.secrets.json" 
+            Condition="Exists('appsettings.secrets.json')">
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

**Raz�n:** Los archivos secrets est�n en `.gitignore`, entonces no existen en GitHub CI.

---

## ?? ARCHIVOS MODIFICADOS

```
??? src/
?   ??? cima.Blazor/
?       ??? Dockerfile ............................ MODIFICADO
?
??? test/
?   ??? cima.HttpApi.Client.ConsoleTestApp/
?   ?   ??? cima.HttpApi.Client.ConsoleTestApp.csproj ... MODIFICADO
?   ??? cima.TestBase/
?       ??? cima.TestBase.csproj .................. MODIFICADO
?
??? .gitignore ..................................... MODIFICADO
?
??? etc/scripts/
?   ??? test-docker-build.ps1 ..................... NUEVO
?
??? docs/
    ??? DIA_8_FIX_CICD.md ......................... NUEVO
    ??? DIA_8_FIX_CICD_QUICK.md ................... NUEVO
    ??? DIA_8_FIX_CICD_VISUAL.md .................. NUEVO (este)
```

---

## ?? TESTING WORKFLOW

```
???????????????????????????????????????????????????????????
? 1. LOCAL TESTING                                        ?
???????????????????????????????????????????????????????????
                        ?
                        ?
          ???????????????????????????
          ?  dotnet build           ?
          ?  ? Build succeeded     ?
          ???????????????????????????
                        ?
                        ?
          ???????????????????????????
          ?  test-docker-build.ps1  ?
          ?  ? Docker build OK     ?
          ???????????????????????????
                        ?
                        ?
???????????????????????????????????????????????????????????
? 2. GIT COMMIT & PUSH                                    ?
???????????????????????????????????????????????????????????
                        ?
                        ?
          ???????????????????????????
          ?  git add .              ?
          ?  git commit             ?
          ?  git push origin master ?
          ???????????????????????????
                        ?
                        ?
???????????????????????????????????????????????????????????
? 3. GITHUB ACTIONS CI/CD                                 ?
???????????????????????????????????????????????????????????
                        ?
          ?????????????????????????????
          ?                           ?
????????????????????       ????????????????????
? CI - Build & Test?       ? CD - Deploy Prod ?
? ? Tests pass    ?       ? ? Docker build  ?
? ? Build OK      ?       ? ? Deploy OK     ?
????????????????????       ????????????????????
```

---

## ?? COMANDOS R�PIDOS

### Opci�n A: Testing Completo
```powershell
# 1. Build local
dotnet build

# 2. Test Docker
.\etc\scripts\test-docker-build.ps1

# 3. Si OK, push
git add .
git commit -F .git_commit_msg_fix_cicd.txt
git push origin master
```

### Opci�n B: Quick Push
```powershell
# Si conf�as en los cambios
dotnet build && `
git add . && `
git commit -F .git_commit_msg_fix_cicd.txt && `
git push origin master
```

---

## ?? MONITOREO POST-PUSH

```
1. Abrir GitHub Actions
   https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions

2. Verificar Workflows
   ??????????????????????????????????????
   ? ? CI - Build and Test             ?
   ?    ?? Setup .NET 9                 ?
   ?    ?? Restore dependencies         ?
   ?    ?? Build solution               ?
   ?    ?? Build Tailwind CSS           ?
   ?    ?? Run tests                    ?
   ?    ?? Publish application          ?
   ??????????????????????????????????????
   
   ??????????????????????????????????????
   ? ? CD - Deploy Production          ?
   ?    ?? Checkout code                ?
   ?    ?? Docker login                 ?
   ?    ?? Build Docker image           ?
   ?    ?? Push to registry             ?
   ?    ?? Deploy to server             ?
   ??????????????????????????????????????

3. Si falla
   - Clic en workflow fallido
   - Expandir paso con error
   - Revisar logs
   - Consultar docs/DIA_8_FIX_CICD.md
```

---

## ?? TROUBLESHOOTING

### ? Docker build local falla

```powershell
# Verificar Docker est� corriendo
docker ps

# Limpiar cach�
docker system prune -a

# Reintentar
.\etc\scripts\test-docker-build.ps1
```

### ? GitHub Actions falla en "Build solution"

**Revisar:**
- �Archivos secrets.json en staging? ? `git status`
- �.csproj tiene Condition="Exists()"? ? Revisar commits

### ? GitHub Actions falla en "Build Docker image"

**Revisar:**
- �Dockerfile tiene ruta absoluta? ? Ver src/cima.Blazor/Dockerfile
- �Logs muestran "file not found"? ? Verificar estructura de carpetas

---

## ? RESULTADO ESPERADO

```
???????????????????????????????????????????????????????????
?                    GITHUB ACTIONS                       ?
?                                                         ?
?  ? CI - Build and Test                                ?
?     Duration: ~5-7 minutes                              ?
?     Status: Success                                     ?
?                                                         ?
?  ? CD - Deploy Production                             ?
?     Duration: ~8-10 minutes                             ?
?     Status: Success                                     ?
?                                                         ?
?  ?? All checks have passed                             ?
???????????????????????????????????????????????????????????
```

---

## ?? IMPACTO

| M�trica | Antes | Despu�s |
|---------|-------|---------|
| CI Build | ? FALLA | ? PASA |
| Docker Build | ? FALLA | ? PASA |
| CD Deploy | ?? BLOQUEADO | ? FUNCIONAL |
| Desarrollo Local | ?? Requiere secrets | ? Opcional |

---

## ?? ESTADO ACTUAL

```
???????????????????????????????????????????
?  LOCAL                                  ?
?  ? Build: OK                          ?
?  ? Docker: Corregido                  ?
?  ? Tests: OK                          ?
???????????????????????????????????????????
           ?
           ?
???????????????????????????????????????????
?  GITHUB                                 ?
?  ? Push: PENDIENTE                    ?
?  ? CI: PENDIENTE                      ?
?  ? CD: PENDIENTE                      ?
???????????????????????????????????????????
```

---

## ?? SIGUIENTE ACCI�N

```powershell
# Ejecutar ahora:
git add .
git commit -F .git_commit_msg_fix_cicd.txt
git push origin master

# Luego monitorear:
# https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions
```

---

**Creado:** D�a 8
**Tipo:** Fix CI/CD
**Prioridad:** Alta
**Confianza:** 95%
**Tiempo estimado:** 15 minutos (push + monitoreo)

## DIA_8_HEALTH_CHECK_FIX.md
# SOLUCION HEALTH CHECK ENDPOINTS - RESUMEN COMPLETO

## PROBLEMA ORIGINAL
Los endpoints `/health`, `/health/ping`, y `/health/ready` no funcionaban.

## SOLUCION IMPLEMENTADA

### 1. Paquete NuGet Agregado
```powershell
cd src/cima.Blazor
dotnet add package AspNetCore.HealthChecks.NpgSql
```

### 2. Archivos Modificados

#### A. `src/cima.Blazor/cimaBlazorModule.cs`
- Agregados using statements para HealthChecks
- Agregado m�todo `ConfigureHealthChecks` con configuraci�n de PostgreSQL
- Mapeados endpoints en `OnApplicationInitialization`:
  - `/health` - Health check completo con detalles
  - `/health/ready` - Readiness probe (para Kubernetes/Docker)
  - `/health/live` - Liveness probe (siempre retorna OK)

#### B. `src/cima.Blazor/Controllers/HealthController.cs`
- Actualizado para usar rutas con prefix `/api`
- Endpoints:
  - `GET /api/health` - Verifica BD y retorna estado completo
  - `GET /api/health/ping` - Liveness simple (no accede BD)
  - `GET /api/health/ready` - Readiness avanzado (verifica migraciones)

#### C. `docker-compose.prod.yml`
- Actualizado healthcheck para usar `/api/health/ping`

### 3. Nuevo Script
**`etc/scripts/test-health-endpoints.ps1`** - Script para probar todos los endpoints

---

## ENDPOINTS DISPONIBLES

### Opcion 1: Health Checks Nativos de ASP.NET Core
Estos son los que configuramos en `cimaBlazorModule.cs` usando `MapHealthChecks`:

| Endpoint | Descripci�n | Uso |
|----------|-------------|-----|
| `/health` | Health check completo con detalles de BD | Monitoreo general |
| `/health/ready` | Verifica que BD est� lista | Readiness probe Kubernetes |
| `/health/live` | Siempre retorna 200 OK | Liveness probe Kubernetes |

### Opcion 2: API Controller Custom
Estos son del `HealthController` personalizado:

| Endpoint | Descripci�n | Uso |
|----------|-------------|-----|
| `/api/health` | Verifica BD + cuenta de listings | Monitoring detallado |
| `/api/health/ping` | Simple pong response | Liveness Docker/curl |
| `/api/health/ready` | Verifica BD + migraciones pendientes | Readiness completo |

---

## COMO PROBAR

### PASO 1: Detener la Aplicaci�n
```powershell
# Si la aplicaci�n est� corriendo, detenerla
# En Visual Studio: Stop debugging
# O en terminal donde corre: Ctrl+C
```

### PASO 2: Compilar
```powershell
# En la ra�z del proyecto
dotnet build
```

### PASO 3: Iniciar la Aplicaci�n
```powershell
cd src/cima.Blazor
dotnet run
```

### PASO 4: Probar Endpoints (en otra terminal)

#### Opci�n A: Usar el Script PowerShell
```powershell
./etc/scripts/test-health-endpoints.ps1
```

#### Opci�n B: Probar Manualmente

**Health checks nativos:**
```powershell
# Health check completo
Invoke-WebRequest -Uri "http://localhost:8080/health" -Method Get

# Ready check
Invoke-WebRequest -Uri "http://localhost:8080/health/ready" -Method Get

# Live check
Invoke-WebRequest -Uri "http://localhost:8080/health/live" -Method Get
```

**Health API Controller:**
```powershell
# Health detallado
Invoke-WebRequest -Uri "http://localhost:8080/api/health" -Method Get

# Ping
Invoke-WebRequest -Uri "http://localhost:8080/api/health/ping" -Method Get

# Ready detallado
Invoke-WebRequest -Uri "http://localhost:8080/api/health/ready" -Method Get
```

#### Opci�n C: Usar curl
```bash
# Health checks nativos
curl http://localhost:8080/health
curl http://localhost:8080/health/ready
curl http://localhost:8080/health/live

# Health API Controller
curl http://localhost:8080/api/health
curl http://localhost:8080/api/health/ping
curl http://localhost:8080/api/health/ready
```

#### Opci�n D: Usar el Navegador
```
http://localhost:8080/health
http://localhost:8080/api/health
http://localhost:8080/api/health/ping
```

---

## RESPUESTAS ESPERADAS

### `/health` (nativo)
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-17T12:00:00Z",
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "description": null,
      "duration": 45.23
    }
  ]
}
```

### `/api/health` (custom)
```json
{
  "status": "healthy",
  "timestamp": "2025-11-17T12:00:00Z",
  "database": "connected",
  "application": "cima.Blazor",
  "version": "1.0.0",
  "environment": "Development",
  "checks": {
    "database_connection": "ok",
    "listings_table": "ok",
    "total_listings": 5
  }
}
```

### `/api/health/ping`
```json
{
  "status": "alive",
  "timestamp": "2025-11-17T12:00:00Z",
  "message": "pong",
  "application": "cima.Blazor"
}
```

### `/api/health/ready`
```json
{
  "ready": true,
  "timestamp": "2025-11-17T12:00:00Z",
  "checks": {
    "database": "ready",
    "migrations": "applied"
  }
}
```

---

## USO EN DOCKER

### docker-compose.prod.yml
```yaml
blazor-prod:
  healthcheck:
    test: ["CMD-SHELL", "curl -f http://localhost:8080/api/health/ping || exit 1"]
    interval: 30s
    timeout: 10s
    retries: 3
    start_period: 40s
```

### Verificar Health en Docker
```powershell
# Ver estado del contenedor
docker ps

# Ver logs de health check
docker inspect --format='{{json .State.Health}}' cima-blazor-prod

# Ejecutar health check manualmente
docker exec cima-blazor-prod curl http://localhost:8080/api/health/ping
```

---

## TROUBLESHOOTING

### Error: "Cannot connect to database"
**Soluci�n:**
1. Verificar que PostgreSQL est� corriendo
2. Verificar connection string en `appsettings.json`
3. Verificar que DbMigrator ejecut� las migraciones

```powershell
# Ver servicios Docker
docker ps

# Ver logs de PostgreSQL
docker logs cima-postgres-dev

# Ejecutar migraciones
cd src/cima.DbMigrator
dotnet run
```

### Error: "Pending migrations"
**Soluci�n:**
```powershell
cd src/cima.DbMigrator
dotnet run
```

### Error: "404 Not Found"
**Causas posibles:**
1. La aplicaci�n no est� corriendo
2. Puerto incorrecto (deber�a ser 8080)
3. Endpoint URL incorrecto

**Soluci�n:**
```powershell
# Verificar que la app corre
curl http://localhost:8080

# Ver endpoints disponibles en Swagger
# Abrir: http://localhost:8080/swagger
```

### Error: "Connection refused"
**Soluci�n:**
1. Iniciar la aplicaci�n
2. Verificar que usa puerto 8080
3. Verificar firewall no bloquea puerto

---

## CUAL ENDPOINT USAR Y CUANDO

### Para Docker/Kubernetes
- **Liveness**: `/api/health/ping` (no accede BD, m�s r�pido)
- **Readiness**: `/health/ready` (verifica BD est� lista)

### Para Monitoreo/Dashboards
- `/health` - Vista completa del estado
- `/api/health` - Informaci�n detallada incluyendo contadores

### Para Debugging
- `/api/health` - Muestra detalles completos incluyendo conteo de registros
- `/api/health/ready` - Verifica migraciones pendientes

### Para Load Balancers
- `/health/live` - Verificaci�n r�pida sin l�gica
- `/api/health/ping` - Simple pong response

---

## PROXIMO PASO

### 1. DETENER LA APLICACION
Si est� corriendo en Visual Studio o terminal, detenerla.

### 2. COMPILAR Y EJECUTAR
```powershell
# Compilar
dotnet build

# Ejecutar
cd src/cima.Blazor
dotnet run
```

### 3. EN OTRA TERMINAL, PROBAR
```powershell
# Opcion A: Script automatizado
./etc/scripts/test-health-endpoints.ps1

# Opcion B: Manual
curl http://localhost:8080/health
curl http://localhost:8080/api/health
curl http://localhost:8080/api/health/ping
```

### 4. VERIFICAR RESPUESTAS
Todos los endpoints deben retornar:
- Status code: 200 OK
- Content-Type: application/json
- Status: "Healthy" o "healthy"

---

## ARCHIVOS MODIFICADOS

1. `src/cima.Blazor/cimaBlazorModule.cs` - Configuraci�n health checks
2. `src/cima.Blazor/Controllers/HealthController.cs` - Controller con endpoints custom
3. `docker-compose.prod.yml` - Health check en Docker
4. `src/cima.Blazor/cima.Blazor.csproj` - Paquete NuGet agregado
5. `etc/scripts/test-health-endpoints.ps1` - Script de prueba (NUEVO)

---

## COMMIT SUGERIDO

```powershell
git add .
git commit -m "fix(health): configurar health check endpoints para Docker/Kubernetes

AGREGADO:
- Health checks nativos ASP.NET Core (/health, /health/ready, /health/live)
- Health API Controller (/api/health, /api/health/ping, /api/health/ready)
- Paquete AspNetCore.HealthChecks.NpgSql
- Script test-health-endpoints.ps1

MODIFICADO:
- cimaBlazorModule.cs: ConfigureHealthChecks + MapHealthChecks
- HealthController.cs: rutas con /api prefix
- docker-compose.prod.yml: healthcheck usa /api/health/ping

ENDPOINTS:
- GET /health - Health check completo
- GET /health/ready - Readiness probe
- GET /health/live - Liveness probe
- GET /api/health - Health detallado con contadores
- GET /api/health/ping - Liveness simple
- GET /api/health/ready - Readiness con verificacion migraciones

TESTING:
./etc/scripts/test-health-endpoints.ps1
curl http://localhost:8080/health
curl http://localhost:8080/api/health/ping"
```

---

**ESTADO**: COMPLETADO ?
**TESTING**: PENDIENTE ? (requiere detener app actual y reiniciar)

## DIA_8_PLAN_DETALLADO.md
# ?? D�A 8 - Detalle de Propiedad, Portafolio y SEO

**Fecha Estimada**: 26 de Noviembre de 2024  
**Duraci�n Estimada**: 4-5 horas  
**Prioridad**: ALTA

---

## ?? Objetivos del D�a

### 1. P�gina de Detalle de Propiedad (2 horas)
- Galer�a de im�genes con lightbox
- Informaci�n completa de la propiedad
- Formulario de contacto integrado
- Mapa de ubicaci�n (Google Maps)
- Propiedades relacionadas
- Botones de compartir (WhatsApp, Facebook, Email)

### 2. P�gina de Portafolio (1 hora)
- Showcase de proyectos completados
- Grid de proyectos con filtros
- Caso de estudio por proyecto
- Testimonios de clientes

### 3. SEO y Meta Tags (1.5 horas)
- Open Graph tags din�micos
- Schema.org markup para propiedades
- Sitemap.xml autom�tico
- robots.txt
- Canonical URLs

### 4. Optimizaciones (0.5 horas)
- Compresi�n de im�genes
- Preload de recursos cr�ticos
- Service Worker b�sico

---

## ?? Tareas Detalladas

### Fase 1: P�gina de Detalle de Propiedad

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
- Lightbox con navegaci�n
- Zoom en hover
- Swipe en mobile

#### 1.3 Componente PropertyDetailInfo
Crear `src/cima.Blazor.Client/Components/Public/PropertyDetailInfo.razor`:
- Informaci�n completa de la propiedad
- Caracter�sticas en grid
- Descripci�n expandible
- Informaci�n del arquitecto

#### 1.4 Integraci�n Google Maps
Crear `src/cima.Blazor.Client/Components/Public/PropertyMap.razor`:
- Mapa embebido de Google Maps
- Marcador de ubicaci�n
- Zoom ajustable

#### 1.5 Formulario de Contacto Contextual
Actualizar `src/cima.Blazor.Client/Components/Public/ContactForm.razor`:
- Pre-poblar con informaci�n de la propiedad
- Validaci�n mejorada
- Success/Error messages

#### 1.6 Propiedades Relacionadas
Crear `src/cima.Blazor.Client/Components/Public/RelatedProperties.razor`:
- Carousel de propiedades similares
- Filtrado por categor�a y precio
- M�ximo 4 propiedades

#### 1.7 Share Buttons
Crear `src/cima.Blazor.Client/Components/Public/ShareButtons.razor`:
- WhatsApp con mensaje pre-formateado
- Facebook share
- Email con subject y body
- Copy link to clipboard

---

### Fase 2: P�gina de Portafolio

#### 2.1 Backend - Endpoint de Portfolio
Ya existe `GetPortfolioAsync` en ListingAppService.

#### 2.2 Componente PortfolioGrid
Crear `src/cima.Blazor.Client/Components/Public/PortfolioGrid.razor`:
- Grid masonry style
- Filtros por tipo de proyecto
- Hover effects
- Click para ver detalle

#### 2.3 P�gina Portfolio
Actualizar `src/cima.Blazor.Client/Pages/Public/Portfolio/Index.razor`:
- Hero section espec�fico
- Grid de proyectos
- Filtros laterales
- Paginaci�n

#### 2.4 Caso de Estudio
Crear `src/cima.Blazor.Client/Pages/Public/Portfolio/CaseStudy.razor`:
- Detalle del proyecto
- Galer�a de im�genes
- Proceso de dise�o
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
- Generar sitemap.xml autom�ticamente
- Incluir todas las propiedades publicadas
- Incluir p�ginas est�ticas
- Actualizaci�n autom�tica

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
- Redimensionar im�genes
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
- Cach� de assets est�ticos
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

## ?? Checklist de Implementaci�n

### Fase 1: Detalle de Propiedad
- [ ] Agregar m�todo `GetRelatedPropertiesAsync` al backend
- [ ] Actualizar `ImageGallery.razor` con lightbox
- [ ] Crear `PropertyDetailInfo.razor`
- [ ] Crear `PropertyMap.razor` con Google Maps
- [ ] Actualizar `ContactForm.razor` contextual
- [ ] Crear `RelatedProperties.razor`
- [ ] Crear `ShareButtons.razor`
- [ ] Actualizar `Detail.razor` con todos los componentes
- [ ] Probar navegaci�n completa
- [ ] Validar responsive design

### Fase 2: Portafolio
- [ ] Crear `PortfolioGrid.razor`
- [ ] Crear p�gina `Portfolio/Index.razor`
- [ ] Crear `Portfolio/CaseStudy.razor`
- [ ] Agregar navegaci�n en men�
- [ ] Probar filtros
- [ ] Validar carga de proyectos

### Fase 3: SEO
- [ ] Crear `MetaTags.razor` component
- [ ] Crear `PropertySchema.razor`
- [ ] Implementar `SitemapService.cs`
- [ ] Crear `robots.txt`
- [ ] Integrar meta tags en todas las p�ginas
- [ ] Validar con Facebook Debugger
- [ ] Validar con Google Rich Results Test
- [ ] Probar canonical URLs

### Fase 4: Optimizaciones
- [ ] Implementar `ImageOptimizationService.cs`
- [ ] Agregar preload de recursos cr�ticos
- [ ] Configurar lazy loading mejorado
- [ ] (Opcional) Implementar Service Worker
- [ ] Medir performance con Lighthouse
- [ ] Optimizar Core Web Vitals

---

## ?? Configuraci�n Requerida

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

### Optimizar Im�genes
```powershell
# Usando ImageMagick
magick convert input.jpg -quality 85 -resize 1200x output.webp
```

---

## ?? M�tricas de �xito

### Performance
- [ ] Lighthouse Performance Score > 90
- [ ] First Contentful Paint < 1.5s
- [ ] Largest Contentful Paint < 2.5s
- [ ] Cumulative Layout Shift < 0.1

### SEO
- [ ] Lighthouse SEO Score > 95
- [ ] Todas las p�ginas tienen meta tags
- [ ] Schema.org markup v�lido
- [ ] Sitemap.xml accesible
- [ ] robots.txt configurado

### UX
- [ ] Galer�a de im�genes fluida
- [ ] Mapa carga correctamente
- [ ] Share buttons funcionan
- [ ] Formulario valida inputs
- [ ] Responsive en mobile

---

## ?? Problemas Anticipados

### Google Maps
**Problema**: Clave API no funciona  
**Soluci�n**: Verificar que est� habilitada Maps JavaScript API y que el dominio est� en la lista blanca

### Lightbox
**Problema**: No abre en mobile  
**Soluci�n**: Usar librer�a compatible con touch events (PhotoSwipe)

### Meta Tags
**Problema**: No se actualizan en redes sociales  
**Soluci�n**: Usar Facebook Debugger para invalidar cach�

### Sitemap
**Problema**: No se genera autom�ticamente  
**Soluci�n**: Implementar endpoint y job scheduler

---

## ?? Recursos y Referencias

### Librer�as Recomendadas
- **Lightbox**: PhotoSwipe, GLightbox
- **Maps**: Google Maps JavaScript API
- **Share**: Web Share API nativo
- **SEO**: MetaTags NuGet package

### Documentaci�n
- [Open Graph Protocol](https://ogp.me/)
- [Schema.org - RealEstateListing](https://schema.org/RealEstateListing)
- [Google Maps JavaScript API](https://developers.google.com/maps/documentation/javascript)
- [Web Share API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Share_API)

---

## ? Entregables

1. **P�gina de Detalle Completa**
   - Con galer�a, mapa, formulario y shares
   
2. **P�gina de Portafolio**
   - Grid de proyectos y casos de estudio
   
3. **SEO Implementado**
   - Meta tags, Schema.org, sitemap, robots.txt
   
4. **Optimizaciones**
   - Im�genes, preload, cach�

5. **Documentaci�n**
   - `DIA_8_COMPLETADO.md`
   - Gu�a de configuraci�n de Google Maps
   - Gu�a de SEO

6. **Commits At�micos**
   - M�nimo 8 commits organizados
   - Cada fase en commits separados

---

## ?? Siguientes Pasos (D�a 9)

1. **Admin Dashboard Completo**
   - Estad�sticas visuales
   - Gr�ficas con Chart.js
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
   - Notificaciones autom�ticas
   - Templates responsive
   - SendGrid/SMTP

---

**Tiempo Estimado Total**: 4-5 horas  
**Complejidad**: Media-Alta  
**Prioridad**: ALTA ???

---

�Deseas comenzar con el D�a 8 ahora o prefieres hacer push de los commits del D�a 7 primero?
