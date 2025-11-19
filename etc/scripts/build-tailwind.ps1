# Script para compilar Tailwind CSS para producción
# Uso: .\etc\scripts\build-tailwind.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Compilando Tailwind CSS (Prod)     " -ForegroundColor Cyan
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

Write-Host "Compilando CSS optimizado para producción..." -ForegroundColor Green

# Cambiar al directorio del cliente y ejecutar npm build
Push-Location $clientPath
try {
    npm run build:css
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "? CSS compilado exitosamente" -ForegroundColor Green
        
        # Mostrar tamaño del archivo
        $cssFile = "wwwroot\css\app.min.css"
        if (Test-Path $cssFile) {
            $size = (Get-Item $cssFile).Length
            $sizeKB = [math]::Round($size / 1KB, 2)
            Write-Host "  Tamaño del archivo: $sizeKB KB" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host ""
        Write-Host "? Error al compilar CSS" -ForegroundColor Red
        exit 1
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Proceso completado                 " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
