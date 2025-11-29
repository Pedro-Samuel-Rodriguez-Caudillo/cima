# FIX RAILWAY STAGING - CORS y BaseURL

## Problema Identificado

### Error en Railway Staging
```
Access to fetch at 'https://localhost:44350/api/abp/application-configuration' 
from origin 'https://cima-staging.up.railway.app' 
has been blocked by CORS policy
```

### Causas Raíz

1. **BaseURL Hardcodeado**: `appsettings.json` del cliente Blazor tenía `localhost:44350` hardcodeado
2. **CORS no configurado**: No había configuración específica de CORS para Railway staging
3. **Falta appsettings por ambiente**: No existían archivos específicos para Staging

---

## Solución Aplicada

### 1. Configuración Cliente Blazor por Ambiente

#### ? `src/cima.Blazor.Client/wwwroot/appsettings.json`
```json
{
  "RemoteServices": {
    "Default": {
      "BaseUrl": "{BLAZOR_API_URL}"  // Variable de entorno
    }
  }
}
```

#### ? `src/cima.Blazor.Client/wwwroot/appsettings.Development.json`
```json
{
  "RemoteServices": {
    "Default": {
      "BaseUrl": "https://localhost:44350"
    }
  }
}
```

#### ? `src/cima.Blazor.Client/wwwroot/appsettings.Staging.json` (NUEVO)
```json
{
  "RemoteServices": {
    "Default": {
      "BaseUrl": "https://cima-staging.up.railway.app"
    }
  }
}
```

#### ? `src/cima.Blazor.Client/wwwroot/appsettings.Production.json` (NUEVO)
```json
{
  "RemoteServices": {
    "Default": {
      "BaseUrl": "https://cima.production.domain.com"
    }
  }
}
```

---

### 2. Configuración Servidor por Ambiente

#### ? `src/cima.Blazor/appsettings.Staging.json` (NUEVO)
```json
{
  "App": {
    "SelfUrl": "https://cima-staging.up.railway.app",
    "CorsOrigins": "https://cima-staging.up.railway.app",
    "RedirectAllowedUrls": "https://cima-staging.up.railway.app"
  },
  "AuthServer": {
    "Authority": "https://cima-staging.up.railway.app",
    "RequireHttpsMetadata": true,
    "IsOnK8s": "false"
  }
}
```

---

## Cómo Funciona

### Jerarquía de Configuración

```
1. appsettings.json (base)
2. appsettings.{Environment}.json (override)
3. Variables de entorno (override final)
```

### En Development
- Lee `appsettings.Development.json`
- BaseURL = `https://localhost:44350`
- CORS = `localhost:44350`

### En Staging (Railway)
- Lee `appsettings.Staging.json`
- BaseURL = `https://cima-staging.up.railway.app`
- CORS = `https://cima-staging.up.railway.app`
- ? **MISMO DOMINIO = SIN PROBLEMAS CORS**

### En Production
- Lee `appsettings.Production.json`
- BaseURL = dominio de producción
- CORS = dominio de producción

---

## Variables de Entorno Railway

### ? Ya Configuradas
```bash
ASPNETCORE_ENVIRONMENT=Staging  # Activa appsettings.Staging.json
DATABASE_URL=postgresql://...    # Connection string
PORT=8080                        # Puerto Railway
```

### ?? Opcional (si necesitas override)
```bash
App__SelfUrl=https://cima-staging.up.railway.app
App__CorsOrigins=https://cima-staging.up.railway.app
AuthServer__Authority=https://cima-staging.up.railway.app
```

---

## Verificación

### 1. Build Local
```bash
dotnet build
```

### 2. Test Staging Config
```bash
$env:ASPNETCORE_ENVIRONMENT="Staging"
dotnet run --project src/cima.Blazor
```

### 3. Verificar en Railway
```bash
# Ver logs
railway logs

# Buscar:
# ? "Using appsettings.Staging.json"
# ? "CORS origins: https://cima-staging.up.railway.app"
# ? "BaseUrl: https://cima-staging.up.railway.app"
```

---

## Archivos Modificados/Creados

### Modificados
- ? `src/cima.Blazor.Client/wwwroot/appsettings.json`

### Creados
- ? `src/cima.Blazor.Client/wwwroot/appsettings.Staging.json`
- ? `src/cima.Blazor.Client/wwwroot/appsettings.Production.json`
- ? `src/cima.Blazor/appsettings.Staging.json`

---

## Próximos Pasos

### 1. Commit y Push
```bash
git add .
git commit -m "fix(railway): configure CORS and BaseURL for staging environment"
git push origin develop
```

### 2. Railway Auto-Deploy
- Railway detectará el push
- Rebuildeará la app
- Aplicará `ASPNETCORE_ENVIRONMENT=Staging`
- Usará `appsettings.Staging.json`

### 3. Verificar en https://cima-staging.up.railway.app
- ? No más errores CORS
- ? API calls a `https://cima-staging.up.railway.app` (mismo dominio)
- ? AuthServer correcto
- ? Aplicación funcional

---

## Troubleshooting

### Si persiste error CORS
1. Verificar logs Railway: `railway logs`
2. Buscar línea: `CORS origins configured: ...`
3. Confirmar que incluye: `https://cima-staging.up.railway.app`

### Si BaseURL sigue apuntando a localhost
1. Verificar que `ASPNETCORE_ENVIRONMENT=Staging` en Railway
2. Ver en logs: `Environment: Staging`
3. Confirmar que existe `appsettings.Staging.json` en deploy

### Si AuthServer falla
1. Verificar variable `AuthServer__Authority`
2. Debe ser `https://cima-staging.up.railway.app`
3. Sin trailing slash `/`

---

## Configuración Final Railway

### Environment Variables Necesarias
```bash
# REQUERIDAS ?
ASPNETCORE_ENVIRONMENT=Staging
DATABASE_URL=postgresql://postgres:XXXX@XXXX.railway.app:5432/railway
PORT=8080

# OPCIONALES (si override necesario)
App__SelfUrl=https://cima-staging.up.railway.app
App__CorsOrigins=https://cima-staging.up.railway.app
AuthServer__Authority=https://cima-staging.up.railway.app
```

---

## Resumen Visual

```
???????????????????????????????????????????????????
?  ANTES (? CORS Error)                          ?
???????????????????????????????????????????????????
?                                                 ?
?  Client (Railway):                              ?
?    Origin: https://cima-staging.up.railway.app  ?
?                                                 ?
?  API Call:                                      ?
?    Target: https://localhost:44350 ?          ?
?                                                 ?
?  CORS Policy: ? BLOCKED                        ?
?                                                 ?
???????????????????????????????????????????????????

???????????????????????????????????????????????????
?  DESPUÉS (? Sin CORS)                          ?
???????????????????????????????????????????????????
?                                                 ?
?  Client (Railway):                              ?
?    Origin: https://cima-staging.up.railway.app  ?
?                                                 ?
?  API Call:                                      ?
?    Target: https://cima-staging.up.railway.app  ?
?            ? MISMO DOMINIO                     ?
?                                                 ?
?  CORS Policy: ? NOT NEEDED (same origin)       ?
?                                                 ?
???????????????????????????????????????????????????
```

---

**Status**: ? FIX LISTO PARA COMMIT  
**Próximo paso**: Commit ? Push ? Railway Auto-Deploy  
**Tiempo estimado**: 2-3 minutos hasta deploy completo
