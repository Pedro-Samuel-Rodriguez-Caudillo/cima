# Script para iniciar Tailwind CSS en modo watch
# Uso: .\etc\scripts\start-tailwind-watch.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Iniciando Tailwind CSS Watch Mode  " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$clientPath = "src\cima.Blazor.Client"

# Verificar que existe el directorio
if (-not (Test-Path $clientPath)) {
    Write-Host "ERROR: No se encontró el directorio $clientPath" -ForegroundColor Red
    exit 1
}

# Verificar que existe node_modules
if (-not (Test-Path "$clientPath\node_modules")) {
    Write-Host "ADVERTENCIA: No se encontró node_modules. Instalando dependencias..." -ForegroundColor Yellow
    Push-Location $clientPath
    npm install
    Pop-Location
}

Write-Host "Iniciando Tailwind CSS en modo watch..." -ForegroundColor Green
Write-Host "Presiona Ctrl+C para detener el proceso" -ForegroundColor Yellow
Write-Host ""

# Cambiar al directorio del cliente y ejecutar npm watch
Push-Location $clientPath
try {
    npm run watch:css
}
finally {
    Pop-Location
}
