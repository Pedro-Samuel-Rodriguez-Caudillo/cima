# Script para crear cliente Blazor WebApp en OpenIddict via PostgreSQL
# Ejecutar: .\etc\scripts\create-blazor-openiddict-client.ps1

Write-Host "?? Creando cliente cima_BlazorWebApp en OpenIddict..." -ForegroundColor Cyan

# Connection string
$connectionString = "Host=localhost;Port=5432;Database=cima;Username=cima_app;Password=cima_dev"

# SQL para insertar el cliente
$sql = @"
INSERT INTO "OpenIddictApplications" (
    "Id",
    "ApplicationType",
    "ClientId",
    "ClientSecret",
    "ClientType",
    "ConsentType",
    "DisplayName",
    "Permissions",
    "RedirectUris",
    "PostLogoutRedirectUris",
    "ClientUri",
    "LogoUri",
    "ExtraProperties",
    "ConcurrencyStamp",
    "CreationTime",
    "IsDeleted"
)
SELECT
    gen_random_uuid(),
    'web',
    'cima_BlazorWebApp',
    'e4bcd65d-41a1-4e47-a5ea-ee1c3e5e5a97',
    'confidential',
    'implicit',
    'Blazor WebApp Application',
    '["ept:token","ept:revocation","ept:introspection","ept:authorization","ept:logout","gt:authorization_code","gt:refresh_token","rst:code","rst:code_id_token","scp:address","scp:email","scp:phone","scp:profile","scp:roles","scp:cima"]',
    '[\"https://localhost:44350/signin-oidc\"]',
    '[\"https://localhost:44350/signout-callback-oidc\"]',
    'https://localhost:44350',
    '/images/clients/blazor.svg',
    '{}',
    gen_random_uuid()::text,
    NOW(),
    FALSE
WHERE NOT EXISTS (
    SELECT 1 FROM "OpenIddictApplications" WHERE "ClientId" = 'cima_BlazorWebApp'
);
"@

try {
    # Cargar Npgsql assembly
    Add-Type -Path "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.1\System.Data.Common.dll" -ErrorAction SilentlyContinue
    
    # Usar dotnet para ejecutar el SQL
    Write-Host "?? Ejecutando SQL..." -ForegroundColor Yellow
    
    $sqlFile = "etc\scripts\temp-insert-client.sql"
    $sql | Out-File -FilePath $sqlFile -Encoding UTF8
    
    # Ejecutar con dotnet ef
    Write-Host "? Conectando a base de datos..." -ForegroundColor Yellow
    $env:PGPASSWORD = "cima_dev"
    
    # Intentar con psql si está disponible
    if (Get-Command psql -ErrorAction SilentlyContinue) {
        psql -h localhost -p 5432 -U cima_app -d cima -f $sqlFile
    } else {
        Write-Host "?? psql no encontrado. Usa pgAdmin o ejecuta manualmente el SQL:" -ForegroundColor Yellow
        Write-Host $sql -ForegroundColor Gray
        return
    }
    
    Remove-Item $sqlFile -ErrorAction SilentlyContinue
    
    Write-Host "? Cliente cima_BlazorWebApp creado exitosamente!" -ForegroundColor Green
    
} catch {
    Write-Host "? Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? SQL a ejecutar manualmente en pgAdmin:" -ForegroundColor Yellow
    Write-Host $sql -ForegroundColor Gray
}
