# actualizar-migraciones.ps1 - Actualizar migraciones de EF Core
param(
    [string]$NombreMigracion = "",
    [switch]$Aplicar = $false,
    [switch]$Limpiar = $false
)

Write-Host "===== ACTUALIZACION DE MIGRACIONES EF CORE =====" -ForegroundColor Cyan
Write-Host ""

$rootPath = Get-Location
$efCoreProject = "src\cima.EntityFrameworkCore"

# Verificar que existe el proyecto
if (-not (Test-Path (Join-Path $rootPath $efCoreProject))) {
    Write-Host "ERROR: No se encuentra el proyecto $efCoreProject" -ForegroundColor Red
    exit 1
}

# Navegar al proyecto
Set-Location (Join-Path $rootPath $efCoreProject)

# PASO 1: Limpiar migraciones antiguas (opcional)
if ($Limpiar) {
    Write-Host "[1/4] Limpiando migraciones antiguas..." -ForegroundColor Yellow
    
    # Listar migraciones actuales
    Write-Host "    Migraciones actuales:" -ForegroundColor Gray
    dotnet ef migrations list
    
    Write-Host ""
    $confirmacion = Read-Host "Deseas eliminar TODAS las migraciones? (s/N)"
    
    if ($confirmacion -eq 's' -or $confirmacion -eq 'S') {
        # Eliminar todas las migraciones
        $migrations = Get-ChildItem "Migrations" -Filter "*.cs" | Where-Object { $_.Name -ne "cimaDbContextModelSnapshot.cs" }
        
        foreach ($migration in $migrations) {
            Write-Host "    Eliminando: $($migration.Name)" -ForegroundColor Gray
            Remove-Item $migration.FullName -Force
        }
        
        # Eliminar el snapshot
        $snapshot = "Migrations\cimaDbContextModelSnapshot.cs"
        if (Test-Path $snapshot) {
            Remove-Item $snapshot -Force
            Write-Host "    Snapshot eliminado" -ForegroundColor Gray
        }
        
        Write-Host "    Migraciones eliminadas OK" -ForegroundColor Green
    } else {
        Write-Host "    Limpieza cancelada" -ForegroundColor Yellow
    }
    Write-Host ""
}

# PASO 2: Verificar cambios pendientes
Write-Host "[2/4] Verificando cambios en el modelo..." -ForegroundColor Yellow

$hasChanges = $false

try {
    # Intentar agregar una migracion temporal para detectar cambios
    $tempMigrationName = "TempCheck_$(Get-Date -Format 'yyyyMMddHHmmss')"
    dotnet ef migrations add $tempMigrationName --no-build 2>&1 | Tee-Object -Variable output | Out-Null
    
    if ($output -like "*No changes were detected*") {
        Write-Host "    No hay cambios pendientes en el modelo" -ForegroundColor Green
        
        # Eliminar la migracion temporal
        dotnet ef migrations remove --force | Out-Null
        $hasChanges = $false
    } else {
        Write-Host "    Se detectaron cambios en el modelo" -ForegroundColor Yellow
        
        # Eliminar la migracion temporal
        dotnet ef migrations remove --force | Out-Null
        $hasChanges = $true
    }
} catch {
    Write-Host "    Error al verificar cambios" -ForegroundColor Red
    Write-Host "    $_" -ForegroundColor Gray
}

Write-Host ""

# PASO 3: Crear nueva migracion
if ($hasChanges -or $NombreMigracion -ne "") {
    Write-Host "[3/4] Creando nueva migracion..." -ForegroundColor Yellow
    
    if ($NombreMigracion -eq "") {
        $NombreMigracion = Read-Host "Nombre de la migracion (ej: AgregarCampoXYZ)"
        
        if ($NombreMigracion -eq "") {
            Write-Host "ERROR: Debes proporcionar un nombre para la migracion" -ForegroundColor Red
            Set-Location $rootPath
            exit 1
        }
    }
    
    Write-Host "    Generando migracion: $NombreMigracion" -ForegroundColor Gray
    
    # Compilar primero
    Write-Host "    Compilando proyecto..." -ForegroundColor Gray
    dotnet build --no-restore -c Debug
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Fallo la compilacion" -ForegroundColor Red
        Set-Location $rootPath
        exit 1
    }
    
    # Crear migracion
    dotnet ef migrations add $NombreMigracion
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Fallo la creacion de la migracion" -ForegroundColor Red
        Set-Location $rootPath
        exit 1
    }
    
    Write-Host "    Migracion creada OK" -ForegroundColor Green
    Write-Host ""
    
    # Mostrar archivos generados
    Write-Host "    Archivos generados:" -ForegroundColor Cyan
    Get-ChildItem "Migrations" -Filter "*$NombreMigracion*" | ForEach-Object {
        Write-Host "      - $($_.Name)" -ForegroundColor Gray
    }
    Write-Host ""
} else {
    Write-Host "[3/4] No se requiere nueva migracion" -ForegroundColor Green
    Write-Host ""
}

# PASO 4: Aplicar migraciones a la BD
if ($Aplicar) {
    Write-Host "[4/4] Aplicando migraciones a la base de datos..." -ForegroundColor Yellow
    
    # Volver a la raiz y ejecutar DbMigrator
    Set-Location $rootPath
    Set-Location "src\cima.DbMigrator"
    
    Write-Host "    Ejecutando DbMigrator..." -ForegroundColor Gray
    dotnet run
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Fallo la ejecucion del DbMigrator" -ForegroundColor Red
        Set-Location $rootPath
        exit 1
    }
    
    Write-Host "    Migraciones aplicadas OK" -ForegroundColor Green
} else {
    Write-Host "[4/4] Aplicacion de migraciones omitida (usa -Aplicar para ejecutar)" -ForegroundColor Yellow
}

# Volver a la raiz
Set-Location $rootPath

Write-Host ""
Write-Host "===== RESUMEN =====" -ForegroundColor Cyan
Write-Host ""

# Listar todas las migraciones
Write-Host "Migraciones actuales:" -ForegroundColor Yellow
Set-Location (Join-Path $rootPath $efCoreProject)
dotnet ef migrations list

Set-Location $rootPath

Write-Host ""
Write-Host "===== COMANDOS UTILES =====" -ForegroundColor Cyan
Write-Host "Crear migracion:               .\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion 'MiCambio'" -ForegroundColor White
Write-Host "Crear y aplicar:               .\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion 'MiCambio' -Aplicar" -ForegroundColor White
Write-Host "Limpiar todas y recrear:       .\etc\scripts\actualizar-migraciones.ps1 -Limpiar -NombreMigracion 'InitialCreate' -Aplicar" -ForegroundColor White
Write-Host "Ver migraciones:               cd src\cima.EntityFrameworkCore; dotnet ef migrations list" -ForegroundColor White
Write-Host "Eliminar ultima migracion:     cd src\cima.EntityFrameworkCore; dotnet ef migrations remove" -ForegroundColor White
Write-Host "Ver SQL de migracion:          cd src\cima.EntityFrameworkCore; dotnet ef migrations script" -ForegroundColor White
Write-Host ""
Write-Host "Aplicar con DbMigrator:        cd src\cima.DbMigrator; dotnet run" -ForegroundColor White
Write-Host "Reset completo BD:             .\etc\scripts\reset-database.ps1" -ForegroundColor White
