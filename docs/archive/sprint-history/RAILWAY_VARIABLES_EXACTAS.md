# ?? VARIABLES RAILWAY - COPIAR Y PEGAR

## ?? INSTRUCCIONES

1. Ir a Railway Dashboard: https://railway.app/dashboard
2. Clic en tu servicio de la app Blazor
3. Clic en "Variables"
4. Clic en "Raw Editor"
5. **COPIAR TODO LO DE ABAJO** ?
6. Pegar en Raw Editor
7. Clic "Deploy"

---

## ?? VARIABLES COMPLETAS

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

## ?? IMPORTANTE

### Si tu servicio PostgreSQL tiene otro nombre

Railway muestra el nombre del servicio en el dashboard. Si NO se llama "Postgres":

**Ver nombre:**
```
Railway Dashboard > Tu servicio PostgreSQL > Settings > Service Name
```

**Ejemplo si se llama "Database":**
```bash
ConnectionStrings__Default=Host=${{Database.PGHOST}};Port=${{Database.PGPORT}};Database=${{Database.PGDATABASE}};Username=${{Database.PGUSER}};Password=${{Database.PGPASSWORD}}
```

**Ejemplo si se llama "postgres-db":**
```bash
ConnectionStrings__Default=Host=${{postgres-db.PGHOST}};Port=${{postgres-db.PGPORT}};Database=${{postgres-db.PGDATABASE}};Username=${{postgres-db.PGUSER}};Password=${{postgres-db.PGPASSWORD}}
```

---

## ?? VERIFICAR VALORES (Opcional)

Para asegurar que las variables se generaron correctamente:

```
Railway Dashboard > Servicio PostgreSQL > Variables

Deberías ver variables como:
- PGHOST = ballast.proxy.rlwy.net
- PGPORT = 24411
- PGDATABASE = railway (o nombre personalizado)
- PGUSER = postgres (o nombre personalizado)
- PGPASSWORD = (generado automáticamente)
```

Railway reemplaza `${{Postgres.VARIABLE}}` con estos valores automáticamente.

---

## ? DESPUÉS DE PEGAR

1. Clic "Deploy" (botón inferior derecha)
2. Railway hace redeploy automático
3. Esperar 5-8 minutos
4. Ver logs en: Deployments > Latest > Logs

**Logs esperados:**
```
info: Microsoft.Hosting.Lifetime
      Now listening on: http://+:8080
info: Microsoft.Hosting.Lifetime
      Application started.
info: Microsoft.Hosting.Lifetime
      Hosting environment: Staging
```

---

## ?? PROBAR CONEXIÓN

```powershell
# Reemplaza con tu URL de Railway
$url = "https://cima-staging-production-xyz.up.railway.app"

# Health check
Invoke-WebRequest "$url/api/health/ping"
```

**Respuesta OK:**
```json
{
  "status": "alive",
  "message": "pong"
}
```

---

**¿Listo?** Copia las variables de arriba y pégalas en Railway. ??
