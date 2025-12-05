# Script para ejecutar todas las pruebas del proyecto
# Incluye tests unitarios, de integración y E2E

param(
    [switch]$UnitOnly,
    [switch]$IntegrationOnly,
    [switch]$E2EOnly,
    [switch]$Verbose,
    [switch]$Coverage
)

$ErrorActionPreference = "Continue"
$testResults = @()
$startTime = Get-Date

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CIMA - Suite Completa de Pruebas" -ForegroundColor Cyan
Write-Host "  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para ejecutar tests
function Run-Tests {
    param(
        [string]$Project,
        [string]$Name,
        [string]$Filter = ""
    )
    
    Write-Host "`n[$Name]" -ForegroundColor Yellow
    Write-Host "Proyecto: $Project" -ForegroundColor Gray
    
    $args = @("test", $Project, "--no-build", "--verbosity", "minimal")
    
    if ($Filter) {
        $args += "--filter"
        $args += $Filter
    }
    
    if ($Coverage) {
        $args += "--collect:""XPlat Code Coverage"""
    }
    
    $result = & dotnet @args 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($Verbose) {
        $result | ForEach-Object { Write-Host $_ }
    }
    
    $passed = ($result | Select-String "Passed!" | Measure-Object).Count
    $failed = ($result | Select-String "Failed!" | Measure-Object).Count
    
    $status = if ($exitCode -eq 0) { "PASSED" } else { "FAILED" }
    $color = if ($exitCode -eq 0) { "Green" } else { "Red" }
    
    Write-Host "Estado: $status" -ForegroundColor $color
    
    return @{
        Name = $Name
        Status = $status
        ExitCode = $exitCode
        Passed = $passed
        Failed = $failed
    }
}

# Compilar solución primero
Write-Host "`nCompilando solución..." -ForegroundColor Yellow
$buildResult = dotnet build --configuration Release --verbosity minimal 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error de compilación!" -ForegroundColor Red
    $buildResult | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    exit 1
}
Write-Host "Compilación exitosa" -ForegroundColor Green

# Tests de Dominio (Unitarios)
if (-not $IntegrationOnly -and -not $E2EOnly) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  TESTS DE DOMINIO (Unitarios)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $testResults += Run-Tests `
        -Project "test\cima.Domain.Tests\cima.Domain.Tests.csproj" `
        -Name "Domain - Entities"
        
    $testResults += Run-Tests `
        -Project "test\cima.Domain.Tests\cima.Domain.Tests.csproj" `
        -Name "Domain - Specifications" `
        -Filter "FullyQualifiedName~Specifications"
        
    $testResults += Run-Tests `
        -Project "test\cima.Domain.Tests\cima.Domain.Tests.csproj" `
        -Name "Domain - Services" `
        -Filter "FullyQualifiedName~Services"
}

# Tests de Aplicación (Integración)
if (-not $UnitOnly -and -not $E2EOnly) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  TESTS DE APLICACIÓN (Integración)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $testResults += Run-Tests `
        -Project "test\cima.Application.Tests\cima.Application.Tests.csproj" `
        -Name "Application - ListingAppService" `
        -Filter "FullyQualifiedName~ListingAppServiceTests"
        
    $testResults += Run-Tests `
        -Project "test\cima.Application.Tests\cima.Application.Tests.csproj" `
        -Name "Application - StatisticsAppService" `
        -Filter "FullyQualifiedName~StatisticsAppServiceTests"
        
    $testResults += Run-Tests `
        -Project "test\cima.Application.Tests\cima.Application.Tests.csproj" `
        -Name "Application - ContactRequestAppService" `
        -Filter "FullyQualifiedName~ContactRequestAppServiceTests"
        
    $testResults += Run-Tests `
        -Project "test\cima.Application.Tests\cima.Application.Tests.csproj" `
        -Name "Application - ArchitectAppService" `
        -Filter "FullyQualifiedName~ArchitectAppServiceTests"
}

# Tests E2E (requiere servidor corriendo)
if (-not $UnitOnly -and -not $IntegrationOnly) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  TESTS E2E (End-to-End)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "NOTA: Requiere que el servidor esté corriendo en localhost:5000" -ForegroundColor Yellow
    
    # Verificar si el servidor está corriendo
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000" -TimeoutSec 5 -ErrorAction SilentlyContinue
        Write-Host "Servidor detectado, ejecutando tests E2E..." -ForegroundColor Green
        
        $testResults += Run-Tests `
            -Project "test\cima.Blazor.E2ETests\cima.Blazor.E2ETests.csproj" `
            -Name "E2E - Public Site" `
            -Filter "FullyQualifiedName~PublicSiteTests"
            
        $testResults += Run-Tests `
            -Project "test\cima.Blazor.E2ETests\cima.Blazor.E2ETests.csproj" `
            -Name "E2E - Admin Panel" `
            -Filter "FullyQualifiedName~AdminPanelTests"
            
        $testResults += Run-Tests `
            -Project "test\cima.Blazor.E2ETests\cima.Blazor.E2ETests.csproj" `
            -Name "E2E - Admin Listings" `
            -Filter "FullyQualifiedName~AdminListingsTests"
    }
    catch {
        Write-Host "Servidor no disponible. Saltando tests E2E." -ForegroundColor Yellow
        Write-Host "Para ejecutar tests E2E, inicia el servidor con: dotnet run --project src\cima.Blazor" -ForegroundColor Gray
    }
}

# Resumen
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DE RESULTADOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$totalPassed = 0
$totalFailed = 0

foreach ($result in $testResults) {
    $color = if ($result.Status -eq "PASSED") { "Green" } else { "Red" }
    $icon = if ($result.Status -eq "PASSED") { "[?]" } else { "[?]" }
    Write-Host "$icon $($result.Name): $($result.Status)" -ForegroundColor $color
    
    if ($result.Status -eq "PASSED") { $totalPassed++ }
    else { $totalFailed++ }
}

Write-Host ""
Write-Host "Total: $($testResults.Count) suites" -ForegroundColor Cyan
Write-Host "  Pasadas: $totalPassed" -ForegroundColor Green
Write-Host "  Fallidas: $totalFailed" -ForegroundColor $(if ($totalFailed -gt 0) { "Red" } else { "Gray" })
Write-Host "  Duración: $($duration.TotalSeconds.ToString("F1")) segundos" -ForegroundColor Gray
Write-Host ""

# Exit code
if ($totalFailed -gt 0) {
    Write-Host "Algunas pruebas fallaron!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "Todas las pruebas pasaron!" -ForegroundColor Green
    exit 0
}
