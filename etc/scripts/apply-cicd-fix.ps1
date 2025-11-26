# Script para aplicar las correcciones de CI/CD y hacer push

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FIX CI/CD - COMMIT Y PUSH" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en la raíz del proyecto
if (-not (Test-Path "cima.sln")) {
    Write-Host "ERROR: Este script debe ejecutarse desde la raíz del proyecto" -ForegroundColor Red
    exit 1
}

# Mostrar archivos modificados
Write-Host "[1/5] Archivos a commitear:" -ForegroundColor Green
Write-Host ""
git status --short
Write-Host ""

# Confirmar
$confirm = Read-Host "¿Proceder con el commit y push? (s/n)"
if ($confirm -ne "s" -and $confirm -ne "S") {
    Write-Host "Cancelado por el usuario" -ForegroundColor Yellow
    exit 0
}

Write-Host ""

# Add files
Write-Host "[2/5] Agregando archivos..." -ForegroundColor Green
git add .

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git add falló" -ForegroundColor Red
    exit 1
}

Write-Host "OK" -ForegroundColor White
Write-Host ""

# Commit
Write-Host "[3/5] Creando commit..." -ForegroundColor Green
git commit -F .git_commit_msg_fix_cicd.txt

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git commit falló" -ForegroundColor Red
    exit 1
}

Write-Host "OK" -ForegroundColor White
Write-Host ""

# Mostrar commit
Write-Host "[4/5] Commit creado:" -ForegroundColor Green
git log -1 --oneline
Write-Host ""

# Push
Write-Host "[5/5] Pushing a origin/master..." -ForegroundColor Green
git push origin master

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git push falló" -ForegroundColor Red
    Write-Host ""
    Write-Host "Puedes reintentar manualmente con:" -ForegroundColor Yellow
    Write-Host "  git push origin master" -ForegroundColor White
    exit 1
}

Write-Host "OK" -ForegroundColor White
Write-Host ""

# Éxito
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PUSH EXITOSO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Monitorear GitHub Actions:" -ForegroundColor White
Write-Host "   https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Verificar workflows:" -ForegroundColor White
Write-Host "   - CI - Build and Test (debe pasar en ~5-7 min)" -ForegroundColor Gray
Write-Host "   - CD - Deploy Production (debe pasar en ~8-10 min)" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Si falla:" -ForegroundColor White
Write-Host "   - Ver logs en GitHub Actions" -ForegroundColor Gray
Write-Host "   - Revisar docs/DIA_8_FIX_CICD.md" -ForegroundColor Gray
Write-Host ""
Write-Host "Abriendo GitHub Actions en el navegador..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Start-Process "https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/actions"
Write-Host ""
