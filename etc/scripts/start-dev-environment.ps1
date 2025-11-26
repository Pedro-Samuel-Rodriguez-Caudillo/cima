# Script para iniciar entorno de desarrollo con Docker
# Uso: ./etc/scripts/start-dev-environment.ps1

Write-Host "=== INICIANDO ENTORNO DE DESARROLLO ===" -ForegroundColor Cyan

# Verificar que existe archivo .env.development
if (-not (Test-Path ".env.development")) {
    Write-Host "Creando .env.development desde ejemplo..." -ForegroundColor Yellow
    Copy-Item ".env.development.example" ".env.development"
    Write-Host "IMPORTANTE: Edita .env.development con tus configuraciones" -ForegroundColor Red
    exit 1
}

# Detener contenedores anteriores si existen
Write-Host "`nDeteniendo contenedores anteriores..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml down 2>$null

# Construir imagenes
Write-Host "`nConstruyendo imagenes..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo al construir imagenes" -ForegroundColor Red
    exit 1
}

# Iniciar servicios
Write-Host "`nIniciando servicios..." -ForegroundColor Yellow
docker-compose -f docker-compose.dev.yml up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo al iniciar servicios" -ForegroundColor Red
    exit 1
}

# Esperar a que PostgreSQL este listo
Write-Host "`nEsperando a que PostgreSQL este listo..." -ForegroundColor Yellow
$maxRetries = 30
$retryCount = 0

while ($retryCount -lt $maxRetries) {
    $healthCheck = docker exec cima-postgres-dev pg_isready -U postgres 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "PostgreSQL listo!" -ForegroundColor Green
        break
    }
    
    $retryCount++
    Write-Host "  Intento $retryCount/$maxRetries..." -ForegroundColor Gray
    Start-Sleep -Seconds 2
}

if ($retryCount -eq $maxRetries) {
    Write-Host "ERROR: PostgreSQL no respondio a tiempo" -ForegroundColor Red
    docker-compose -f docker-compose.dev.yml logs postgres
    exit 1
}

# Esperar a que Blazor este listo
Write-Host "`nEsperando a que Blazor este listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

$blazorHealth = docker exec cima-blazor-dev curl -f http://localhost:8080/health 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "Blazor listo!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Blazor puede no estar listo aun" -ForegroundColor Yellow
}

# Mostrar estado
Write-Host "`n=== SERVICIOS INICIADOS ===" -ForegroundColor Green
docker-compose -f docker-compose.dev.yml ps

Write-Host "`nURLs de acceso:" -ForegroundColor Cyan
Write-Host "  Aplicacion: http://localhost:8080" -ForegroundColor White
Write-Host "  PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host "    - Database: cima_dev" -ForegroundColor Gray
Write-Host "    - User: postgres" -ForegroundColor Gray
Write-Host "    - Password: postgres_dev_password" -ForegroundColor Gray

Write-Host "`nComandos utiles:" -ForegroundColor Cyan
Write-Host "  Ver logs: docker-compose -f docker-compose.dev.yml logs -f" -ForegroundColor White
Write-Host "  Detener: docker-compose -f docker-compose.dev.yml down" -ForegroundColor White
Write-Host "  Reiniciar: docker-compose -f docker-compose.dev.yml restart" -ForegroundColor White
Write-Host ""
