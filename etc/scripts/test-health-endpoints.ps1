# Script para probar los endpoints de health check
# Asegurate de que la aplicacion este corriendo primero

param(
    [string]$BaseUrl = "http://localhost:8080"
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Test de Health Check Endpoints" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

# Funcion para probar un endpoint
function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Name
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Green
    Write-Host "URL: $Url" -ForegroundColor Gray
    Write-Host ""
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing -ErrorAction Stop
        
        Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Gray
        $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
        Write-Host ""
        
        return $true
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        return $false
    }
}

# Test 1: Ping endpoint (simple liveness)
Write-Host "1. PING ENDPOINT (Liveness)" -ForegroundColor Magenta
Write-Host "----------------------------" -ForegroundColor Magenta
$pingSuccess = Test-Endpoint -Url "$BaseUrl/api/health/ping" -Name "Ping"

# Test 2: Health endpoint completo
Write-Host "2. HEALTH ENDPOINT (Completo)" -ForegroundColor Magenta
Write-Host "------------------------------" -ForegroundColor Magenta
$healthSuccess = Test-Endpoint -Url "$BaseUrl/api/health" -Name "Health"

# Test 3: Ready endpoint (readiness)
Write-Host "3. READY ENDPOINT (Readiness)" -ForegroundColor Magenta
Write-Host "------------------------------" -ForegroundColor Magenta
$readySuccess = Test-Endpoint -Url "$BaseUrl/api/health/ready" -Name "Ready"

# Test 4: Health check endpoint nativo de ASP.NET Core
Write-Host "4. NATIVE HEALTH CHECK" -ForegroundColor Magenta
Write-Host "----------------------" -ForegroundColor Magenta
$nativeSuccess = Test-Endpoint -Url "$BaseUrl/health" -Name "Native Health Check"

# Resumen
Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$results = @{
    "Ping (/api/health/ping)" = $pingSuccess
    "Health (/api/health)" = $healthSuccess
    "Ready (/api/health/ready)" = $readySuccess
    "Native (/health)" = $nativeSuccess
}

foreach ($test in $results.GetEnumerator()) {
    $color = if ($test.Value) { "Green" } else { "Red" }
    $status = if ($test.Value) { "OK" } else { "FAILED" }
    Write-Host "$($test.Key): " -NoNewline
    Write-Host $status -ForegroundColor $color
}

Write-Host ""

# Verificar si todos pasaron
$allPassed = $results.Values | Where-Object { $_ -eq $false } | Measure-Object | Select-Object -ExpandProperty Count
if ($allPassed -eq 0) {
    Write-Host "EXITO: Todos los endpoints funcionan correctamente!" -ForegroundColor Green
} else {
    Write-Host "ADVERTENCIA: $allPassed endpoint(s) fallaron" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Verificar que:" -ForegroundColor Yellow
    Write-Host "1. La aplicacion este corriendo en $BaseUrl" -ForegroundColor Gray
    Write-Host "2. La base de datos este conectada" -ForegroundColor Gray
    Write-Host "3. No haya errores en los logs" -ForegroundColor Gray
}

Write-Host ""
