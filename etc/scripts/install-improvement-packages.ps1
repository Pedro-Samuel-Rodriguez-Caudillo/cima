# Script para instalar los paquetes NuGet de las mejoras
# Uso: ./etc/scripts/install-improvement-packages.ps1

Write-Host "=== INSTALANDO PAQUETES DE MEJORAS ===" -ForegroundColor Cyan

$blazorProject = "src/cima.Blazor/cima.Blazor.csproj"
$testProject = "test/cima.Application.Tests/cima.Application.Tests.csproj"

# Verificar que estamos en la raiz del proyecto
if (-not (Test-Path "cima.sln")) {
    Write-Host "ERROR: Ejecutar desde la raiz del proyecto (donde esta cima.sln)" -ForegroundColor Red
    exit 1
}

Write-Host "`n[1/3] Instalando paquetes de Redis Cache..." -ForegroundColor Yellow
dotnet add $blazorProject package Microsoft.Extensions.Caching.StackExchangeRedis --version 9.0.0

Write-Host "`n[2/3] Instalando paquetes de OpenTelemetry..." -ForegroundColor Yellow
dotnet add $blazorProject package OpenTelemetry --version 1.10.0
dotnet add $blazorProject package OpenTelemetry.Extensions.Hosting --version 1.10.0
dotnet add $blazorProject package OpenTelemetry.Instrumentation.AspNetCore --version 1.10.1
dotnet add $blazorProject package OpenTelemetry.Instrumentation.Http --version 1.10.0
dotnet add $blazorProject package OpenTelemetry.Instrumentation.Runtime --version 1.10.0
dotnet add $blazorProject package OpenTelemetry.Instrumentation.Process --version 0.5.0-beta.6
dotnet add $blazorProject package OpenTelemetry.Exporter.Console --version 1.10.0
dotnet add $blazorProject package OpenTelemetry.Exporter.OpenTelemetryProtocol --version 1.10.0
dotnet add $blazorProject package OpenTelemetry.Exporter.Prometheus.AspNetCore --version 1.9.0

Write-Host "`n[3/3] Instalando paquetes de Tests..." -ForegroundColor Yellow
dotnet add $testProject package Testcontainers.PostgreSql --version 4.0.0
dotnet add $testProject package FluentAssertions --version 6.12.2

Write-Host "`n=== INSTALACION COMPLETADA ===" -ForegroundColor Green

Write-Host "`nProximos pasos:" -ForegroundColor Cyan
Write-Host "1. Copiar configuracion de appsettings.Improvements.json a appsettings.json" -ForegroundColor White
Write-Host "2. En cimaBlazorModule.cs agregar:" -ForegroundColor White
Write-Host "   using cima.Blazor.Infrastructure;" -ForegroundColor Gray
Write-Host "3. En ConfigureServices agregar:" -ForegroundColor White
Write-Host "   context.Services.AddCimaImprovements(configuration, hostingEnvironment);" -ForegroundColor Gray
Write-Host "4. En OnApplicationInitialization (despues de UseRouting) agregar:" -ForegroundColor White
Write-Host "   app.UseCimaImprovements(configuration);" -ForegroundColor Gray
Write-Host ""
