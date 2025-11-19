# ver-logs.ps1 - Visualizar logs del sistema CIMA
param(
    [ValidateSet("blazor", "migrator", "all")]
    [string]$Proyecto = "all",
    [int]$Lineas = 50
)

Write-Host "===== LOGS SISTEMA CIMA =====" -ForegroundColor Cyan
Write-Host ""

$rootPath = Get-Location

function Show-LogFile {
    param(
        [string]$LogPath,
        [string]$Nombre,
        [int]$Lines
    )
    
    if (Test-Path $LogPath) {
        $fileInfo = Get-Item $LogPath
        $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
        
        Write-Host "===== $Nombre =====" -ForegroundColor Yellow
        Write-Host "Ubicacion: $LogPath" -ForegroundColor Gray
        Write-Host "Tamaño: $sizeKB KB" -ForegroundColor Gray
        Write-Host "Ultima modificacion: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Ultimas $Lines lineas:" -ForegroundColor Cyan
        Write-Host ""
        
        Get-Content $LogPath -Tail $Lines | ForEach-Object {
            if ($_ -match "ERR") {
                Write-Host $_ -ForegroundColor Red
            } elseif ($_ -match "WRN") {
                Write-Host $_ -ForegroundColor Yellow
            } elseif ($_ -match "INF") {
                Write-Host $_ -ForegroundColor Green
            } elseif ($_ -match "DBG") {
                Write-Host $_ -ForegroundColor Gray
            } else {
                Write-Host $_
            }
        }
        Write-Host ""
    } else {
        Write-Host "WARNING: Log no encontrado en $LogPath" -ForegroundColor Yellow
        Write-Host ""
    }
}

# Blazor Logs
if ($Proyecto -eq "blazor" -or $Proyecto -eq "all") {
    $blazorLog = Join-Path $rootPath "src\cima.Blazor\Logs\logs.txt"
    Show-LogFile -LogPath $blazorLog -Nombre "BLAZOR WEB APP" -Lines $Lineas
}

# DbMigrator Logs
if ($Proyecto -eq "migrator" -or $Proyecto -eq "all") {
    $migratorLog = Join-Path $rootPath "src\cima.DbMigrator\Logs\logs.txt"
    Show-LogFile -LogPath $migratorLog -Nombre "DB MIGRATOR" -Lines $Lineas
}

# Resumen
Write-Host "===== COMANDOS UTILES =====" -ForegroundColor Cyan
Write-Host "Ver solo Blazor:    .\etc\scripts\ver-logs.ps1 -Proyecto blazor" -ForegroundColor White
Write-Host "Ver solo Migrator:  .\etc\scripts\ver-logs.ps1 -Proyecto migrator" -ForegroundColor White
Write-Host "Ver mas lineas:     .\etc\scripts\ver-logs.ps1 -Lineas 100" -ForegroundColor White
Write-Host "Abrir en Notepad:   notepad src\cima.Blazor\Logs\logs.txt" -ForegroundColor White
Write-Host "Limpiar logs:       .\etc\scripts\limpiar-logs.ps1" -ForegroundColor White
