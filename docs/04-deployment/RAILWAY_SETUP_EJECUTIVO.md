# ? RAILWAY STAGING - RESUMEN EJECUTIVO

## ? LO QUE SE HIZO

### 1. Estructura de Ramas Profesional

```
master (production)
  ??? develop (staging) ? NUEVA RAMA
```

### 2. Archivos Creados

- ? `railway.json` - Configuración Railway
- ? `Procfile` - Comando inicio
- ? `docs/GIT_BRANCHING_STRATEGY.md` - Estrategia branching
- ? `docs/RAILWAY_STAGING_CONFIG.md` - Config Railway
- ? `etc/scripts/setup-railway-staging.ps1` - Script setup
- ? `.github/workflows/cd-deploy-staging.yml` - Actualizado para develop

### 3. Rama Develop Creada

```bash
? develop branch creada desde master
? Archivos Railway configurados
? Pendiente: Push a GitHub
```

---

## ?? EJECUTAR AHORA

### Opción A: Script Automático (RECOMENDADO)

```powershell
.\etc\scripts\setup-railway-staging.ps1
```

**Qué hace:**
1. Verifica que estás en rama develop
2. Verifica archivos Railway existen
3. Hace commit si hay cambios
4. Push a GitHub (con confirmación)
5. Muestra próximos pasos en Railway

### Opción B: Manual

```powershell
# 1. Verificar rama
git branch
# Debe mostrar: * develop

# 2. Commit
git add railway.json Procfile docs/ etc/scripts/ .github/workflows/
git commit -m "ci(railway): add Railway configuration for staging"

# 3. Push
git push -u origin develop

# 4. Ver variables
.\etc\scripts\setup-railway-staging.ps1 -ShowVariables
```

---

## ?? VARIABLES RAILWAY (COPIAR DESPUÉS)

**En Railway Dashboard > cima-blazor > Variables > Raw Editor:**

```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080
PORT=8080
APP_SELF_URL=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__Authority=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__RequireHttpsMetadata=true
ConnectionStrings__Default=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
```

---

## ?? FLUJO COMPLETO

### 1. Push Develop (AHORA)

```powershell
.\etc\scripts\setup-railway-staging.ps1
```

### 2. Configurar Railway (5 min)

```
1. https://railway.app/
2. New Project > Deploy from GitHub
3. Repo: cima | Rama: develop ? IMPORTANTE
4. + New > Database > PostgreSQL
5. cima-blazor > Variables > Pegar variables
6. Esperar deploy (~5 min)
```

### 3. Verificar Deploy

```powershell
# URL de Railway (la verás en dashboard)
$url = "https://cima-staging.up.railway.app"

# Probar health check
Invoke-WebRequest "$url/api/health/ping"
```

**Respuesta esperada:**
```json
{
  "status": "alive",
  "message": "pong"
}
```

---

## ?? WORKFLOW DESARROLLO

### Crear Feature

```bash
git checkout develop
git pull origin develop
git checkout -b feature/nueva-funcionalidad

# Trabajar...
git add .
git commit -m "feat(scope): nueva funcionalidad"
git push origin feature/nueva-funcionalidad

# Crear PR en GitHub: feature/... ? develop
```

### Deploy a Staging

```bash
# Después de merge PR
git checkout develop
git pull origin develop

# Railway detecta cambios y hace deploy automático
# Probar en: https://cima-staging.up.railway.app
```

### Release a Producción

```bash
# Cuando staging está estable
git checkout master
git merge develop
git tag v1.0.0
git push origin master --tags

# GitHub Actions deploy a producción
```

---

## ?? AMBIENTES

### Staging (develop)

```
Rama: develop
Railway: Auto-deploy en push
URL: https://cima-staging.up.railway.app
BD: cima_staging (PostgreSQL en Railway)
Logs: Railway Dashboard
```

### Production (master) - Futuro

```
Rama: master
Deploy: GitHub Actions
URL: https://cima.com (con dominio)
BD: cima_production (servidor propio)
Logs: Servidor producción
```

---

## ??? PROTECCIÓN DE RAMAS (Configurar después)

**GitHub Settings > Branches > Add rule**

### master

- ?? Require pull request reviews
- ?? Require status checks to pass
- ?? Include administrators

### develop

- ?? Require status checks to pass
- ?? Require branches up to date

---

## ? CHECKLIST

- [x] Rama develop creada
- [x] Archivos Railway configurados
- [x] Workflow GitHub Actions actualizado
- [x] Script setup creado
- [x] Documentación completa
- [ ] **Push a GitHub** ? HACER AHORA
- [ ] Configurar Railway (5 min)
- [ ] Verificar deploy staging
- [ ] Configurar protección ramas

---

## ?? DOCUMENTACIÓN

| Documento | Descripción |
|-----------|-------------|
| `GIT_BRANCHING_STRATEGY.md` | Estrategia completa de ramas |
| `RAILWAY_STAGING_CONFIG.md` | Config Railway paso a paso |
| `RAILWAY_OBTENER_POSTGRES_LINK.md` | Cómo usar PostgreSQL |
| `RAILWAY_VARIABLES_COPIAR.md` | Variables para copiar |

---

## ?? CONVENCIONES

### Commits

```bash
feat(scope): nueva funcionalidad
fix(scope): corrección bug
docs(scope): documentación
style(scope): formato código
refactor(scope): refactorización
test(scope): tests
chore(scope): tareas mantenimiento
ci(scope): CI/CD
```

### Branches

```bash
feature/nombre-descriptivo    # Nueva feature
hotfix/nombre-bug             # Fix urgente
release/v1.0.0                # Preparar release
```

---

## ?? SI ALGO FALLA

### Railway no detecta build

**Verificar:**
```powershell
# Archivo railway.json existe?
Get-Content railway.json

# Está en la raíz del repo?
git ls-files railway.json
```

### PostgreSQL no conecta

**Verificar:**
1. PostgreSQL agregado en Railway
2. Servicio PostgreSQL Running (? green)
3. Variables usan sintaxis: `${{Postgres.VARIABLE}}`

### Health check falla

**Ver logs:**
```
Railway Dashboard > cima-blazor > Deployments > Latest > Logs
```

---

## ?? COMANDO PARA EJECUTAR

```powershell
# Todo en uno
.\etc\scripts\setup-railway-staging.ps1

# O manual
git add .
git commit -m "ci(railway): add Railway configuration for staging"
git push -u origin develop
```

**Luego:** Configurar en Railway Dashboard (5 min)

---

**ESTADO:** LISTO PARA PUSH ?  
**RAMA:** develop  
**PRÓXIMO:** Push a GitHub y configurar Railway
