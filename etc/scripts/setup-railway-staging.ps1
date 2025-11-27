# Script para configurar Railway Staging

param(
    [switch]$SkipPush,
    [switch]$ShowVariables
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RAILWAY STAGING SETUP" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar rama actual
$currentBranch = git branch --show-current

if ($currentBranch -ne "develop") {
    Write-Host "ERROR: Debes estar en la rama 'develop'" -ForegroundColor Red
    Write-Host "Rama actual: $currentBranch" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Ejecuta:" -ForegroundColor Cyan
    Write-Host "  git checkout develop" -ForegroundColor White
    exit 1
}

Write-Host "? Rama actual: develop" -ForegroundColor Green
Write-Host ""

# Verificar archivos Railway
Write-Host "[1/5] Verificando archivos Railway..." -ForegroundColor Yellow

$railwayFiles = @("railway.json", "Procfile")
$allFilesExist = $true

foreach ($file in $railwayFiles) {
    if (Test-Path $file) {
        Write-Host "  ? $file existe" -ForegroundColor Green
    } else {
        Write-Host "  ? FALTA: $file" -ForegroundColor Red
        $allFilesExist = $false
    }
}

if (-not $allFilesExist) {
    Write-Host ""
    Write-Host "ERROR: Faltan archivos necesarios" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Verificar cambios
Write-Host "[2/5] Verificando cambios pendientes..." -ForegroundColor Yellow

$status = git status --porcelain

if ($status) {
    Write-Host "  Cambios pendientes de commit:" -ForegroundColor Yellow
    git status --short
    Write-Host ""
    
    $commit = Read-Host "¿Quieres hacer commit ahora? (s/n)"
    
    if ($commit -eq "s" -or $commit -eq "S") {
        Write-Host ""
        Write-Host "Agregando archivos..." -ForegroundColor Cyan
        git add railway.json Procfile docs/GIT_BRANCHING_STRATEGY.md docs/RAILWAY_STAGING_CONFIG.md .github/workflows/cd-deploy-staging.yml
        
        Write-Host "Creando commit..." -ForegroundColor Cyan
        git commit -m "ci(railway): add Railway configuration for staging

- Add railway.json with Dockerfile builder config
- Add Procfile for Railway startup command
- Add branching strategy documentation
- Update GitHub Actions workflow for develop branch
- Configure develop branch for Railway staging deployment

Branch: develop ? Railway staging environment"
        
        Write-Host "? Commit creado" -ForegroundColor Green
    }
} else {
    Write-Host "  ? No hay cambios pendientes" -ForegroundColor Green
}

Write-Host ""

# Push a GitHub
if (-not $SkipPush) {
    Write-Host "[3/5] Pushing a GitHub..." -ForegroundColor Yellow
    
    $push = Read-Host "¿Quieres hacer push de la rama develop? (s/n)"
    
    if ($push -eq "s" -or $push -eq "S") {
        Write-Host ""
        Write-Host "Pushing develop a origin..." -ForegroundColor Cyan
        git push -u origin develop
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Push exitoso" -ForegroundColor Green
        } else {
            Write-Host "? Error en push" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "? Push omitido" -ForegroundColor Yellow
    }
} else {
    Write-Host "[3/5] Push omitido (--SkipPush)" -ForegroundColor Yellow
}

Write-Host ""

# Mostrar variables
Write-Host "[4/5] Variables de Railway..." -ForegroundColor Yellow
Write-Host ""

if ($ShowVariables) {
    Write-Host "Copiar estas variables en Railway Dashboard:" -ForegroundColor Cyan
    Write-Host "cima-blazor > Variables > Raw Editor" -ForegroundColor Gray
    Write-Host ""
    Write-Host "ASPNETCORE_ENVIRONMENT=Staging" -ForegroundColor White
    Write-Host "ASPNETCORE_URLS=http://+:8080" -ForegroundColor White
    Write-Host "PORT=8080" -ForegroundColor White
    Write-Host "APP_SELF_URL=https://`${{RAILWAY_PUBLIC_DOMAIN}}" -ForegroundColor White
    Write-Host "AuthServer__Authority=https://`${{RAILWAY_PUBLIC_DOMAIN}}" -ForegroundColor White
    Write-Host "AuthServer__RequireHttpsMetadata=true" -ForegroundColor White
    Write-Host "ConnectionStrings__Default=Host=`${{Postgres.PGHOST}};Port=`${{Postgres.PGPORT}};Database=`${{Postgres.PGDATABASE}};Username=`${{Postgres.PGUSER}};Password=`${{Postgres.PGPASSWORD}}" -ForegroundColor White
} else {
    Write-Host "  Ver variables completas:" -ForegroundColor White
    Write-Host "  .\etc\scripts\setup-railway-staging.ps1 -ShowVariables" -ForegroundColor Gray
}

Write-Host ""

# Próximos pasos
Write-Host "[5/5] Próximos pasos en Railway..." -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Ir a Railway Dashboard:" -ForegroundColor Cyan
Write-Host "   https://railway.app/dashboard" -ForegroundColor White
Write-Host ""
Write-Host "2. New Project > Deploy from GitHub repo" -ForegroundColor Cyan
Write-Host "   - Seleccionar: Pedro-Samuel-Rodriguez-Caudillo/cima" -ForegroundColor White
Write-Host "   - IMPORTANTE: Rama develop (no master)" -ForegroundColor Yellow
Write-Host ""
Write-Host "3. Agregar PostgreSQL:" -ForegroundColor Cyan
Write-Host "   + New > Database > PostgreSQL" -ForegroundColor White
Write-Host ""
Write-Host "4. Configurar Variables:" -ForegroundColor Cyan
Write-Host "   cima-blazor > Variables > Raw Editor" -ForegroundColor White
Write-Host "   Pegar variables (ejecuta con -ShowVariables)" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Esperar Deploy:" -ForegroundColor Cyan
Write-Host "   Deployments > Latest (5-8 min primera vez)" -ForegroundColor White
Write-Host ""
Write-Host "6. Verificar Health Check:" -ForegroundColor Cyan
Write-Host "   https://[tu-url]/api/health/ping" -ForegroundColor White
Write-Host ""

# Resumen
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Rama: develop ?" -ForegroundColor Green
Write-Host "Archivos Railway: ?" -ForegroundColor Green

if (-not $SkipPush) {
    Write-Host "Push a GitHub: Verificar arriba" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Documentación:" -ForegroundColor Cyan
Write-Host "  - docs/RAILWAY_STAGING_CONFIG.md" -ForegroundColor White
Write-Host "  - docs/GIT_BRANCHING_STRATEGY.md" -ForegroundColor White
Write-Host ""
Write-Host "URL Staging (después de configurar Railway):" -ForegroundColor Cyan
Write-Host "  https://cima-staging.up.railway.app" -ForegroundColor White
Write-Host ""
