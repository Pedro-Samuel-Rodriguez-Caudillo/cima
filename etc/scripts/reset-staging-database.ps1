# reset-staging-database.ps1
# Script para limpiar datos corruptos de staging en Railway
# 
# USO:
#   .\etc\scripts\reset-staging-database.ps1 -ConnectionString "tu_connection_string"
#
# O puedes copiar el SQL y ejecutarlo directamente en Railway's Data tab

param(
    [string]$ConnectionString = ""
)

Write-Host "===== RESET DE DATOS STAGING =====" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Join-Path $PSScriptRoot "reset-staging-seeders.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: No se encuentra el script SQL: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "OPCIONES PARA EJECUTAR EL RESET:" -ForegroundColor Yellow
Write-Host ""
Write-Host "OPCION 1: Ejecutar via psql (si tienes PostgreSQL instalado localmente)" -ForegroundColor Cyan
Write-Host "-------------------------------------------------------------------" -ForegroundColor Gray
Write-Host ""
Write-Host '  psql "postgresql://USER:PASSWORD@HOST:PORT/DATABASE" -f "etc\scripts\reset-staging-seeders.sql"' -ForegroundColor White
Write-Host ""
Write-Host "  Reemplaza USER, PASSWORD, HOST, PORT, DATABASE con tus valores de Railway" -ForegroundColor Gray
Write-Host ""

Write-Host "OPCION 2: Usar Railway CLI" -ForegroundColor Cyan
Write-Host "-------------------------------------------------------------------" -ForegroundColor Gray
Write-Host ""
Write-Host "  1. Instala Railway CLI: npm install -g @railway/cli" -ForegroundColor White
Write-Host "  2. Autenticate: railway login" -ForegroundColor White
Write-Host "  3. Vincula proyecto: railway link" -ForegroundColor White
Write-Host "  4. Ejecuta: railway run psql -f etc/scripts/reset-staging-seeders.sql" -ForegroundColor White
Write-Host ""

Write-Host "OPCION 3: Panel de Railway (Data Tab)" -ForegroundColor Cyan
Write-Host "-------------------------------------------------------------------" -ForegroundColor Gray
Write-Host ""
Write-Host "  1. Ve a tu proyecto en Railway" -ForegroundColor White
Write-Host "  2. Click en tu servicio de PostgreSQL" -ForegroundColor White
Write-Host "  3. Ve a la pestana 'Data'" -ForegroundColor White
Write-Host "  4. Click en 'Query'" -ForegroundColor White
Write-Host "  5. Copia y pega el contenido del archivo SQL" -ForegroundColor White
Write-Host ""

Write-Host "OPCION 4: Herramientas GUI (DBeaver, pgAdmin, etc)" -ForegroundColor Cyan
Write-Host "-------------------------------------------------------------------" -ForegroundColor Gray
Write-Host ""
Write-Host "  Conectate con los datos de Railway y ejecuta el script SQL" -ForegroundColor White
Write-Host ""

# Si se proporciono connection string, intentar ejecutar
if ($ConnectionString -ne "") {
    Write-Host "Intentando ejecutar con connection string proporcionado..." -ForegroundColor Yellow
    
    # Verificar si psql esta disponible
    $psqlExists = Get-Command psql -ErrorAction SilentlyContinue
    
    if ($psqlExists) {
        Write-Host "Ejecutando script SQL..." -ForegroundColor Green
        psql $ConnectionString -f $scriptPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "===== LIMPIEZA COMPLETADA =====" -ForegroundColor Green
            Write-Host ""
            Write-Host "SIGUIENTE PASO:" -ForegroundColor Cyan
            Write-Host "  Redeploya tu aplicacion en Railway para que ejecute el seeding automatico" -ForegroundColor White
            Write-Host ""
            Write-Host "  railway up" -ForegroundColor Green
            Write-Host "  o haz push a la rama staging" -ForegroundColor Green
        } else {
            Write-Host "ERROR al ejecutar el script" -ForegroundColor Red
        }
    } else {
        Write-Host "psql no encontrado. Usa una de las otras opciones." -ForegroundColor Yellow
    }
} else {
    Write-Host "===== CONTENIDO DEL SCRIPT SQL =====" -ForegroundColor Cyan
    Write-Host "(Puedes copiar esto y pegarlo en Railway)" -ForegroundColor Gray
    Write-Host ""
    Get-Content $scriptPath | ForEach-Object { Write-Host $_ }
}

Write-Host ""
Write-Host "===== DESPUES DE LA LIMPIEZA =====" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Redeploya la aplicacion para que ejecute el seeding" -ForegroundColor White
Write-Host "2. El seeding creara automaticamente:" -ForegroundColor White
Write-Host "   - Usuario admin (admin@abp.io / 1q2w3E*)" -ForegroundColor Gray
Write-Host "   - Roles del sistema" -ForegroundColor Gray
Write-Host "   - Clientes OpenIddict (Blazor, Swagger, etc)" -ForegroundColor Gray
Write-Host "   - Permisos basicos" -ForegroundColor Gray
Write-Host ""
