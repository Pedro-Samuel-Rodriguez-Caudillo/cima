# ? RAILWAY STAGING - LISTO PARA CONFIGURAR

## ?? COMPLETADO

```
? Rama develop creada y pusheada
? railway.json configurado
? Procfile configurado  
? Workflow GitHub Actions actualizado
? Documentación completa
? PostgreSQL ya existe: ballast.proxy.rlwy.net:24411
```

---

## ?? HACER AHORA (5 MINUTOS)

### 1. Abrir Railway Dashboard

```
https://railway.app/dashboard
```

### 2. Verificar/Crear Servicio App

**Si YA existe el servicio de la app:**
- Clic en el servicio
- Ir a paso 3

**Si NO existe:**
- Clic "+ New"
- "GitHub Repo"
- Seleccionar: `Pedro-Samuel-Rodriguez-Caudillo/cima`
- **IMPORTANTE:** Rama `develop`
- Railway detecta `railway.json` automáticamente

### 3. Configurar Variables

```
Servicio App > Variables > Raw Editor
```

**COPIAR Y PEGAR:**
```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080
PORT=8080
APP_SELF_URL=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__Authority=https://${{RAILWAY_PUBLIC_DOMAIN}}
AuthServer__RequireHttpsMetadata=true
ConnectionStrings__Default=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
```

**NOTA:** Si tu PostgreSQL se llama diferente a "Postgres", cámbialo.

### 4. Deploy

```
Clic "Deploy" ? Esperar 5-8 min
```

### 5. Verificar

```powershell
# Railway te da una URL como:
# https://cima-staging-production-abc123.up.railway.app

# Probar health check
Invoke-WebRequest "https://TU-URL/api/health/ping"
```

---

## ?? ESTRUCTURA RAILWAY

```
???????????????????????????????????????????
?  Tu Proyecto Railway                    ?
?                                         ?
?  ???????????????    ???????????????   ?
?  ? PostgreSQL  ?????? cima-blazor ?   ?
?  ?             ?    ?             ?   ?
?  ? ballast...  ?    ? Dockerfile  ?   ?
?  ? :24411      ?    ? develop     ?   ?
?  ? ? Running   ?    ? ? Building ?   ?
?  ???????????????    ???????????????   ?
?                            ?            ?
?                            ?            ?
?                     URL Pública         ?
?         cima-staging-xyz.up.railway.app ?
???????????????????????????????????????????
```

---

## ? SEÑALES DE ÉXITO

### Variables Configuradas

```
Railway > cima-blazor > Variables

? ASPNETCORE_ENVIRONMENT = Staging
? PORT = 8080
? APP_SELF_URL = https://...
? ConnectionStrings__Default = Host=...
```

### Deploy Exitoso

```
Railway > Deployments > Latest

? Detecting builder... (Dockerfile)
? Building image...
? Image built successfully
? Starting container...
? Health check passed
? Running
```

### Logs Correctos

```
Railway > Logs

? Now listening on: http://+:8080
? Application started
? Hosting environment: Staging
? Database connection established
```

---

## ?? WORKFLOW COMPLETO

```
LOCAL (develop)
    ?
    ?? git commit
    ?? git push origin develop
    ?
    ?
GITHUB (develop branch)
    ?
    ?
RAILWAY
    ?
    ?? Detecta cambios
    ?? Build con Dockerfile
    ?? Deploy automático
    ?
    ?
STAGING URL
    ?
    ?? https://cima-staging-xyz.up.railway.app
```

---

## ?? DOCUMENTACIÓN

| Documento | Uso |
|-----------|-----|
| `RAILWAY_CONFIGURAR_AHORA.md` | ? **LEE ESTO PARA CONFIGURAR** |
| `RAILWAY_VARIABLES_EXACTAS.md` | Variables para copiar |
| `GIT_BRANCHING_STRATEGY.md` | Cómo usar ramas |
| `RAILWAY_SETUP_EJECUTIVO.md` | Resumen completo |

---

## ?? SI ALGO FALLA

### Build Error

```
Railway > Deployments > Latest > Build Logs
```

Buscar errores de:
- Dockerfile
- dotnet restore
- npm install

### Connection Error

```
Verificar:
1. PostgreSQL Running ?
2. Variables correctas
3. ConnectionString usa ${{Postgres.*}}
```

### Health Check Falla

```
Ver logs:
Railway > Logs

Buscar:
- Connection refused
- Port binding error
- Missing variables
```

---

## ?? CHECKLIST

- [ ] Railway Dashboard abierto
- [ ] PostgreSQL visible (ballast.proxy.rlwy.net:24411)
- [ ] Servicio app creado desde repo develop
- [ ] Variables copiadas y pegadas
- [ ] Deploy iniciado
- [ ] Esperando build (5-8 min)
- [ ] Logs muestran "Application started"
- [ ] Health check retorna {"status":"alive"}
- [ ] URL staging accesible

---

## ?? DESPUÉS DE CONFIGURAR

### Desarrollo Diario

```bash
# Crear feature
git checkout develop
git pull origin develop
git checkout -b feature/nueva-funcionalidad

# Trabajar...
git add .
git commit -m "feat(scope): nueva funcionalidad"
git push origin feature/nueva-funcionalidad

# PR en GitHub ? develop
# Merge ? Railway deploy automático
```

### Probar en Staging

```
URL: https://TU-URL-RAILWAY.up.railway.app
Login: admin / 1q2w3E*
Probar features antes de producción
```

---

## ?? RESUMEN

**ESTADO:** Listo para configurar ?

**ACCIÓN:** Ir a Railway y configurar variables (5 min)

**RESULTADO:** Staging automático funcionando

---

**URL Railway:** https://railway.app/dashboard

**Variables:** Ver `docs/RAILWAY_VARIABLES_EXACTAS.md`

**Próximo paso:** Configurar ? Deploy ? Probar ??
