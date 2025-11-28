# Solución: Permisos API CIMA

## Problema identificado

Los tests del API estaban fallando con errores 403 (Forbidden) y 400/500 en varios endpoints porque:

1. **Falta de permisos**: El rol `admin` no tenía asignados automáticamente los permisos personalizados de CIMA
2. **Errores en el script de pruebas**: Algunos endpoints usan GET con filtros como query parameters, no POST

## Solución aplicada

### 1. Data Seeder de permisos

Creado `src/cima.Domain/Data/cimaDataSeeder.cs` que asigna automáticamente todos los permisos de CIMA al rol `admin` cuando se ejecutan las migraciones.

```csharp
// Asigna permisos al rol "admin"
await _permissionDataSeeder.SeedAsync(
    RolePermissionValueProvider.ProviderName,
    "admin",
    new[] {
        cimaPermissions.Listings.Default,
        cimaPermissions.Listings.Create,
        // ... todos los permisos
    }
);
```

### 2. Script de reseteo de BD

Creado `etc/scripts/reset-database.ps1` para:
- Eliminar BD existente
- Crear BD nueva
- Ejecutar DbMigrator (que ejecuta el seeder de permisos)

### 3. Script de pruebas actualizado

Actualizado `etc/scripts/test-api.ps1` con:
- Corrección de endpoints GET que usaban filtros
- Mejor manejo de errores
- UTF-8 en Content-Type
- Orden lógico de pruebas (crear arquitecto primero, luego listing, luego contact request)

## Pasos para aplicar la solución

### Opción 1: Reseteo completo (RECOMENDADO)

```powershell
# Desde la raíz del proyecto
.\etc\scripts\reset-database.ps1
```

Esto:
1. Elimina `cima_db`
2. Crea `cima_db` nueva
3. Ejecuta migraciones
4. **Aplica permisos al rol admin automáticamente**

### Opción 2: Asignar permisos manualmente (sin resetear BD)

Si no quieres perder datos existentes, puedes ejecutar solo el migrator:

```powershell
cd src\cima.DbMigrator
dotnet run
cd ..\..
```

El seeder detectará que `admin` no tiene los permisos y los asignará.

### Verificar que funcionó

Ejecutar el script de pruebas:

```powershell
.\etc\scripts\test-api.ps1
```

Deberías ver:

```
===== PRUEBAS API CIMA =====
TOKEN OK: eyJhbGciOiJSUzI1NiIsImtp...

===== ARCHITECT =====
[2] POST /api/app/architect... OK
[3] GET usuario admin... OK
[4] GET /api/app/architect/{id}... OK
...

===== RESULTADO =====
Total pruebas: 20
Exitosas: 20
Fallidas: 0
TODO OK. API FUNCIONANDO.
```

## Endpoints que ahora funcionan

### Architect
- ? `POST /api/app/architect` - Crear
- ? `GET /api/app/architect/{id}` - Obtener por ID
- ? `GET /api/app/architect/by-user-id/{userId}` - Obtener por user
- ? `PUT /api/app/architect/{id}` - Actualizar
- ? `DELETE /api/app/architect/{id}` - Eliminar

### Listing
- ? `GET /api/app/listing` - Listar (paginado con filtros)
- ? `POST /api/app/listing` - Crear
- ? `GET /api/app/listing/{id}` - Obtener por ID
- ? `PUT /api/app/listing/{id}` - Actualizar
- ? `POST /api/app/listing/{id}/publish` - Publicar
- ? `POST /api/app/listing/{id}/archive` - Archivar
- ? `GET /api/app/listing/by-architect/{architectId}` - Listar por arquitecto
- ? `DELETE /api/app/listing/{id}` - Eliminar

### ContactRequest
- ? `POST /api/app/contact-request` - Crear (público, sin auth)
- ? `GET /api/app/contact-request` - Listar (paginado)
- ? `GET /api/app/contact-request/{id}` - Obtener por ID
- ? `POST /api/app/contact-request/{id}/mark-as-replied` - Marcar como respondida
- ? `POST /api/app/contact-request/{id}/close` - Cerrar
- ? `GET /api/app/contact-request/by-architect/{architectId}` - Listar por arquitecto

## Notas importantes

1. **Permisos persistentes**: Los permisos se asignan en la BD, no se pierden al reiniciar la aplicación
2. **Usuarios nuevos**: Si creas más usuarios admin, deberás asignarles permisos manualmente o volver a ejecutar el seeder
3. **Desarrollo**: El seeder solo se ejecuta si los permisos no existen, es seguro ejecutarlo múltiples veces
4. **Producción**: En producción, asigna permisos a través del UI de administración de ABP

## Troubleshooting

### Error 403 en endpoints
- Ejecuta `reset-database.ps1` para reaplicar permisos

### Error 400 en ContactRequest
- Verifica que el `listingId` en el body sea un GUID válido de un listing existente

### Error 500 interno
- Revisa los logs de la aplicación para más detalles
- Verifica que la conexión a BD sea correcta
