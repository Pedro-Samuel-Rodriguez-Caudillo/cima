# Instrucciones para Aplicar la Refactorización DDD

## ? Estado Actual
- [x] Código refactorizado
- [x] Tests actualizados y pasando
- [x] Compilación exitosa
- [x] Migración EF Core creada

## ?? Pasos Siguientes

### 1. Revisar la Migración

```bash
# Ver el archivo de migración generado
code src\cima.EntityFrameworkCore\Migrations\<timestamp>_RefactorDDD_BoundedContexts_AreasAndStats.cs
```

**Verificar que incluye:**
- Drop columns: `Name`, `Bio` de tabla `Architects`
- Add columns en `Architects`: `TotalListingsPublished`, `ActiveListings`, `RegistrationDate`, `IsActive`
- Rename column: `Area` ? `LandArea` en tabla `Listings`
- Add column: `ConstructionArea` en tabla `Listings`

### 2. Aplicar la Migración a Base de Datos

#### Opción A: Usando DbMigrator (Recomendada)
```bash
cd src\cima.DbMigrator
dotnet run
```

Esto ejecutará:
1. Migraciones pendientes
2. Seeder de permisos
3. Seeder de desarrollo (usuarios + 12 listings)

#### Opción B: Usando EF CLI Directamente
```bash
cd src\cima.EntityFrameworkCore
dotnet ef database update --startup-project ..\cima.DbMigrator\cima.DbMigrator.csproj
```

### 3. Verificar los Datos

```bash
# Conectarse a PostgreSQL
psql -U postgres -d cima

# Verificar estructura de Architects
\d "Architects"

# Verificar estructura de Listings  
\d "Listings"

# Verificar datos seeded
SELECT "UserId", "TotalListingsPublished", "ActiveListings", "IsActive" FROM "Architects";
SELECT "Title", "LandArea", "ConstructionArea" FROM "Listings" LIMIT 5;
```

### 4. Probar la Aplicación

```bash
# Terminal 1: Iniciar API
cd src\cima.Blazor
dotnet watch run

# Terminal 2: Iniciar Tailwind watch
cd src\cima.Blazor.Client
npm run watch:css
```

**Verificar:**
- [ ] Login con admin@cima.com / 1q2w3E*
- [ ] Login con arq@cima.com / 1q2w3E*
- [ ] Ver listado de propiedades (debería haber 8 publicadas)
- [ ] Ver detalles de una propiedad (verificar LandArea y ConstructionArea)
- [ ] Panel de admin accesible

### 5. Ejecutar Tests

```bash
# Tests de dominio
dotnet test test\cima.Domain.Tests\cima.Domain.Tests.csproj

# Tests de aplicación
dotnet test test\cima.Application.Tests\cima.Application.Tests.csproj

# Todos los tests
dotnet test
```

### 6. Hacer Commit

```bash
git add .
git commit -F .git_commit_msg_refactor_ddd.txt
git push origin develop
```

---

## ?? Rollback (Si es necesario)

### Revertir Migración
```bash
cd src\cima.EntityFrameworkCore
dotnet ef migrations remove --startup-project ..\cima.DbMigrator\cima.DbMigrator.csproj
```

### Revertir Base de Datos
```bash
cd src\cima.EntityFrameworkCore
dotnet ef database update <PreviousMigrationName> --startup-project ..\cima.DbMigrator\cima.DbMigrator.csproj
```

---

## ?? Datos de Prueba Creados

### Usuarios:
| Email | Password | Rol | Propósito |
|-------|----------|-----|-----------|
| admin@cima.com | 1q2w3E* | admin | Testing panel admin |
| arq@cima.com | 1q2w3E* | - | Testing arquitecto |

### Listings:
- **8** Published (visibles al público)
- **1** Portfolio (proyecto completado)
- **1** Archived (vendida)
- **2** Draft (borradores)

**Total:** 12 propiedades con datos realistas

### Featured Listings:
- 6 propiedades marcadas como destacadas

---

## ?? Notas Importantes

### Breaking Changes:
1. **API DTOs:** `CreateUpdateListingDto` ahora requiere `LandArea` y `ConstructionArea`
2. **ArchitectDto:** Ya no tiene `Name` ni `Bio`, tiene `TotalListingsPublished`, `ActiveListings`, `IsActive`
3. **Database:** Columnas eliminadas y renombradas

### Backward Compatibility:
- `ListingDto.Area` (read-only) apunta a `LandArea` para compatibilidad
- Servicios validan ambas áreas obligatorias

### ABP Settings Nuevos:
```csharp
// Configurables desde panel admin
cimaSettings.Contact.AdminEmail  // Default: "contacto@cima.com"
cimaSettings.Contact.AdminPhone  // Default: "+52 55 1234 5678"
```

---

## ?? Verificación Final

### Checklist Completa:
- [ ] Migración aplicada sin errores
- [ ] Seeder ejecutado correctamente
- [ ] Login con usuarios de prueba funciona
- [ ] Propiedades visibles en sitio público
- [ ] Panel admin accesible
- [ ] Tests pasan (Domain + Application)
- [ ] API responde correctamente
- [ ] Commit realizado

---

## ?? Soporte

Si encuentras algún problema:
1. Revisar logs en `src\cima.Blazor\Logs\`
2. Verificar connection string en `appsettings.json`
3. Consultar `REFACTORIZACION_DDD_COMPLETADA.md` para detalles técnicos

---

**¡Refactorización DDD completada exitosamente! ??**
