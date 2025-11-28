# ?? CONFIGURAR RAILWAY AHORA - PASO A PASO

## ? COMPLETADO

- ? Rama `develop` creada
- ? Archivos Railway configurados
- ? Push a GitHub exitoso
- ? PostgreSQL ya existe en Railway: `ballast.proxy.rlwy.net:24411`

---

## ?? CONFIGURAR VARIABLES EN RAILWAY (2 MINUTOS)

### Paso 1: Ir a Railway Dashboard

1. Abrir: https://railway.app/dashboard
2. Clic en tu proyecto (probablemente ya existe si tienes PostgreSQL)

### Paso 2: Verificar PostgreSQL

Deberías ver un servicio PostgreSQL con:
```
Host: ballast.proxy.rlwy.net
Port: 24411
```

### Paso 3: Configurar Servicio Blazor

Si no existe un servicio para la app Blazor:

1. Clic "+ New"
2. "GitHub Repo"
3. Seleccionar: `Pedro-Samuel-Rodriguez-Caudillo/cima`
4. **IMPORTANTE:** Rama `develop` (no master)
5. Railway detectará `railway.json` automáticamente

### Paso 4: Variables de Entorno

1. Clic en el servicio `cima-blazor` (o como lo nombraste)
2. Variables > Raw Editor
3. **COPIAR Y PEGAR ESTO:**

```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080
PORT=8080
APP_SELF_URL=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__Authority=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__RequireHttpsMetadata=true
ConnectionStrings__Default=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
```

**NOTA:** Si tu PostgreSQL tiene nombre diferente a "Postgres", cámbialo en las variables.

### Paso 5: Deploy

1. Clic "Deploy" (esquina inferior derecha)
2. Railway hará build automático (5-8 min)
3. Ver logs en: Deployments > Latest > Logs

---

## ?? VERIFICAR QUE POSTGRESQL ESTÁ CONECTADO

### Opción A: Desde Railway Dashboard

```
1. Clic en servicio PostgreSQL
2. Variables
3. Copiar estos valores (para referencia):
   - PGHOST (debería ser ballast.proxy.rlwy.net)
   - PGPORT (debería ser 24411)
   - PGDATABASE
   - PGUSER
   - PGPASSWORD
```

### Opción B: Desde Railway CLI

```powershell
# Instalar CLI (si no lo tienes)
npm install -g @railway/cli

# Login
railway login

# Listar proyectos
railway list

# Seleccionar proyecto
railway link

# Ver variables de Postgres
railway variables -s postgres
```

---

## ?? ESTRUCTURA ESPERADA EN RAILWAY

```
Tu Proyecto Railway
?? PostgreSQL
?  ?? Host: ballast.proxy.rlwy.net
?  ?? Port: 24411
?  ?? Database: railway (o nombre personalizado)
?  ?? Status: ? Running
?
?? cima-blazor (tu app)
   ?? Source: GitHub repo (rama develop)
   ?? Builder: Dockerfile
   ?? Status: ? Running (después de deploy)
   ?? URL: https://cima-staging-xyz.up.railway.app
```

---

## ? SEÑALES DE QUE ESTÁ BIEN CONFIGURADO

### 1. Variables Visibles

```
Railway > cima-blazor > Variables

Deberías ver:
? ASPNETCORE_ENVIRONMENT = Staging
? PORT = 8080
? ConnectionStrings__Default = Host=...
? (Railway genera automáticamente las ${{Postgres.*}})
```

### 2. Build Exitoso

```
Railway > Deployments > Latest

? Detecting builder... (Dockerfile)
? Building...
? Image built successfully
? Starting container...
? Health check passed
? Status: Running ?
```

### 3. Logs Sin Errores

```
Railway > Logs

Esperado:
info: Microsoft.Hosting.Lifetime
      Now listening on: http://+:8080
info: Microsoft.Hosting.Lifetime  
      Application started.
info: Microsoft.Hosting.Lifetime
      Hosting environment: Staging
```

---

## ?? PROBAR DEPLOYMENT

### Health Check

```powershell
# Esperar a que Railway te dé la URL
# Ejemplo: https://cima-staging-production-abc123.up.railway.app

$url = "https://TU-URL-RAILWAY.up.railway.app"
Invoke-WebRequest "$url/api/health/ping"
```

**Respuesta esperada:**
```json
{
  "status": "alive",
  "timestamp": "2025-01-17T...",
  "message": "pong",
  "application": "cima.Blazor"
}
```

### Swagger (Opcional)

```powershell
# Abrir en navegador
start "https://TU-URL-RAILWAY.up.railway.app/swagger"
```

---

## ?? TROUBLESHOOTING

### Error: "Cannot connect to database"

**Verificar ConnectionString:**

Si tu servicio PostgreSQL se llama diferente a "Postgres":

```bash
# Ejemplo si se llama "Database" o "postgres-db"
ConnectionStrings__Default=Host=${{Database.PGHOST}};Port=${{Database.PGPORT}};...
```

**Ver nombre exacto:**
```
Railway Dashboard > PostgreSQL > Settings > Service Name
```

### Error: "Port already in use"

**Verificar:**
```bash
# Asegurar que PORT=8080 está configurado
PORT=8080

# Y que Dockerfile expone 8080
# (ya está correcto en railway.json)
```

### Error: "Build failed"

**Ver logs de build:**
```
Railway > Deployments > Latest > Build Logs
```

Buscar errores de:
- `dotnet restore`
- `npm install`  
- `Dockerfile`

---

## ?? CHECKLIST FINAL

- [ ] Railway proyecto existe
- [ ] PostgreSQL visible (ballast.proxy.rlwy.net:24411)
- [ ] Servicio cima-blazor creado desde repo GitHub
- [ ] Rama develop seleccionada
- [ ] Variables copiadas en Raw Editor
- [ ] Deploy iniciado (botón Deploy presionado)
- [ ] Build completado sin errores (5-8 min)
- [ ] Status: Running ?
- [ ] Health check retorna "pong"
- [ ] URL accesible

---

## ?? PRÓXIMO PASO

1. **AHORA:** Configurar variables en Railway (2 min)
2. **ESPERAR:** Deploy automático (5-8 min)
3. **PROBAR:** Health check y Swagger
4. **RESULTADO:** Staging funcionando en Railway

---

## ?? SI NECESITAS AYUDA

**Ver documentación:**
- `docs/RAILWAY_STAGING_CONFIG.md` - Guía paso a paso
- `docs/GIT_BRANCHING_STRATEGY.md` - Estrategia branching
- `docs/RAILWAY_SETUP_EJECUTIVO.md` - Resumen ejecutivo

**Railway Docs:**
- https://docs.railway.app/
- https://docs.railway.app/deploy/deployments

---

**URL de tu proyecto Railway:**
https://railway.app/dashboard

**Próximo paso:** Configurar variables ? Deploy ? Probar
