# Performance Audit Script
# Requires: npm install -g lighthouse

param(
    [string]$Url = "http://localhost:5000",
    [string]$OutputDir = "test-results/performance"
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "CIMA Performance Audit" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$reportPath = Join-Path $OutputDir "lighthouse-report-$timestamp.html"
$jsonPath = Join-Path $OutputDir "lighthouse-report-$timestamp.json"

Write-Host "Testing URL: $Url" -ForegroundColor Yellow
Write-Host "Output: $reportPath" -ForegroundColor Yellow
Write-Host ""

# Check if lighthouse is installed
try {
    $lighthouseVersion = lighthouse --version 2>&1
    Write-Host "Lighthouse version: $lighthouseVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Lighthouse not found. Install with: npm install -g lighthouse" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Running Lighthouse audit..." -ForegroundColor Cyan

# Run Lighthouse
$lighthouseArgs = @(
    $Url,
    "--output=html",
    "--output=json",
    "--output-path=$OutputDir/lighthouse-report-$timestamp",
    "--chrome-flags=`"--headless`"",
    "--only-categories=performance,accessibility,best-practices,seo"
)

try {
    & lighthouse @lighthouseArgs
    
    Write-Host ""
    Write-Host "================================" -ForegroundColor Green
    Write-Host "Audit Complete!" -ForegroundColor Green
    Write-Host "================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Reports saved to:" -ForegroundColor Yellow
    Write-Host "  HTML: $reportPath" -ForegroundColor White
    Write-Host "  JSON: $jsonPath" -ForegroundColor White
    Write-Host ""
    
    # Read and display key metrics from JSON
    if (Test-Path $jsonPath) {
        $report = Get-Content $jsonPath | ConvertFrom-Json
        
        Write-Host "Key Metrics:" -ForegroundColor Cyan
        Write-Host "  Performance: $($report.categories.performance.score * 100)%" -ForegroundColor $(if ($report.categories.performance.score -ge 0.9) { "Green" } elseif ($report.categories.performance.score -ge 0.5) { "Yellow" } else { "Red" })
        Write-Host "  Accessibility: $($report.categories.accessibility.score * 100)%" -ForegroundColor $(if ($report.categories.accessibility.score -ge 0.9) { "Green" } elseif ($report.categories.accessibility.score -ge 0.5) { "Yellow" } else { "Red" })
        Write-Host "  Best Practices: $($report.categories.'best-practices'.score * 100)%" -ForegroundColor $(if ($report.categories.'best-practices'.score -ge 0.9) { "Green" } elseif ($report.categories.'best-practices'.score -ge 0.5) { "Yellow" } else { "Red" })
        Write-Host "  SEO: $($report.categories.seo.score * 100)%" -ForegroundColor $(if ($report.categories.seo.score -ge 0.9) { "Green" } elseif ($report.categories.seo.score -ge 0.5) { "Yellow" } else { "Red" })
        
        Write-Host ""
        Write-Host "Performance Metrics:" -ForegroundColor Cyan
        $audits = $report.audits
        Write-Host "  First Contentful Paint: $($audits.'first-contentful-paint'.displayValue)" -ForegroundColor White
        Write-Host "  Largest Contentful Paint: $($audits.'largest-contentful-paint'.displayValue)" -ForegroundColor White
        Write-Host "  Total Blocking Time: $($audits.'total-blocking-time'.displayValue)" -ForegroundColor White
        Write-Host "  Cumulative Layout Shift: $($audits.'cumulative-layout-shift'.displayValue)" -ForegroundColor White
        Write-Host "  Speed Index: $($audits.'speed-index'.displayValue)" -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "Open the HTML report to see full details:" -ForegroundColor Yellow
    Write-Host "  $reportPath" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "ERROR running Lighthouse:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
