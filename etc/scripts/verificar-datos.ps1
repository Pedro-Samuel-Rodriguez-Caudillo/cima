# Script para verificar y forzar seed de datos

Write-Host "=== Verificando datos en BD ===" -ForegroundColor Cyan

# Verificar si existe psql
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlExists) {
    Write-Host "`nConteo de registros:" -ForegroundColor Yellow
    
    $env:PGPASSWORD = "cima2024"
    
    # Contar listings
    $listingsCount = psql -h localhost -U cima_user -d cima -t -c "SELECT COUNT(*) FROM \"Listings\";" 2>$null
    Write-Host "Listings: $listingsCount" -ForegroundColor $(if ($listingsCount -gt 0) { "Green" } else { "Red" })
    
    # Contar featured
    $featuredCount = psql -h localhost -U cima_user -d cima -t -c "SELECT COUNT(*) FROM \"FeaturedListings\";" 2>$null
    Write-Host "Featured Listings: $featuredCount" -ForegroundColor $(if ($featuredCount -gt 0) { "Green" } else { "Red" })
    
    # Contar arquitectos
    $architectsCount = psql -h localhost -U cima_user -d cima -t -c "SELECT COUNT(*) FROM \"Architects\";" 2>$null
    Write-Host "Architects: $architectsCount" -ForegroundColor $(if ($architectsCount -gt 0) { "Green" } else { "Red" })
    
    # Si no hay datos, ofrecer borrar todo
    if ($listingsCount -eq 0 -or $listingsCount -eq $null) {
        Write-Host "`n¿Deseas BORRAR todos los datos y ejecutar el seeder nuevamente? (S/N)" -ForegroundColor Yellow
        $response = Read-Host
        
        if ($response -eq 'S' -or $response -eq 's') {
            Write-Host "`nBorrando datos existentes..." -ForegroundColor Yellow
            
            psql -h localhost -U cima_user -d cima -c "TRUNCATE TABLE \"FeaturedListings\" CASCADE;" 2>$null
            psql -h localhost -U cima_user -d cima -c "TRUNCATE TABLE \"Listings\" CASCADE;" 2>$null
            psql -h localhost -U cima_user -d cima -c "TRUNCATE TABLE \"Architects\" CASCADE;" 2>$null
            psql -h localhost -U cima_user -d cima -c "TRUNCATE TABLE \"ContactRequests\" CASCADE;" 2>$null
            
            Write-Host "Datos borrados. Ejecutando DbMigrator..." -ForegroundColor Cyan
            
            Push-Location "src\cima.DbMigrator"
            dotnet run
            Pop-Location
            
            Write-Host "`nVerificando datos nuevamente..." -ForegroundColor Cyan
            $listingsCount = psql -h localhost -U cima_user -d cima -t -c "SELECT COUNT(*) FROM \"Listings\";"
            Write-Host "Listings: $listingsCount" -ForegroundColor Green
        }
    }
    else {
        Write-Host "`n¡Hay datos en la BD!" -ForegroundColor Green
        
        # Mostrar algunas propiedades
        Write-Host "`nÚltimas 5 propiedades:" -ForegroundColor Cyan
        psql -h localhost -U cima_user -d cima -c "SELECT \"Title\", \"Status\", \"Price\" FROM \"Listings\" ORDER BY \"CreatedAt\" DESC LIMIT 5;"
    }
}
else {
    Write-Host "`npsql no está disponible. Usando API..." -ForegroundColor Yellow
    Write-Host "Abre la app y verifica manualmente:" -ForegroundColor Cyan
    Write-Host "https://localhost:44365/propiedades" -ForegroundColor White
}
