# Script para probar configuracion de Nginx
# Uso: ./etc/scripts/test-nginx-config.ps1

Write-Host "=== VERIFICACION DE CONFIGURACION NGINX ===" -ForegroundColor Cyan

# Verificar que los archivos existen
$requiredFiles = @(
    "etc/nginx/nginx.conf",
    "etc/nginx/conf.d/cima.conf"
)

Write-Host "`n[1/4] Verificando archivos de configuracion..." -ForegroundColor Yellow
$missing = @()
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "  OK: $file" -ForegroundColor Green
    } else {
        Write-Host "  FALTA: $file" -ForegroundColor Red
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    Write-Host "`nERROR: Faltan archivos de configuracion" -ForegroundColor Red
    exit 1
}

# Probar sintaxis de Nginx (requiere Docker)
Write-Host "`n[2/4] Probando sintaxis de Nginx..." -ForegroundColor Yellow
try {
    $testResult = docker run --rm `
        -v "${PWD}/etc/nginx/nginx.conf:/etc/nginx/nginx.conf:ro" `
        -v "${PWD}/etc/nginx/conf.d:/etc/nginx/conf.d:ro" `
        nginx:alpine nginx -t 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  OK: Sintaxis de Nginx valida" -ForegroundColor Green
    } else {
        Write-Host "  ERROR: Sintaxis invalida" -ForegroundColor Red
        Write-Host $testResult -ForegroundColor Gray
        exit 1
    }
} catch {
    Write-Host "  WARNING: No se pudo probar sintaxis (Docker no disponible?)" -ForegroundColor Yellow
}

# Verificar que docker-compose.prod.yml usa Nginx
Write-Host "`n[3/4] Verificando docker-compose.prod.yml..." -ForegroundColor Yellow
if (Test-Path "docker-compose.prod.yml") {
    $composeContent = Get-Content "docker-compose.prod.yml" -Raw
    
    if ($composeContent -match "nginx:") {
        Write-Host "  OK: Servicio nginx configurado" -ForegroundColor Green
    } else {
        Write-Host "  WARNING: Servicio nginx no encontrado en docker-compose.prod.yml" -ForegroundColor Yellow
    }
    
    if ($composeContent -match "./etc/nginx/nginx.conf") {
        Write-Host "  OK: Volumenes de configuracion mapeados" -ForegroundColor Green
    } else {
        Write-Host "  WARNING: Volumenes de Nginx no configurados" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ERROR: docker-compose.prod.yml no encontrado" -ForegroundColor Red
    exit 1
}

# Mostrar resumen de configuracion
Write-Host "`n[4/4] Resumen de configuracion..." -ForegroundColor Yellow
Write-Host "  Puertos HTTP: 80" -ForegroundColor White
Write-Host "  Puertos HTTPS: 443" -ForegroundColor White
Write-Host "  Upstream: blazor-prod:8080" -ForegroundColor White
Write-Host "  Client max body size: 20MB" -ForegroundColor White
Write-Host "  SSL: Let's Encrypt" -ForegroundColor White
Write-Host "  Rate limiting: Habilitado" -ForegroundColor White
Write-Host "  Gzip: Habilitado" -ForegroundColor White

Write-Host "`n=== VERIFICACION COMPLETADA ===" -ForegroundColor Green
Write-Host "`nProximos pasos:" -ForegroundColor Cyan
Write-Host "1. Configurar DNS: cima.com -> IP del servidor" -ForegroundColor White
Write-Host "2. En servidor: sudo ./etc/scripts/setup-ssl.sh cima.com admin@cima.com" -ForegroundColor White
Write-Host "3. Iniciar: docker-compose -f docker-compose.prod.yml up -d" -ForegroundColor White
Write-Host "4. Verificar: https://cima.com/health" -ForegroundColor White
Write-Host ""