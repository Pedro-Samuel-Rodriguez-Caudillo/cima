# Script para probar el Dockerfile localmente
# Ejecutar desde la raíz del proyecto

param(
    [string]$ImageTag = "cima:test-local",
    [switch]$NoBuild,
    [switch]$Run,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TEST DOCKER BUILD LOCAL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en la raíz del proyecto
if (-not (Test-Path "cima.sln")) {
    Write-Host "ERROR: Este script debe ejecutarse desde la raíz del proyecto" -ForegroundColor Red
    Write-Host "Directorio actual: $(Get-Location)" -ForegroundColor Gray
    exit 1
}

# Limpiar imagenes anteriores si se solicita
if ($Clean) {
    Write-Host "Limpiando imagenes anteriores..." -ForegroundColor Yellow
    docker images | Select-String -Pattern "cima" | ForEach-Object {
        $imageLine = $_ -split '\s+'
        $imageId = $imageLine[2]
        Write-Host "  Eliminando imagen: $imageId" -ForegroundColor Gray
        docker rmi -f $imageId 2>$null
    }
    Write-Host ""
}

# Build de la imagen
if (-not $NoBuild) {
    Write-Host "[1/3] Building Docker image..." -ForegroundColor Green
    Write-Host "Comando: docker build -f src/cima.Blazor/Dockerfile -t $ImageTag ." -ForegroundColor Gray
    Write-Host ""
    
    $buildStart = Get-Date
    docker build -f src/cima.Blazor/Dockerfile -t $ImageTag .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Docker build falló" -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "1. Verificar que Docker Desktop está corriendo" -ForegroundColor Gray
        Write-Host "2. Verificar que tienes espacio en disco" -ForegroundColor Gray
        Write-Host "3. Revisar errores arriba" -ForegroundColor Gray
        Write-Host ""
        exit 1
    }
    
    $buildEnd = Get-Date
    $buildDuration = $buildEnd - $buildStart
    
    Write-Host ""
    Write-Host "BUILD EXITOSO en $($buildDuration.TotalMinutes.ToString('0.00')) minutos" -ForegroundColor Green
    Write-Host ""
}

# Verificar imagen creada
Write-Host "[2/3] Verificando imagen creada..." -ForegroundColor Green
$imageInfo = docker images $ImageTag --format "{{.Repository}}:{{.Tag}} | Size: {{.Size}} | Created: {{.CreatedSince}}"

if ($imageInfo) {
    Write-Host "Imagen creada:" -ForegroundColor White
    Write-Host "  $imageInfo" -ForegroundColor Cyan
} else {
    Write-Host "ERROR: No se encontró la imagen $ImageTag" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Ejecutar contenedor si se solicita
if ($Run) {
    Write-Host "[3/3] Ejecutando contenedor de prueba..." -ForegroundColor Green
    Write-Host ""
    
    # Detener contenedor anterior si existe
    docker stop cima-test-local 2>$null
    docker rm cima-test-local 2>$null
    
    # Ejecutar nuevo contenedor
    Write-Host "Iniciando contenedor en puerto 8085..." -ForegroundColor Yellow
    docker run -d `
        --name cima-test-local `
        -p 8085:8080 `
        -e ASPNETCORE_ENVIRONMENT=Development `
        -e ConnectionStrings__Default="Host=host.docker.internal;Port=5433;Database=cima_dev;Username=cima;Password=YourPasswordHere" `
        $ImageTag
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: No se pudo iniciar el contenedor" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "Esperando que la aplicación inicie (30 segundos)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Probar health check
    Write-Host ""
    Write-Host "Probando health check..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8085/api/health/ping" -UseBasicParsing -TimeoutSec 5
        
        if ($response.StatusCode -eq 200) {
            Write-Host "Health check OK!" -ForegroundColor Green
            Write-Host "Response:" -ForegroundColor Gray
            $response.Content
        } else {
            Write-Host "Health check retornó status: $($response.StatusCode)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "WARNING: Health check falló - $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "Esto puede ser normal si la BD no está disponible" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Ver logs del contenedor:" -ForegroundColor Cyan
    Write-Host "  docker logs cima-test-local" -ForegroundColor White
    Write-Host ""
    Write-Host "Detener contenedor:" -ForegroundColor Cyan
    Write-Host "  docker stop cima-test-local" -ForegroundColor White
    Write-Host "  docker rm cima-test-local" -ForegroundColor White
    Write-Host ""
    Write-Host "Acceder a la aplicación:" -ForegroundColor Cyan
    Write-Host "  http://localhost:8085" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "[3/3] Para ejecutar la imagen:" -ForegroundColor Green
    Write-Host ""
    Write-Host "docker run -d ``" -ForegroundColor White
    Write-Host "  --name cima-test-local ``" -ForegroundColor White
    Write-Host "  -p 8085:8080 ``" -ForegroundColor White
    Write-Host "  -e ASPNETCORE_ENVIRONMENT=Development ``" -ForegroundColor White
    Write-Host "  -e ConnectionStrings__Default='Host=host.docker.internal;...' ``" -ForegroundColor White
    Write-Host "  $ImageTag" -ForegroundColor White
    Write-Host ""
    Write-Host "O ejecutar este script con -Run:" -ForegroundColor Yellow
    Write-Host "  .\etc\scripts\test-docker-build.ps1 -Run" -ForegroundColor White
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Imagen: $ImageTag" -ForegroundColor White
Write-Host "Estado: BUILD EXITOSO" -ForegroundColor Green

if ($Run) {
    Write-Host "Contenedor: cima-test-local (corriendo en http://localhost:8085)" -ForegroundColor White
}

Write-Host ""
Write-Host "Siguiente paso: Probar en GitHub Actions" -ForegroundColor Cyan
Write-Host "  git add ." -ForegroundColor White
Write-Host "  git commit -m 'fix(docker): corregir rutas en Dockerfile'" -ForegroundColor White
Write-Host "  git push origin master" -ForegroundColor White
Write-Host ""
