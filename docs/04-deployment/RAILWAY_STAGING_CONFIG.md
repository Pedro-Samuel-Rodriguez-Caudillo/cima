# CONFIGURACIÓN RAILWAY - STAGING (develop)

## ?? VARIABLES DE ENTORNO

### Copiar en Railway Dashboard > cima-blazor > Variables > Raw Editor

```bash
# === AMBIENTE ===
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080
PORT=8080

# === URLs PÚBLICAS ===
# Railway genera automáticamente RAILWAY_PUBLIC_DOMAIN
APP_SELF_URL=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__Authority=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__RequireHttpsMetadata=true

# === BASE DE DATOS ===
# Usa las variables que Railway genera al agregar PostgreSQL
ConnectionStrings__Default=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}

# === OPCIONAL: Si agregas Redis ===
# Redis__Configuration=${{Redis.REDIS_URL}}
```

---

## ?? PASOS DE CONFIGURACIÓN

### 1. Push Rama Develop a GitHub

```powershell
# Ya estás en rama develop
git add railway.json Procfile docs/GIT_BRANCHING_STRATEGY.md
git commit -m "ci(railway): add Railway configuration for staging

- Add railway.json with Dockerfile builder config
- Add Procfile for Railway startup
- Add branching strategy documentation
- Configure develop branch for staging deployment

Branch: develop ? Railway staging environment"

git push -u origin develop
```

### 2. Configurar Railway

#### A. Crear Proyecto Railway

1. Ir a: https://railway.app/
2. Login con GitHub
3. New Project > Deploy from GitHub repo
4. Seleccionar: `Pedro-Samuel-Rodriguez-Caudillo/cima`
5. **IMPORTANTE:** Seleccionar rama **develop** (no master)

#### B. Agregar PostgreSQL

1. En tu proyecto Railway, clic "+ New"
2. Database > PostgreSQL
3. Railway crea la BD automáticamente

#### C. Configurar Variables

1. Clic en servicio `cima-blazor`
2. Variables > Raw Editor
3. Copiar y pegar las variables de arriba ?
4. Clic "Deploy"

#### D. Verificar Build

1. Deployments > Latest
2. Ver que usa Dockerfile
3. Ver logs de build
4. Esperar ~5 minutos

---

## ?? URLs ESPERADAS

### Staging (develop branch)

```
App: https://cima-staging.up.railway.app
     o
     https://cima-production-[random].up.railway.app

Health: https://[tu-url]/api/health/ping
Swagger: https://[tu-url]/swagger
```

### Production (master branch) - Futuro

```
App: https://cima.com (con dominio propio)
Health: https://cima.com/api/health/ping
```

---

## ?? VERIFICAR CONFIGURACIÓN

### 1. Variables en Railway

```
Railway Dashboard > cima-blazor > Variables

Deberías ver:
? ASPNETCORE_ENVIRONMENT = Staging
? PORT = 8080
? APP_SELF_URL = https://...
? ConnectionStrings__Default = Host=...
? (Variables Postgres.* auto-generadas)
```

### 2. Deployment Exitoso

```
Railway Dashboard > Deployments > Latest

? Building... (2-3 min)
? Starting... (30 seg)
? Health check passed (30 seg)
? Running (? green)
```

### 3. Health Check

```powershell
# Reemplaza con tu URL real de Railway
$url = "https://cima-staging.up.railway.app"
Invoke-WebRequest "$url/api/health/ping"
```

**Respuesta esperada:**
```json
{
  "status": "alive",
  "timestamp": "2025-11-17T...",
  "message": "pong",
  "application": "cima.Blazor"
}
```

---

## ?? TROUBLESHOOTING

### Error: "Could not determine how to build"

**Solución:** Asegurar que `railway.json` está en la raíz del repo.

```powershell
# Verificar
git ls-files railway.json
# Debería mostrar: railway.json
```

### Error: "Connection to database failed"

**Verificar:**
1. PostgreSQL está agregado al proyecto Railway
2. Servicio PostgreSQL está Running (? green)
3. ConnectionString usa sintaxis correcta: `${{Postgres.PGHOST}}`

### Error: "Health check failed"

**Verificar:**
1. Puerto correcto: `PORT=8080`
2. ASPNETCORE_URLS correcto: `http://+:8080`
3. Health check path: `/api/health/ping`

**Ver logs:**
```
Railway Dashboard > cima-blazor > Deployments > Latest > Logs
```

---

## ?? MONITOREO

### Metrics en Railway

```
Railway Dashboard > cima-blazor > Metrics

- CPU Usage
- Memory Usage
- Network (Requests/seg)
- Deployment History
```

### Logs en Tiempo Real

```powershell
# Con Railway CLI
npm install -g @railway/cli
railway login
railway link
railway logs
```

---

## ?? WORKFLOW DESARROLLO

### Deploy a Staging

```bash
# 1. Trabajar en feature branch
git checkout -b feature/nueva-funcionalidad

# 2. Commits
git add .
git commit -m "feat(scope): descripción"

# 3. Push
git push origin feature/nueva-funcionalidad

# 4. Pull Request en GitHub
# feature/nueva-funcionalidad ? develop

# 5. Merge PR
# Railway detecta cambios en develop
# Deploy automático a staging

# 6. Probar en staging
# https://cima-staging.up.railway.app
```

### Deploy a Producción (Futuro)

```bash
# Cuando staging está estable
git checkout master
git merge develop
git tag v1.0.0
git push origin master --tags

# GitHub Actions deploy a producción
```

---

## ?? CHECKLIST SETUP

- [ ] Rama develop pushed a GitHub
- [ ] Railway proyecto creado
- [ ] Railway conectado a repo (rama develop)
- [ ] PostgreSQL agregado a Railway
- [ ] Variables configuradas en Railway
- [ ] Primer deploy exitoso
- [ ] Health check pasando
- [ ] URL staging accesible
- [ ] Documentación revisada

---

## ?? PRÓXIMOS PASOS

1. ? Push rama develop a GitHub
2. ? Configurar Railway (5 min)
3. ? Verificar deploy
4. ? Probar app en staging
5. ? Configurar protección de ramas en GitHub

---

**COMANDO PARA EJECUTAR AHORA:**

```powershell
# Estás en rama develop, hacer commit y push
git add railway.json Procfile docs/GIT_BRANCHING_STRATEGY.md docs/RAILWAY_STAGING_CONFIG.md
git commit -m "ci(railway): add Railway configuration for staging"
git push -u origin develop
```

Luego configurar en Railway dashboard.
