# restore-database.ps1
# Script para restaurar backup de la base de datos PostgreSQL
# Uso: .\restore-database.ps1 -BackupFile ".\backups\cima_Development_2024-01-15_10-30-00.sql"

param(
    [Parameter(Mandatory=$true)]
    [string]$BackupFile,
    
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment = "Development",
    
    [switch]$Force,  # Omitir confirmación
    
    [switch]$DropExisting  # Eliminar base de datos existente
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CIMA Database Restore" -ForegroundColor Cyan
Write-Host "  Environment: $Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verificar que el archivo existe
if (-not (Test-Path $BackupFile)) {
    throw "Backup file not found: $BackupFile"
}

# Descomprimir si es necesario
$restoreFile = $BackupFile
if ($BackupFile.EndsWith(".gz")) {
    Write-Host "Decompressing backup..." -ForegroundColor Yellow
    $restoreFile = $BackupFile -replace '\.gz$', ''
    & gzip -d -k $BackupFile
} elseif ($BackupFile.EndsWith(".zip")) {
    Write-Host "Extracting backup..." -ForegroundColor Yellow
    $extractPath = Split-Path $BackupFile
    Expand-Archive -Path $BackupFile -DestinationPath $extractPath -Force
    $restoreFile = Join-Path $extractPath ((Get-Item $BackupFile).BaseName + ".sql")
}

Write-Host "Restore file: $restoreFile" -ForegroundColor Yellow

# Obtener configuración de conexión
function Get-ConnectionString {
    param([string]$Env)
    
    switch ($Env) {
        "Development" { $configPath = "src\cima.Blazor\appsettings.Development.json" }
        "Staging" { $configPath = "src\cima.Blazor\appsettings.Staging.json" }
        "Production" { $configPath = "src\cima.Blazor\appsettings.json" }
    }
    
    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json
        return $config.ConnectionStrings.Default
    }
    
    $connStr = $env:ConnectionStrings__Default
    if ($connStr) { return $connStr }
    
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
    $connectionString = Get-ConnectionString -Env $Environment
    $connParams = Parse-ConnectionString -ConnectionString $connectionString
    
    $host = $connParams['Host'] ?? $connParams['Server'] ?? 'localhost'
    $port = $connParams['Port'] ?? '5432'
    $database = $connParams['Database']
    $username = $connParams['Username'] ?? $connParams['User Id']
    $password = $connParams['Password']
    
    Write-Host "`nTarget Database: $database" -ForegroundColor Yellow
    Write-Host "Host: $host`:$port" -ForegroundColor Yellow
    
    # Confirmación
    if (-not $Force) {
        Write-Host "`n??  WARNING: This will overwrite the database '$database'" -ForegroundColor Red
        $confirm = Read-Host "Type 'YES' to continue"
        if ($confirm -ne "YES") {
            Write-Host "Restore cancelled" -ForegroundColor Yellow
            exit 0
        }
    }
    
    $env:PGPASSWORD = $password
    
    if ($DropExisting) {
        Write-Host "`nDropping existing database..." -ForegroundColor Yellow
        & psql -h $host -p $port -U $username -d postgres -c "DROP DATABASE IF EXISTS $database WITH (FORCE);"
        & psql -h $host -p $port -U $username -d postgres -c "CREATE DATABASE $database;"
    }
    
    Write-Host "`nRestoring database..." -ForegroundColor Yellow
    
    $result = & psql -h $host -p $port -U $username -d $database -f $restoreFile 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed: $result"
    }
    
    Write-Host "`nDatabase restored successfully!" -ForegroundColor Green
    
    # Limpiar archivo temporal descomprimido
    if ($BackupFile -ne $restoreFile -and (Test-Path $restoreFile)) {
        Remove-Item $restoreFile -Force
    }
}
catch {
    Write-Host "`nRestore FAILED!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
finally {
    $env:PGPASSWORD = $null
}
