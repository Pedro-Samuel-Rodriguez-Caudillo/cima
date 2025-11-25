# Script para ejecutar el Data Seeder de desarrollo
# Solo funciona en ambiente Development

param(
    [switch]$Force
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Data Seeder de Desarrollo - CIMA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en el directorio correcto
$currentDir = Get-Location
if (-not (Test-Path "cima.sln")) {
    Write-Host "ERROR: Este script debe ejecutarse desde el directorio raíz del proyecto" -ForegroundColor Red
    Write-Host "Directorio actual: $currentDir" -ForegroundColor Yellow
    exit 1
}

# Verificar ambiente
$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host "Ambiente: Development" -ForegroundColor Green
Write-Host ""

# Confirmar con el usuario
if (-not $Force) {
    Write-Host "ADVERTENCIA: Este script agregará datos de prueba a la base de datos." -ForegroundColor Yellow
    Write-Host "Solo se ejecuta si NO existen propiedades en la BD." -ForegroundColor Yellow
    Write-Host ""
    $confirm = Read-Host "¿Desea continuar? (S/N)"
    if ($confirm -ne "S" -and $confirm -ne "s") {
        Write-Host "Operación cancelada" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Ejecutando DbMigrator..." -ForegroundColor Cyan

try {
    # Navegar al proyecto DbMigrator
    Push-Location "src\cima.DbMigrator"
    
    # Ejecutar el migrator
    dotnet run
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  Data Seeder ejecutado exitosamente" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Datos creados:" -ForegroundColor Cyan
        Write-Host "  - 1 Arquitecto de prueba" -ForegroundColor White
        Write-Host "  - 9 Propiedades (6 publicadas, 1 portafolio, 1 archivada, 1 borrador)" -ForegroundColor White
        Write-Host "  - 6 Propiedades marcadas como destacadas" -ForegroundColor White
        Write-Host "  - 3 Solicitudes de contacto" -ForegroundColor White
        Write-Host ""
        Write-Host "Puedes iniciar la aplicación con:" -ForegroundColor Cyan
        Write-Host "  cd src\cima.Blazor" -ForegroundColor Yellow
        Write-Host "  dotnet run" -ForegroundColor Yellow
    }
    else {
        Write-Host "ERROR: El DbMigrator falló con código $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
