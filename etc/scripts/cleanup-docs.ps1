# Script para limpiar archivos duplicados y obsoletos de docs/
# Este script mueve archivos duplicados a archive/

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  LIMPIEZA DE DOCUMENTACIÓN" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Definir archivos a archivar (ya copiados a nueva estructura)
$filesToArchive = @(
    "00_INICIO_AQUI.md",
    "QUICK_REFERENCE.md",
    "SETUP_SIN_MULTITENANCY.md",
    "NAMESPACE_REORGANIZATION_SUMMARY.md",
    "GUIA_MIGRACIONES_EF.md",
    "CONFIGURACION_TAILWIND.md",
    "SISTEMA_LOGGING.md",
    "GUIA_CLASES_CSS_CIMA.md",
    "DIA_1_*.md",
    "DIA_4_*.md",
    "DIA_5_*.md",
    "DIA_6_*.md",
    "DIA_7_*.md",
    "DIA_8_*.md",
    "CORRECCIONES_*.md",
    "FIX_*.md",
    "SOLUCION_*.md",
    "RAILWAY_*.md",
    "GITHUB_*.md",
    "CI_CD_*.md",
    "GIT_BRANCHING_STRATEGY.md"
)

# Archivos que pueden ser eliminados completamente (temporales)
$filesToDelete = @(
    "PUSH_AHORA.md",
    "PUSH_FINAL_AHORA.md",
    "EJECUTAR_FIX_CICD.md",
    "RAILWAY_CONFIGURAR_AHORA.md",
    "RAILWAY_FIX_EJECUTAR_AHORA.md",
    "RAILWAY_LISTO_AHORA.md",
    "STAGING_QUICK_DECISION.md"
)

Write-Host "Paso 1: Mover archivos a archive/sprint-history/" -ForegroundColor Yellow
Write-Host ""

$archiveDir = "docs\archive\sprint-history"
$movedCount = 0

foreach ($pattern in $filesToArchive) {
    $files = Get-ChildItem -Path "docs" -Filter $pattern -File -ErrorAction SilentlyContinue
    
    foreach ($file in $files) {
        $destPath = Join-Path $archiveDir $file.Name
        
        if (Test-Path $file.FullName) {
            try {
                Copy-Item -Path $file.FullName -Destination $destPath -Force
                Remove-Item -Path $file.FullName -Force
                Write-Host "  ? Movido: $($file.Name)" -ForegroundColor Green
                $movedCount++
            }
            catch {
                Write-Host "  ? Error moviendo: $($file.Name)" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
Write-Host "Archivos movidos a archive: $movedCount" -ForegroundColor Cyan
Write-Host ""

Write-Host "Paso 2: Eliminar archivos temporales" -ForegroundColor Yellow
Write-Host ""

$deletedCount = 0

foreach ($fileName in $filesToDelete) {
    $filePath = "docs\$fileName"
    
    if (Test-Path $filePath) {
        try {
            Remove-Item -Path $filePath -Force
            Write-Host "  ? Eliminado: $fileName" -ForegroundColor Green
            $deletedCount++
        }
        catch {
            Write-Host "  ? Error eliminando: $fileName" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Archivos eliminados: $deletedCount" -ForegroundColor Cyan
Write-Host ""

Write-Host "Paso 3: Verificar estructura final" -ForegroundColor Yellow
Write-Host ""

$expectedDirs = @(
    "docs\01-getting-started",
    "docs\02-architecture",
    "docs\03-development\guides",
    "docs\03-development\sprints",
    "docs\04-deployment",
    "docs\05-troubleshooting",
    "docs\archive"
)

foreach ($dir in $expectedDirs) {
    if (Test-Path $dir) {
        $fileCount = (Get-ChildItem -Path $dir -File -Recurse).Count
        Write-Host "  ? $dir ($fileCount archivos)" -ForegroundColor Green
    }
    else {
        Write-Host "  ? $dir (NO EXISTE)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  LIMPIEZA COMPLETADA" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Resumen:" -ForegroundColor Yellow
Write-Host "  - Archivos movidos a archive: $movedCount" -ForegroundColor White
Write-Host "  - Archivos eliminados: $deletedCount" -ForegroundColor White
Write-Host ""
Write-Host "Siguiente paso:" -ForegroundColor Yellow
Write-Host "  1. Revisar docs\INDEX.md para la nueva estructura" -ForegroundColor White
Write-Host "  2. Revisar .github\BRANCHING_STRATEGY.md para el flujo de trabajo" -ForegroundColor White
Write-Host ""
