# Fix Railway - Debug de Migraciones

## Problema Identificado

Las migraciones **NO se estaban ejecutando** en Railway. Los logs mostraban:

```
[22:52:28 INF] === INICIANDO MIGRACIONES AUTOMATICAS ===
[22:52:30 INF] === MIGRACIONES COMPLETADAS EXITOSAMENTE ===
```

Pero PostgreSQL indicaba:
```
ERROR: relation "__EFMigrationsHistory" does not exist
```

Esto significa que **nunca se ejecuto ninguna migracion**.

## Cambios Aplicados

### 1. Eliminada Llamada Duplicada

**Archivo:** `src/cima.Blazor/cimaBlazorModule.cs`

- Eliminada segunda llamada a `ExecutarMigracionesAsync()` al final del metodo
- Solo se ejecuta UNA VEZ al inicio de `OnApplicationInitialization`

### 2. Logging Mejorado en Migrator

**Archivo:** `src/cima.EntityFrameworkCore/EntityFrameworkCore/EntityFrameworkCorecimaDbSchemaMigrator.cs`

Ahora muestra:
- Connection string (con password enmascarado)
- Migraciones pendientes
- Migraciones aplicadas

### 3. Manejo de Errores Mejorado

**Archivo:** `src/cima.Blazor/cimaBlazorModule.cs`

Ahora en caso de error:
- Se loguea el mensaje completo
- Se loguea el stack trace
- Se loguea la inner exception
- **SE LANZA LA EXCEPCION** para que Railway vea el fallo

## Proximo Deploy en Railway

Despues de hacer push, Railway mostrara logs como:

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

## Verificacion en Railway

### 1. Verificar Logs

En Railway Dashboard:
1. Ve al servicio `cima-blazor`
2. Click en **Logs**
3. Busca `[MIGRATION]`

### 2. Verificar PostgreSQL

Conectate a PostgreSQL en Railway:

```sql
-- Ver tabla de migraciones
SELECT * FROM "__EFMigrationsHistory";

-- Ver tablas creadas
SELECT tablename FROM pg_tables WHERE schemaname = 'public';

-- Contar registros en cada tabla
SELECT 
    schemaname,
    tablename,
    (xpath('/row/cnt/text()', xml_count))[1]::text::int as row_count
FROM (
    SELECT 
        schemaname, 
        tablename,
        query_to_xml(format('select count(*) as cnt from %I.%I', schemaname, tablename), false, true, '') as xml_count
    FROM pg_tables
    WHERE schemaname = 'public'
) t
ORDER BY row_count DESC;
```

## Si Persiste el Error

### Opcion 1: Verificar Variables de Entorno

```json
{
  "ConnectionStrings__Default": "Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=..."
}
```

### Opcion 2: Ejecutar DbMigrator Manualmente

1. Crear servicio separado en Railway
2. Usar Dockerfile.migrator
3. Ejecutar ANTES del servicio principal

### Opcion 3: Reset de Base de Datos

**CUIDADO:** Esto eliminara todos los datos

```sql
-- En PostgreSQL de Railway
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;
```

Luego redeploy en Railway.

## Comandos Utiles

```powershell
# Ver logs en Railway
railway logs --follow

# Conectar a PostgreSQL
railway connect postgres

# Ver variables de entorno
railway variables

# Forzar redeploy
railway up --detach
```

## Commit y Push

```bash
git add .
git commit -m "fix(railway): mejorar logging y corregir ejecucion de migraciones

- Eliminada llamada duplicada a ExecutarMigracionesAsync
- Agregado logging detallado en EntityFrameworkCorecimaDbSchemaMigrator
- Mejorado manejo de errores con stack traces completos
- Lanzar excepcion para que Railway detecte fallos criticos"

git push origin develop
```

## Proximos Pasos

1. Hacer commit y push
2. Railway detectara cambios automaticamente
3. Verificar logs en Railway Dashboard
4. Conectar a PostgreSQL y verificar tablas
5. Si todo funciona, las 5 migraciones se aplicaran correctamente

## Notas Importantes

- Las migraciones se ejecutan **SOLO** en Staging y Production
- En Development se usa el comportamiento normal (manual)
- El logging ahora es mucho mas verbose para debugging
- Si hay error, la app **NO iniciara** (lanzara excepcion)
