# ejecutar-commits-atomicos.ps1 - Crear commits atomicos organizados
Write-Host "===== COMMITS ATOMICOS CIMA =====" -ForegroundColor Cyan
Write-Host ""

$commits = @(
    @{
        Files = @("src/cima.Application.Contracts/Architects/", "src/cima.Application.Contracts/ContactRequests/", "src/cima.Application.Contracts/Properties/")
        Message = "feat(contracts): agregar interfaces reorganizadas por feature"
    },
    @{
        Files = @("src/cima.Application/Architects/", "src/cima.Application/ContactRequests/", "src/cima.Application/Properties/PropertyAppService.cs")
        Message = "feat(application): implementar AppServices reorganizados"
    },
    @{
        Files = @("src/cima.Application.Contracts/Services/", "src/cima.Application/Services/")
        Message = "refactor(application): eliminar servicios legacy en carpeta Services"
        Update = $true
    },
    @{
        Files = @("src/cima.Application/cimaApplicationAutoMapperProfile.cs", "src/cima.Application.Contracts/Permissions/")
        Message = "feat(application): actualizar AutoMapper y permisos"
    },
    @{
        Files = @("src/cima.Application/Data/")
        Message = "feat(data): agregar seeder de permisos para rol admin"
    },
    @{
        Files = @("src/cima.Domain/OpenIddict/OpenIddictDataSeedContributor.cs")
        Message = "feat(auth): agregar grant type password a cliente Swagger"
    },
    @{
        Files = @("src/cima.EntityFrameworkCore/EntityFrameworkCore/cimaDbContext.cs", "src/cima.EntityFrameworkCore/Migrations/cimaDbContextModelSnapshot.cs")
        Message = "feat(efcore): actualizar DbContext con entidad Listing"
    },
    @{
        Files = @("src/cima.Domain/Entities/", "src/cima.Domain.Shared/Dtos/", "src/cima.Domain.Shared/Enums.cs", "src/cima.Domain.Shared/Dtos/ValidationDtos.cs")
        Message = "refactor(domain): actualizar entidades y DTOs existentes"
    },
    @{
        Files = @("etc/scripts/reset-database.ps1", "etc/scripts/actualizar-migraciones.ps1", "etc/scripts/ver-logs.ps1", "etc/scripts/limpiar-logs.ps1", "etc/scripts/test-api.ps1")
        Message = "feat(scripts): agregar scripts de gestion de BD, logs y pruebas"
    },
    @{
        Files = @("etc/scripts/verificar-permisos.sql", "etc/scripts/verificar-swagger-client.sql")
        Message = "feat(scripts): agregar scripts SQL de verificacion"
    },
    @{
        Files = @("tools/cima.ScriptRunner/")
        Message = "feat(tools): agregar app de consola para ejecutar scripts"
    },
    @{
        Files = @("etc/scripts/README.md")
        Message = "docs(scripts): actualizar README con nuevos scripts"
    },
    @{
        Files = @("docs/CORRECCIONES_4_FALLOS_API.md", "docs/RESUMEN_PRUEBAS_API.md", "docs/SOLUCION_PERMISOS_API.md")
        Message = "docs(api): documentar correcciones y pruebas API"
    },
    @{
        Files = @("docs/GUIA_MIGRACIONES_EF.md", "docs/SISTEMA_LOGGING.md", "docs/NAMESPACE_REORGANIZATION_SUMMARY.md", "docs/COMANDOS_PRUEBA_REORGANIZACION.md", "docs/PLAN_COMMITS_ATOMICOS.md")
        Message = "docs(tecnica): agregar guias de migraciones, logging y plan de commits"
    },
    @{
        Files = @("docs/")
        Message = "docs: eliminar documentos obsoletos"
        Update = $true
    }
)

$successCount = 0
$failCount = 0

foreach ($commit in $commits) {
    Write-Host "Commit: $($commit.Message)" -ForegroundColor Yellow
    
    try {
        if ($commit.Update) {
            # Para archivos eliminados/modificados
            foreach ($file in $commit.Files) {
                git add -u $file 2>$null
            }
        } else {
            # Para archivos nuevos/modificados
            foreach ($file in $commit.Files) {
                git add $file 2>$null
            }
        }
        
        # Verificar si hay cambios staged
        $status = git diff --cached --name-only
        if ($status) {
            git commit -m $commit.Message
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  OK" -ForegroundColor Green
                $successCount++
            } else {
                Write-Host "  FALLO en commit" -ForegroundColor Red
                $failCount++
            }
        } else {
            Write-Host "  SIN CAMBIOS (omitido)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  ERROR: $_" -ForegroundColor Red
        $failCount++
    }
    
    Write-Host ""
}

Write-Host "===== RESUMEN =====" -ForegroundColor Cyan
Write-Host "Exitosos: $successCount" -ForegroundColor Green
Write-Host "Fallidos: $failCount" -ForegroundColor Red
Write-Host ""
Write-Host "Verificar historial:" -ForegroundColor Yellow
Write-Host "  git log --oneline -15" -ForegroundColor White
