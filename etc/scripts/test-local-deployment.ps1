# Test Local Deployment - Dia 8
# Este script prueba todos los componentes del Dia 8 localmente

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TEST LOCAL DEPLOYMENT - DIA 8" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"
$testsPassed = 0
$testsFailed = 0

# Funcion auxiliar para mostrar resultado
function Show-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Message = ""
    )
    
    if ($Success) {
        Write-Host "[PASS] $TestName" -ForegroundColor Green
        if ($Message) { Write-Host "       $Message" -ForegroundColor Gray }
        $script:testsPassed++
    } else {
        Write-Host "[FAIL] $TestName" -ForegroundColor Red
        if ($Message) { Write-Host "       $Message" -ForegroundColor Yellow }
        $script:testsFailed++
    }
}

# ===========================================
# TEST 1: Verificar archivos Docker
# ===========================================
Write-Host "`n=== TEST 1: Archivos Docker ===" -ForegroundColor Yellow

$dockerFiles = @(
    "src\cima.Blazor\Dockerfile",
    ".dockerignore",
    "docker-compose.dev.yml",
    "docker-compose.staging.yml",
    "docker-compose.prod.yml"
)

foreach ($file in $dockerFiles) {
    $exists = Test-Path $file
    Show-TestResult "Archivo existe: $file" $exists
}

# ===========================================
# TEST 2: Verificar archivos .env
# ===========================================
Write-Host "`n=== TEST 2: Archivos de Entorno ===" -ForegroundColor Yellow

$envFiles = @(
    ".env.development.example",
    ".env.staging.example",
    ".env.production.example"
)

foreach ($file in $envFiles) {
    $exists = Test-Path $file
    Show-TestResult "Archivo ejemplo: $file" $exists
}

# Verificar que archivos .env reales NO estan en Git
$gitignoreContent = Get-Content ".gitignore" -Raw
$hasEnvIgnore = $gitignoreContent -match "\.env\.development" -and 
                $gitignoreContent -match "\.env\.staging" -and 
                $gitignoreContent -match "\.env\.production"

Show-TestResult "Archivos .env en .gitignore" $hasEnvIgnore "Protege secrets de commits accidentales"

# ===========================================
# TEST 3: Verificar GitHub Actions
# ===========================================
Write-Host "`n=== TEST 3: GitHub Actions Workflows ===" -ForegroundColor Yellow

$workflowFiles = @(
    ".github\workflows\ci-build-test.yml",
    ".github\workflows\cd-deploy-staging.yml",
    ".github\workflows\cd-deploy-production.yml"
)

foreach ($file in $workflowFiles) {
    $exists = Test-Path $file
    Show-TestResult "Workflow: $file" $exists
}

# ===========================================
# TEST 4: Verificar Nginx
# ===========================================
Write-Host "`n=== TEST 4: Configuracion Nginx ===" -ForegroundColor Yellow

$nginxFiles = @(
    "etc\nginx\nginx.conf",
    "etc\nginx\conf.d\cima.conf"
)

foreach ($file in $nginxFiles) {
    $exists = Test-Path $file
    Show-TestResult "Config Nginx: $file" $exists
}

# ===========================================
# TEST 5: Verificar Scripts
# ===========================================
Write-Host "`n=== TEST 5: Scripts de Utilidad ===" -ForegroundColor Yellow

$scripts = @(
    "etc\scripts\start-dev-environment.ps1",
    "etc\scripts\stop-dev-environment.ps1",
    "etc\scripts\setup-ssl.sh",
    "etc\scripts\test-nginx-config.ps1",
    "etc\scripts\verificar-docker-cicd.ps1"
)

foreach ($script in $scripts) {
    $exists = Test-Path $script
    Show-TestResult "Script: $script" $exists
}

# ===========================================
# TEST 6: Verificar HealthController
# ===========================================
Write-Host "`n=== TEST 6: Health Check Controller ===" -ForegroundColor Yellow

$healthControllerPath = "src\cima.Blazor\Controllers\HealthController.cs"
$healthExists = Test-Path $healthControllerPath

Show-TestResult "HealthController existe" $healthExists

