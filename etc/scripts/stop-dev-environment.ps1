# Script para detener entorno de desarrollo
# Uso: ./etc/scripts/stop-dev-environment.ps1

param(
    [switch]$Clean = $false
)

Write-Host "=== DETENIENDO ENTORNO DE DESARROLLO ===" -ForegroundColor Cyan

if ($Clean) {
    Write-Host "`nModo LIMPIEZA activado - se eliminaran volumenes y datos" -ForegroundColor Yellow
    $confirm = Read-Host "Estas seguro? (s/n)"
    
    if ($confirm -ne "s") {
        Write-Host "Operacion cancelada" -ForegroundColor Gray
        exit 0
    }
    
    # Detener y eliminar todo
    docker-compose -f docker-compose.dev.yml down -v
    
    Write-Host "`nLimpiando imagenes huerfanas..." -ForegroundColor Yellow
    docker image prune -f
    
    Write-Host "`nTODO LIMPIADO - Base de datos y volumenes eliminados" -ForegroundColor Green
} else {
    # Solo detener contenedores
    docker-compose -f docker-compose.dev.yml down
    
    Write-Host "`nContenedores detenidos - datos preservados" -ForegroundColor Green
    Write-Host "Para eliminar datos usar: ./etc/scripts/stop-dev-environment.ps1 -Clean" -ForegroundColor Gray
}

Write-Host ""
