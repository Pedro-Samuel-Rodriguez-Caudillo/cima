# Run All Tests Script
# Executes unit tests, component tests, and E2E tests

param(
    [switch]$SkipE2E,
    [switch]$SkipUnit,
    [switch]$SkipUI,
    [switch]$Coverage
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "CIMA Test Suite Runner" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$testResults = @{
    Domain = $null
    Application = $null
    UI = $null
    E2E = $null
}

$failed = $false

# Run Domain Tests
if (-not $SkipUnit) {
    Write-Host "Running Domain Tests..." -ForegroundColor Yellow
    Write-Host "-----------------------------------" -ForegroundColor Gray
    
    $result = dotnet test test/cima.Domain.Tests/cima.Domain.Tests.csproj --logger "console;verbosity=minimal"
    $testResults.Domain = $LASTEXITCODE
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[PASSED] Domain Tests" -ForegroundColor Green
    } else {
        Write-Host "[FAILED] Domain Tests" -ForegroundColor Red
        $failed = $true
    }
    Write-Host ""
}

# Run Application Tests
if (-not $SkipUnit) {
    Write-Host "Running Application Tests..." -ForegroundColor Yellow
    Write-Host "-----------------------------------" -ForegroundColor Gray
    
    $result = dotnet test test/cima.Application.Tests/cima.Application.Tests.csproj --logger "console;verbosity=minimal"
    $testResults.Application = $LASTEXITCODE
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[PASSED] Application Tests" -ForegroundColor Green
    } else {
        Write-Host "[FAILED] Application Tests" -ForegroundColor Red
        $failed = $true
    }
    Write-Host ""
}

# Run UI Component Tests
if (-not $SkipUI) {
    Write-Host "Running UI Component Tests..." -ForegroundColor Yellow
    Write-Host "-----------------------------------" -ForegroundColor Gray
    
    $result = dotnet test test/cima.Blazor.UITests/cima.Blazor.UITests.csproj --logger "console;verbosity=minimal"
    $testResults.UI = $LASTEXITCODE
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[PASSED] UI Component Tests" -ForegroundColor Green
    } else {
        Write-Host "[FAILED] UI Component Tests" -ForegroundColor Red
        $failed = $true
    }
    Write-Host ""
}

# Run E2E Tests
if (-not $SkipE2E) {
    Write-Host "Running E2E Tests..." -ForegroundColor Yellow
    Write-Host "-----------------------------------" -ForegroundColor Gray
    Write-Host "NOTE: E2E tests require the application to be running" -ForegroundColor Cyan
    Write-Host "Start the app with: dotnet run --project src/cima.Blazor/cima.Blazor.csproj" -ForegroundColor Cyan
    Write-Host ""
    
    $appRunning = $false
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000" -Method Head -TimeoutSec 2 -ErrorAction SilentlyContinue
        $appRunning = $true
    } catch {
        $appRunning = $false
    }
    
    if ($appRunning) {
        Write-Host "App is running, executing E2E tests..." -ForegroundColor Green
        
        # Install Playwright browsers if needed
        try {
            pwsh test/cima.Blazor.E2ETests/bin/Debug/net9.0/playwright.ps1 install chromium 2>&1 | Out-Null
        } catch {
            Write-Host "Note: Playwright browsers may need manual installation" -ForegroundColor Yellow
        }
        
        $result = dotnet test test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj --logger "console;verbosity=minimal"
        $testResults.E2E = $LASTEXITCODE
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[PASSED] E2E Tests" -ForegroundColor Green
        } else {
            Write-Host "[FAILED] E2E Tests" -ForegroundColor Red
            $failed = $true
        }
    } else {
        Write-Host "[SKIPPED] E2E Tests - Application not running" -ForegroundColor Yellow
        Write-Host "Start the app first: dotnet run --project src/cima.Blazor/cima.Blazor.csproj" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Summary
Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

if (-not $SkipUnit) {
    $domainStatus = if ($testResults.Domain -eq 0) { "[PASSED]" } else { "[FAILED]" }
    $domainColor = if ($testResults.Domain -eq 0) { "Green" } else { "Red" }
    Write-Host "$domainStatus Domain Tests" -ForegroundColor $domainColor
    
    $appStatus = if ($testResults.Application -eq 0) { "[PASSED]" } else { "[FAILED]" }
    $appColor = if ($testResults.Application -eq 0) { "Green" } else { "Red" }
    Write-Host "$appStatus Application Tests" -ForegroundColor $appColor
}

if (-not $SkipUI) {
    $uiStatus = if ($testResults.UI -eq 0) { "[PASSED]" } else { "[FAILED]" }
    $uiColor = if ($testResults.UI -eq 0) { "Green" } else { "Red" }
    Write-Host "$uiStatus UI Component Tests" -ForegroundColor $uiColor
}

if (-not $SkipE2E -and $testResults.E2E -ne $null) {
    $e2eStatus = if ($testResults.E2E -eq 0) { "[PASSED]" } else { "[FAILED]" }
    $e2eColor = if ($testResults.E2E -eq 0) { "Green" } else { "Red" }
    Write-Host "$e2eStatus E2E Tests" -ForegroundColor $e2eColor
}

Write-Host ""

if ($failed) {
    Write-Host "Some tests failed. Review the output above." -ForegroundColor Red
    exit 1
} else {
    Write-Host "All tests passed!" -ForegroundColor Green
    exit 0
}
