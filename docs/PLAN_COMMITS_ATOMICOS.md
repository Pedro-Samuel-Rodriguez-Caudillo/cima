# Plan de Commits Atomicos

Siguiendo la convencion: `<type>(<scope>): <subject>`

## Commits a Realizar

### 1. Reorganizacion de Namespaces
```bash
git add src/cima.Domain/Entities/Listing.cs
git add src/cima.Domain/Entities/ListingImage.cs
git add src/cima.Domain.Shared/Dtos/ListingDto.cs
git add src/cima.Domain.Shared/Dtos/ListingFiltersDto.cs
git add src/cima.Application.Contracts/Listings/
git add src/cima.Application/Listings/
git add src/cima.EntityFrameworkCore/Migrations/20251119190311_*
git commit -m "refactor(domain): renombrar Property a Listing para consistencia"
```

### 2. Nuevas Interfaces y Contratos
```bash
git add src/cima.Application.Contracts/Architects/
git add src/cima.Application.Contracts/ContactRequests/
git add src/cima.Application.Contracts/Properties/
git add src/cima.Domain.Shared/Dtos/
git commit -m "feat(contracts): agregar interfaces reorganizadas por feature"
```

### 3. Implementaciones de AppServices
```bash
git add src/cima.Application/Architects/
git add src/cima.Application/ContactRequests/
git add src/cima.Application/Properties/PropertyAppService.cs
git commit -m "feat(application): implementar AppServices reorganizados"
```

### 4. Limpieza de Archivos Legacy
```bash
git add -u src/cima.Application.Contracts/Services/
git add -u src/cima.Application/Services/
git commit -m "refactor(application): eliminar servicios legacy en carpeta Services"
```

### 5. AutoMapper y Permisos
```bash
git add src/cima.Application/cimaApplicationAutoMapperProfile.cs
git add src/cima.Application.Contracts/Permissions/
git commit -m "feat(application): actualizar AutoMapper y permisos"
```

### 6. Seeder de Permisos
```bash
git add src/cima.Application/Data/cimaPermissionsDataSeeder.cs
git commit -m "feat(data): agregar seeder de permisos para rol admin"
```

### 7. OpenIddict Grant Types
```bash
git add src/cima.Domain/OpenIddict/OpenIddictDataSeedContributor.cs
git commit -m "feat(auth): agregar grant type password a cliente Swagger"
```

### 8. Migraciones y DbContext
```bash
git add src/cima.EntityFrameworkCore/EntityFrameworkCore/cimaDbContext.cs
git add src/cima.EntityFrameworkCore/Migrations/cimaDbContextModelSnapshot.cs
git commit -m "feat(efcore): actualizar DbContext con entidad Listing"
```

### 9. Scripts de Automatizacion
```bash
git add etc/scripts/reset-database.ps1
git add etc/scripts/actualizar-migraciones.ps1
git add etc/scripts/ver-logs.ps1
git add etc/scripts/limpiar-logs.ps1
git commit -m "feat(scripts): agregar scripts de gestion de BD y logs"
```

### 10. Scripts de Verificacion
```bash
git add etc/scripts/verificar-permisos.sql
git add etc/scripts/verificar-swagger-client.sql
git commit -m "feat(scripts): agregar scripts SQL de verificacion"
```

### 11. Script Runner App
```bash
git add tools/cima.ScriptRunner/
git commit -m "feat(tools): agregar app de consola para ejecutar scripts"
```

### 12. Actualizacion Scripts README
```bash
git add etc/scripts/README.md
git commit -m "docs(scripts): actualizar README con nuevos scripts"
```

### 13. Documentacion de Correcciones
```bash
git add docs/CORRECCIONES_4_FALLOS_API.md
git add docs/RESUMEN_PRUEBAS_API.md
git add docs/SOLUCION_PERMISOS_API.md
git commit -m "docs(api): documentar correcciones y pruebas API"
```

### 14. Documentacion Tecnica
```bash
git add docs/GUIA_MIGRACIONES_EF.md
git add docs/SISTEMA_LOGGING.md
git add docs/NAMESPACE_REORGANIZATION_SUMMARY.md
git add docs/COMANDOS_PRUEBA_REORGANIZACION.md
git commit -m "docs(tecnica): agregar guias de migraciones y logging"
```

### 15. Limpieza de Documentos Obsoletos
```bash
git add -u docs/
git commit -m "docs: eliminar documentos obsoletos"
```

## Orden de Ejecucion

1. Cambios de dominio (entidades, DTOs)
2. Cambios de aplicacion (servicios, contratos)
3. Cambios de infraestructura (DbContext, migraciones)
4. Configuracion (permisos, auth)
5. Herramientas (scripts, app)
6. Documentacion

## Verificacion Post-Commits

```bash
# Verificar historial
git log --oneline -15

# Verificar que todo compila
dotnet build

# Ejecutar pruebas
dotnet run --project tools/cima.ScriptRunner
# Opcion 4: Ejecutar pruebas API
```
