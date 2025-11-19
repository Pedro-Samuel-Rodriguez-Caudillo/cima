# limpiar-logs.ps1 - Limpiar archivos de log
Write-Host "===== LIMPIEZA DE LOGS =====" -ForegroundColor Cyan
Write-Host ""

$rootPath = Get-Location
$logFiles = @(
    "src\cima.Blazor\Logs\logs.txt",
    "src\cima.DbMigrator\Logs\logs.txt"
)

$totalSize = 0
$cleanedFiles = 0

foreach ($logFile in $logFiles) {
    $fullPath = Join-Path $rootPath $logFile
    
    if (Test-Path $fullPath) {
        $fileInfo = Get-Item $fullPath
        $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
        $totalSize += $sizeKB
        
        Write-Host "Limpiando: $logFile ($sizeKB KB)..." -ForegroundColor Yellow -NoNewline
        
        # Crear backup si el archivo es grande
        if ($sizeKB -gt 100) {
            $backupPath = "$fullPath.backup"
            Copy-Item $fullPath $backupPath -Force
            Write-Host " [Backup creado]" -ForegroundColor Gray -NoNewline
        }
        
        # Limpiar archivo
        Clear-Content $fullPath -Force
        $cleanedFiles++
        
        Write-Host " OK" -ForegroundColor Green
    } else {
        Write-Host "No encontrado: $logFile" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "===== RESUMEN =====" -ForegroundColor Cyan
Write-Host "Archivos limpiados: $cleanedFiles" -ForegroundColor Green
Write-Host "Espacio liberado: $totalSize KB" -ForegroundColor Green
Write-Host ""
Write-Host "Los logs se regeneraran automaticamente al ejecutar las aplicaciones" -ForegroundColor Yellow
