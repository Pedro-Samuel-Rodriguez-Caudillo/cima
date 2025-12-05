# backup-database.ps1
# Script para realizar backup automático de la base de datos PostgreSQL
# Uso: .\backup-database.ps1 [-Environment Development|Staging|Production]

param(
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment = "Development",
    
    [string]$BackupPath = ".\backups",
    
    [int]$RetentionDays = 30,
    
    [switch]$Compress,
    
    [switch]$Upload  # Para subir a almacenamiento cloud (requiere configuración adicional)
)

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CIMA Database Backup" -ForegroundColor Cyan
Write-Host "  Environment: $Environment" -ForegroundColor Cyan
Write-Host "  Timestamp: $timestamp" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Crear directorio de backups si no existe
if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    Write-Host "Created backup directory: $BackupPath" -ForegroundColor Green
}

# Obtener configuración de conexión según ambiente
function Get-ConnectionString {
    param([string]$Env)
    
    switch ($Env) {
        "Development" {
            $configPath = "src\cima.Blazor\appsettings.Development.json"
        }
        "Staging" {
            $configPath = "src\cima.Blazor\appsettings.Staging.json"
        }
        "Production" {
            $configPath = "src\cima.Blazor\appsettings.json"
        }
    }
    
    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json
        return $config.ConnectionStrings.Default
    }
    
    # Intentar desde variables de entorno
    $connStr = $env:ConnectionStrings__Default
    if ($connStr) {
        return $connStr
    }
    
    throw "No se encontró la cadena de conexión para el ambiente $Env"
}

function Parse-ConnectionString {
    param([string]$ConnectionString)
    
    $params = @{}
    $ConnectionString -split ';' | ForEach-Object {
        if ($_ -match '(.+)=(.+)') {
            $params[$matches[1].Trim()] = $matches[2].Trim()
        }
    }
    return $params
}

try {
    # Obtener conexión
    $connectionString = Get-ConnectionString -Env $Environment
    $connParams = Parse-ConnectionString -ConnectionString $connectionString
    
    $host = $connParams['Host'] ?? $connParams['Server'] ?? 'localhost'
    $port = $connParams['Port'] ?? '5432'
    $database = $connParams['Database']
    $username = $connParams['Username'] ?? $connParams['User Id']
    $password = $connParams['Password']
    
    if (-not $database) {
        throw "No se pudo determinar el nombre de la base de datos"
    }
    
    Write-Host "`nDatabase: $database" -ForegroundColor Yellow
    Write-Host "Host: $host`:$port" -ForegroundColor Yellow
    Write-Host "User: $username" -ForegroundColor Yellow
    
    # Nombre del archivo de backup
    $backupFileName = "cima_${Environment}_${timestamp}"
    $backupFile = Join-Path $BackupPath "$backupFileName.sql"
    
    # Configurar variable de entorno para contraseña
    $env:PGPASSWORD = $password
    
    Write-Host "`nCreating backup..." -ForegroundColor Yellow
    
    # Ejecutar pg_dump
    $pgDumpArgs = @(
        "-h", $host,
        "-p", $port,
        "-U", $username,
        "-d", $database,
        "-F", "p",  # Plain text format
        "-f", $backupFile,
        "--no-owner",
        "--no-acl",
        "--verbose"
    )
    
    $result = & pg_dump @pgDumpArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump failed: $result"
    }
    
    $backupSize = (Get-Item $backupFile).Length
    $backupSizeMB = [math]::Round($backupSize / 1MB, 2)
    
    Write-Host "Backup created: $backupFile ($backupSizeMB MB)" -ForegroundColor Green
    
    # Comprimir si se solicita
    if ($Compress) {
        Write-Host "Compressing backup..." -ForegroundColor Yellow
        
        $compressedFile = "$backupFile.gz"
        
        # Usar gzip si está disponible, si no, usar Compress-Archive
        if (Get-Command gzip -ErrorAction SilentlyContinue) {
            & gzip -9 $backupFile
            $compressedFile = "$backupFile.gz"
        } else {
            $zipFile = Join-Path $BackupPath "$backupFileName.zip"
            Compress-Archive -Path $backupFile -DestinationPath $zipFile -CompressionLevel Optimal
            Remove-Item $backupFile -Force
            $compressedFile = $zipFile
        }
        
        $compressedSize = (Get-Item $compressedFile).Length
        $compressedSizeMB = [math]::Round($compressedSize / 1MB, 2)
        $compressionRatio = [math]::Round(($compressedSize / $backupSize) * 100, 1)
        
        Write-Host "Compressed to: $compressedFile ($compressedSizeMB MB, $compressionRatio%)" -ForegroundColor Green
    }
    
    # Limpiar backups antiguos
    Write-Host "`nCleaning old backups (retention: $RetentionDays days)..." -ForegroundColor Yellow
    
    $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
    $oldBackups = Get-ChildItem $BackupPath -File | Where-Object { 
        $_.LastWriteTime -lt $cutoffDate -and 
        ($_.Name -like "cima_*.sql*" -or $_.Name -like "cima_*.zip")
    }
    
    if ($oldBackups) {
        $oldBackups | ForEach-Object {
            Remove-Item $_.FullName -Force
            Write-Host "  Deleted: $($_.Name)" -ForegroundColor Gray
        }
        Write-Host "Deleted $($oldBackups.Count) old backup(s)" -ForegroundColor Yellow
    } else {
        Write-Host "No old backups to delete" -ForegroundColor Gray
    }
    
    # Generar resumen
    $backups = Get-ChildItem $BackupPath -File | Where-Object { 
        $_.Name -like "cima_*.sql*" -or $_.Name -like "cima_*.zip"
    } | Sort-Object LastWriteTime -Descending
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  Backup Summary" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Total backups: $($backups.Count)" -ForegroundColor White
    
    $totalSize = ($backups | Measure-Object -Property Length -Sum).Sum
    $totalSizeMB = [math]::Round($totalSize / 1MB, 2)
    Write-Host "Total size: $totalSizeMB MB" -ForegroundColor White
    
    Write-Host "`nLatest backups:" -ForegroundColor Yellow
    $backups | Select-Object -First 5 | ForEach-Object {
        $sizeMB = [math]::Round($_.Length / 1MB, 2)
        Write-Host "  $($_.Name) - $sizeMB MB - $($_.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))" -ForegroundColor Gray
    }
    
    Write-Host "`nBackup completed successfully!" -ForegroundColor Green
    
    # Retornar información del backup para uso en scripts
    return @{
        Success = $true
        BackupFile = if ($Compress) { $compressedFile } else { $backupFile }
        Database = $database
        Environment = $Environment
        Timestamp = $timestamp
        SizeMB = if ($Compress) { $compressedSizeMB } else { $backupSizeMB }
    }
}
catch {
    Write-Host "`nBackup FAILED!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    return @{
        Success = $false
        Error = $_.Exception.Message
    }
}
finally {
    # Limpiar variable de entorno
    $env:PGPASSWORD = $null
}
