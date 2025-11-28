# EJECUTAR AHORA - Fix Railway Migrations

## 1. Commit y Push

```bash
git add .
git commit -F .git_commit_msg_fix_railway_migrations.txt
git push origin develop
```

## 2. Monitorear Railway

1. Abre Railway Dashboard: https://railway.app
2. Ve al servicio **cima-blazor**
3. Click en **Deployments**
4. Espera a que se complete el build
5. Click en **Logs**

## 3. Buscar en Logs

Busca esta linea:
```
[MIGRATION] Connection String:
```

Deberas ver:
```
[MIGRATION] Connection String: Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=***
[MIGRATION] Pending Migrations: 5
[MIGRATION]   - 20251117221607_InitialCreate
[MIGRATION]   - 20251118170041_FixPhoneNullableInContactRequest
[MIGRATION]   - 20251119190311_CorrigiendoPropertiesAListings
[MIGRATION]   - 20251121204743_AddRepliedAtToContactRequest
[MIGRATION]   - 20251125200837_AddPropertyCategoryAndFeaturedListings
[MIGRATION] Ejecutando Database.MigrateAsync()...
[MIGRATION] MigrateAsync() completado
[MIGRATION] Applied Migrations: 5
```

## 4. Verificar PostgreSQL

En Railway:
1. Click en el servicio **PostgreSQL**
2. Click en **Data**
3. Ejecuta:

```sql
SELECT * FROM "__EFMigrationsHistory";
```

Deberas ver 5 registros.

## 5. Si Hay Error

Si ves error en los logs:

```
[ERROR] ERROR CRITICO al ejecutar migraciones automaticas
```

Copia el stack trace completo y pegalo aqui para analizarlo.

## Posibles Problemas

### Error: "password authentication failed"
**Solucion:** Verifica variable `ConnectionStrings__Default` en Railway

### Error: "database does not exist"
**Solucion:** Verifica que la base de datos se llame `railway`

### Error: "could not translate host name"
**Solucion:** Verifica que ambos servicios esten en el mismo proyecto Railway

## Si Todo Funciona

Veras:
- ? 5 migraciones aplicadas
- ? Tabla `__EFMigrationsHistory` con 5 registros
- ? Todas las tablas creadas (AbpUsers, Listings, etc.)
- ? App iniciando correctamente

## Siguiente Paso

Si las migraciones se ejecutan pero la app falla en otro punto, avisa y revisamos el siguiente error.