if ($healthExists) {
    $content = Get-Content $healthControllerPath -Raw
    
    $hasGetEndpoint = $content -match '\[HttpGet\]' -and $content -match 'public async Task<IActionResult> Get\(\)'
    Show-TestResult "Endpoint GET /health" $hasGetEndpoint
    
    $hasPingEndpoint = $content -match '\[HttpGet\("ping"\)\]'
    Show-TestResult "Endpoint GET /health/ping" $hasPingEndpoint
    
    $hasReadyEndpoint = $content -match '\[HttpGet\("ready"\)\]'
    Show-TestResult "Endpoint GET /health/ready" $hasReadyEndpoint
    
    $hasAllowAnonymous = $content -match '\[AllowAnonymous\]'
    Show-TestResult "AllowAnonymous attribute" $hasAllowAnonymous "Necesario para Docker health checks"
}

# ===========================================
# TEST 7: Verificar Dockerfile
# ===========================================
Write-Host "`n=== TEST 7: Dockerfile Validacion ===" -ForegroundColor Yellow

$dockerfilePath = "src\cima.Blazor\Dockerfile"
if (Test-Path $dockerfilePath) {
    $dockerContent = Get-Content $dockerfilePath -Raw
    
    $hasMultiStage = ($dockerContent -match 'AS base') -and ($dockerContent -match 'AS build')
    Show-TestResult "Multi-stage build" $hasMultiStage "Optimiza tamano de imagen"
    
    $hasRestore = $dockerContent -match 'dotnet restore'
    Show-TestResult "dotnet restore" $hasRestore
    
    $hasBuild = $dockerContent -match 'dotnet build'
    Show-TestResult "dotnet build" $hasBuild
    
    $hasPublish = $dockerContent -match 'dotnet publish'
    Show-TestResult "dotnet publish" $hasPublish
    
    $hasHealthCheck = $dockerContent -match 'HEALTHCHECK'
    Show-TestResult "HEALTHCHECK instruction" $hasHealthCheck
}

# ===========================================
# TEST 8: Probar Docker Build
# ===========================================
Write-Host "`n=== TEST 8: Docker Build (Opcional) ===" -ForegroundColor Yellow
Write-Host "Este test puede tardar varios minutos..." -ForegroundColor Gray

$buildDocker = Read-Host "`n¿Quieres probar el Docker build? (s/N)"

if ($buildDocker -eq 's' -or $buildDocker -eq 'S') {
    Write-Host "`nIniciando Docker build..." -ForegroundColor Cyan
    
    try {
        $buildOutput = docker build -f src\cima.Blazor\Dockerfile -t cima:test . 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Show-TestResult "Docker build exitoso" $true
            
            # Verificar que la imagen existe
            $imageExists = docker images | Select-String -Pattern "cima.*test"
            if ($imageExists) {
                Write-Host "`nImagen creada:" -ForegroundColor Green
                docker images | Select-String -Pattern "cima"
                
                # Limpiar imagen de prueba
                $cleanup = Read-Host "`n¿Eliminar imagen de prueba? (S/n)"
                if ($cleanup -ne 'n' -and $cleanup -ne 'N') {
                    docker rmi cima:test
                    Write-Host "Imagen eliminada" -ForegroundColor Gray
                }
            }
        } else {
            Show-TestResult "Docker build exitoso" $false "Exit code: $LASTEXITCODE"
            Write-Host "`nUltimas lineas del error:" -ForegroundColor Yellow
            $buildOutput | Select-Object -Last 20
        }
    } catch {
        Show-TestResult "Docker build exitoso" $false $_.Exception.Message
    }
} else {
    Write-Host "Docker build omitido (puedes ejecutarlo manualmente despues)" -ForegroundColor Gray
}

# ===========================================
# TEST 9: Probar Health Check Local
# ===========================================
Write-Host "`n=== TEST 9: Health Check Local (Opcional) ===" -ForegroundColor Yellow

$testHealth = Read-Host "`n¿Quieres iniciar la app y probar health checks? (s/N)"

