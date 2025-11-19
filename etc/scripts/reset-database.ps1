# reset-database.ps1 - Resetear BD y reaplicar migraciones con permisos
Write-Host "===== RESETEO DE BASE DE DATOS CIMA =====" -ForegroundColor Cyan
Write-Host ""

$DbName = "cima_db"
$PgUser = "postgres"
$PgPassword = "postgres"
$PgHost = "localhost"  # Cambiado de $Host a $PgHost
$PgPort = "5432"
$ContainerName = "cima-postgres"

# Verificar si Docker está corriendo
Write-Host "[0/4] Verificando Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Docker no está corriendo" -ForegroundColor Red
        Write-Host "Inicia Docker Desktop e intenta de nuevo" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "    Docker OK" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Docker no encontrado" -ForegroundColor Red
    Write-Host "Asegurate de tener Docker Desktop instalado" -ForegroundColor Yellow
    exit 1
}

# Verificar si el contenedor existe
Write-Host "[1/4] Verificando contenedor PostgreSQL..." -ForegroundColor Yellow
$containerExists = docker ps -a --filter "name=$ContainerName" --format "{{.Names}}" | Select-String -Pattern $ContainerName

if (-not $containerExists) {
    Write-Host "ERROR: Contenedor '$ContainerName' no existe" -ForegroundColor Red
    Write-Host "Ejecuta primero: .\etc\scripts\setup-postgres-docker.ps1" -ForegroundColor Yellow
    exit 1
}

# Verificar si el contenedor está corriendo
$containerRunning = docker ps --filter "name=$ContainerName" --format "{{.Names}}" | Select-String -Pattern $ContainerName

if (-not $containerRunning) {
    Write-Host "    Contenedor detenido. Iniciando..." -ForegroundColor Yellow
    docker start $ContainerName | Out-Null
    Start-Sleep -Seconds 3
}

Write-Host "    Contenedor OK" -ForegroundColor Green

# 1. DROP DATABASE usando Docker exec
Write-Host "[2/4] Eliminando base de datos $DbName..." -ForegroundColor Yellow
docker exec -i $ContainerName psql -U $PgUser -c "DROP DATABASE IF EXISTS $DbName;" 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR al eliminar BD (puede que no exista)" -ForegroundColor Yellow
}

Write-Host "    BD eliminada/verificada" -ForegroundColor Green

# 2. CREATE DATABASE usando Docker exec
Write-Host "[3/4] Creando base de datos $DbName..." -ForegroundColor Yellow
docker exec -i $ContainerName psql -U $PgUser -c "CREATE DATABASE $DbName OWNER cima_user;" 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR al crear BD" -ForegroundColor Red
    Write-Host "Verificando si cima_user existe..." -ForegroundColor Yellow
    
    # Intentar crear el usuario si no existe
    docker exec -i $ContainerName psql -U $PgUser -c "CREATE USER cima_user WITH PASSWORD 'cima_pass';" 2>$null
    docker exec -i $ContainerName psql -U $PgUser -c "ALTER USER cima_user CREATEDB;" 2>$null
    
    # Reintentar crear BD
    docker exec -i $ContainerName psql -U $PgUser -c "CREATE DATABASE $DbName OWNER cima_user;" 2>$null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: No se pudo crear la BD" -ForegroundColor Red
        exit 1
    }
}

Write-Host "    BD creada OK" -ForegroundColor Green

# 3. RUN MIGRATOR
Write-Host "[4/4] Ejecutando DbMigrator..." -ForegroundColor Yellow
Write-Host "    (Esto puede tomar 30-60 segundos...)" -ForegroundColor Gray

$originalLocation = Get-Location
Set-Location "src\cima.DbMigrator"

dotnet run

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en migraciones" -ForegroundColor Red
    Set-Location $originalLocation
    exit 1
}

Set-Location $originalLocation

Write-Host ""
Write-Host "===== RESETEO COMPLETADO =====" -ForegroundColor Green
Write-Host ""
Write-Host "La base de datos ha sido reseteada con:" -ForegroundColor Cyan
Write-Host "  - Esquema completo (tablas de ABP + CIMA)" -ForegroundColor White
Write-Host "  - Usuario admin con permisos completos" -ForegroundColor White
Write-Host "  - Permisos de CIMA asignados al rol admin" -ForegroundColor White
Write-Host ""
Write-Host "Ahora puedes ejecutar las pruebas:" -ForegroundColor Yellow
Write-Host "  .\etc\scripts\test-api.ps1" -ForegroundColor Green
