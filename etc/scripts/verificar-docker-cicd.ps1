# Script de Verificacion - Docker y CI/CD
# Ejecutar antes de hacer push a GitHub

Write-Host "=== VERIFICACION DE CONFIGURACION DOCKER Y CI/CD ===" -ForegroundColor Cyan
Write-Host ""

# Verificar archivos criticos
$files = @(
    "Dockerfile",
    ".dockerignore",
    "docker-compose.dev.yml",
    "docker-compose.staging.yml",
    "docker-compose.prod.yml",
    ".env.development.example",
    ".env.staging.example",
    ".env.production.example",
    ".github/workflows/ci-build-test.yml",
    ".github/workflows/cd-deploy-staging.yml",
    ".github/workflows/cd-deploy-production.yml"
)

Write-Host "[1/5] Verificando archivos necesarios..." -ForegroundColor Yellow
$missing = @()
foreach ($file in $files) {
    $fullPath = if ($file -eq "Dockerfile") { "src/cima.Blazor/Dockerfile" } else { $file }
    if (Test-Path $fullPath) {
        Write-Host "  OK: $file" -ForegroundColor Green
    } else {
        Write-Host "  FALTA: $file" -ForegroundColor Red
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    Write-Host "`nERROR: Faltan $($missing.Count) archivos" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/5] Verificando que secrets NO estan en Git..." -ForegroundColor Yellow
$secrets = @(".env.development", ".env.staging", ".env.production", "appsettings.secrets.json")
$leaked = @()
foreach ($secret in $secrets) {
    if (Test-Path $secret) {
        $gitStatus = git ls-files $secret 2>$null
        if ($gitStatus) {
            Write-Host "  PELIGRO: $secret esta en Git!" -ForegroundColor Red
            $leaked += $secret
        } else {
            Write-Host "  OK: $secret NO esta en Git" -ForegroundColor Green
        }
    }
}

if ($leaked.Count -gt 0) {
    Write-Host "`nERROR: Archivos secretos encontrados en Git" -ForegroundColor Red
    Write-Host "Ejecutar: git rm --cached $($leaked -join ' ')" -ForegroundColor Yellow
    exit 1
}

Write-Host "`n[3/5] Validando sintaxis YAML..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose.dev.yml config > $null 2>&1
    Write-Host "  OK: docker-compose.dev.yml" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: docker-compose.dev.yml invalido" -ForegroundColor Red
    exit 1
}

try {
    docker-compose -f docker-compose.staging.yml config > $null 2>&1
    Write-Host "  OK: docker-compose.staging.yml" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: docker-compose.staging.yml invalido" -ForegroundColor Red
    exit 1
}

try {
    docker-compose -f docker-compose.prod.yml config > $null 2>&1
    Write-Host "  OK: docker-compose.prod.yml" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: docker-compose.prod.yml invalido" -ForegroundColor Red
    exit 1
}

Write-Host "`n[4/5] Verificando Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "  OK: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: Docker no instalado" -ForegroundColor Red
    exit 1
}

Write-Host "`n[5/5] Probando build de imagen..." -ForegroundColor Yellow
Write-Host "  Construyendo imagen de prueba (puede tomar varios minutos)..." -ForegroundColor Gray

$buildResult = docker build -f src/cima.Blazor/Dockerfile -t cima:test . 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "  OK: Imagen construida exitosamente" -ForegroundColor Green
    
    # Limpiar imagen de prueba
    docker rmi cima:test > $null 2>&1
} else {
    Write-Host "  ERROR: Fallo al construir imagen" -ForegroundColor Red
    Write-Host "  Salida del error:" -ForegroundColor Yellow
    Write-Host $buildResult -ForegroundColor Gray
    exit 1
}

Write-Host "`n=== VERIFICACION COMPLETADA EXITOSAMENTE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Proximos pasos:" -ForegroundColor Cyan
Write-Host "1. Configurar secrets en GitHub (ver docs/GITHUB_SECRETS_SETUP.md)" -ForegroundColor White
Write-Host "2. Preparar servidores de staging y production" -ForegroundColor White
Write-Host "3. Hacer push a 'develop' para deployment automatico a staging" -ForegroundColor White
Write-Host "4. Crear tag 'v1.0.0' para deployment a production" -ForegroundColor White
Write-Host ""