if ($testHealth -eq 's' -or $testHealth -eq 'S') {
    Write-Host "`nIniciando aplicacion..." -ForegroundColor Cyan
    Write-Host "Presiona Ctrl+C cuando termine el test" -ForegroundColor Yellow
    
    # Iniciar app en background
    $appProcess = Start-Process -FilePath "dotnet" `
        -ArgumentList "run --project src\cima.Blazor\cima.Blazor.csproj" `
        -PassThru -NoNewWindow
    
    # Esperar a que la app inicie
    Write-Host "Esperando a que la aplicacion inicie (30 segundos)..." -ForegroundColor Gray
    Start-Sleep -Seconds 30
    
    # Probar endpoints
    try {
        Write-Host "`nProbando /health..." -ForegroundColor Cyan
        $healthResponse = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
        Show-TestResult "GET /health (HTTP $($healthResponse.StatusCode))" ($healthResponse.StatusCode -eq 200)
        
        Write-Host "`nProbando /health/ping..." -ForegroundColor Cyan
        $pingResponse = Invoke-WebRequest -Uri "http://localhost:5000/health/ping" -UseBasicParsing
        Show-TestResult "GET /health/ping (HTTP $($pingResponse.StatusCode))" ($pingResponse.StatusCode -eq 200)
        
        Write-Host "`nProbando /health/ready..." -ForegroundColor Cyan
        $readyResponse = Invoke-WebRequest -Uri "http://localhost:5000/health/ready" -UseBasicParsing
        Show-TestResult "GET /health/ready (HTTP $($readyResponse.StatusCode))" ($readyResponse.StatusCode -eq 200)
        
        # Mostrar respuesta de /health
        Write-Host "`nRespuesta de /health:" -ForegroundColor Green
        $healthResponse.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
        
    } catch {
        Show-TestResult "Health checks" $false $_.Exception.Message
        Write-Host "`nNOTA: Asegurate de que PostgreSQL esta corriendo" -ForegroundColor Yellow
    }
    
    # Detener aplicacion
    Write-Host "`nDeteniendo aplicacion..." -ForegroundColor Cyan
    Stop-Process -Id $appProcess.Id -Force
    Write-Host "Aplicacion detenida" -ForegroundColor Gray
    
} else {
    Write-Host "Health check local omitido" -ForegroundColor Gray
}

# ===========================================
# TEST 10: Validar Nginx Config
# ===========================================
Write-Host "`n=== TEST 10: Validar Sintaxis Nginx ===" -ForegroundColor Yellow

$testNginx = Read-Host "`n¿Quieres validar la sintaxis de Nginx? (S/n)"

if ($testNginx -ne 'n' -and $testNginx -ne 'N') {
    try {
        Write-Host "Ejecutando test de sintaxis Nginx..." -ForegroundColor Cyan
        
        $nginxTest = docker run --rm `
            -v "${PWD}/etc/nginx:/etc/nginx:ro" `
            nginx:alpine nginx -t 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Show-TestResult "Sintaxis Nginx valida" $true
            Write-Host $nginxTest -ForegroundColor Gray
        } else {
            Show-TestResult "Sintaxis Nginx valida" $false
            Write-Host $nginxTest -ForegroundColor Yellow
        }
    } catch {
        Show-TestResult "Test Nginx" $false $_.Exception.Message
    }
} else {
    Write-Host "Validacion Nginx omitida" -ForegroundColor Gray
}

# ===========================================
# RESUMEN FINAL
# ===========================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DE TESTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$totalTests = $testsPassed + $testsFailed
$successRate = if ($totalTests -gt 0) { [math]::Round(($testsPassed / $totalTests) * 100, 2) } else { 0 }

Write-Host "`nTests Pasados:  $testsPassed" -ForegroundColor Green
Write-Host "Tests Fallados: $testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host "Total:          $totalTests" -ForegroundColor Cyan
Write-Host "Exito:          $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 60) { "Yellow" } else { "Red" })

Write-Host "`n========================================" -ForegroundColor Cyan

if ($testsFailed -eq 0) {
    Write-Host "`nTODOS LOS TESTS PASARON!" -ForegroundColor Green
    Write-Host "El Dia 8 DevOps esta listo para deployment" -ForegroundColor Green
} elseif ($successRate -ge 80) {
    Write-Host "`nMAYORIA DE TESTS PASARON" -ForegroundColor Yellow
    Write-Host "Revisa los fallos antes de deployment" -ForegroundColor Yellow
} else {
    Write-Host "`nVARIOS TESTS FALLARON" -ForegroundColor Red
    Write-Host "Corrige los errores antes de continuar" -ForegroundColor Red
}

Write-Host "`nProximos pasos:" -ForegroundColor Cyan
Write-Host "1. Si todo esta OK, ejecuta: .\etc\scripts\start-dev-environment.ps1" -ForegroundColor White
Write-Host "2. Revisa docs/DEPLOYMENT_GUIDE.md para deployment a produccion" -ForegroundColor White
Write-Host "3. Cuando tengas dominio, sigue docs/CONFIGURACION_DOMINIO_PENDIENTE.md" -ForegroundColor White
Write-Host ""
