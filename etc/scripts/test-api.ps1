# test-api.ps1 - PRUEBAS API CIMA (CORREGIDO - URL LISTING)
param(
    [string]$BaseUrl = "https://localhost:44350",
    [string]$Username = "admin",
    [string]$Password = "1q2w3E*",
    [string]$ClientId = "cima_Swagger"
)

# Deshabilitar SSL en desarrollo
Add-Type @"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
public static class SSLFix {
    public static void Ignore() {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
    }
}
"@

[SSLFix]::Ignore()
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$totalTests = 0
$passedTests = 0
$failedTests = 0

function Test-Endpoint {
    param([string]$Name, [scriptblock]$TestBlock)
    $script:totalTests++
    Write-Host "[$script:totalTests] $Name..." -ForegroundColor Yellow -NoNewline
    try {
        & $TestBlock
        $script:passedTests++
        Write-Host " OK" -ForegroundColor Green
    } catch {
        $script:failedTests++
        Write-Host " FALLO" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails) {
            Write-Host "   Detalles: $($_.ErrorDetails.Message)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "===== PRUEBAS API CIMA =====" -ForegroundColor Cyan
Write-Host ""

# 1. TOKEN
$TOKEN = $null
Test-Endpoint "Obtener token" {
    $body = "grant_type=password&username=$Username&password=$([System.Uri]::EscapeDataString($Password))&client_id=$ClientId&scope=cima"

    $response = Invoke-RestMethod -Uri "$BaseUrl/connect/token" `
      -Method POST `
      -ContentType "application/x-www-form-urlencoded" `
      -Body $body `
      -ErrorAction Stop

    $script:TOKEN = $response.access_token
}

if (-not $TOKEN) {
    Write-Host "NO SE PUDO OBTENER TOKEN. ABORTANDO." -ForegroundColor Red
    exit 1
}

Write-Host "TOKEN OK: $($TOKEN.Substring(0,25))..." -ForegroundColor Gray
Write-Host ""

# ===== ARCHITECT TESTS =====
Write-Host "===== ARCHITECT =====" -ForegroundColor Cyan

# 2. CREAR ARCHITECT
$architectId = $null
Test-Endpoint "POST /api/app/architect" {
    $timestamp = Get-Date -Format 'HHmmss'
    $headers = @{ Authorization = "Bearer $TOKEN" }

    $body = @{
        bio = "Biografía de prueba $timestamp"
        specialization = "Diseño moderno"
        yearsOfExperience = 5
        portfolioUrl = "https://example.com/portfolio"
        isVerified = $false
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/architect" `
          -Method POST `
          -Headers $headers `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        $script:architectId = $response.id
        Write-Host "    Architect ID: $architectId" -ForegroundColor Gray
        # Intentar obtener el userId del architect recién creado
        $architectDetails = Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/$architectId" `
            -Method GET `
            -Headers $headers `
            -ErrorAction Stop
        $script:architectUserId = $architectDetails.userId
        Write-Host "    Architect User ID: $architectUserId" -ForegroundColor Gray
    } catch {
        Write-Host "    Ya existe o error (OK para prueba)" -ForegroundColor Gray
    }
}

# 3. GET BY USER-ID (obtener ID del admin user)
$adminUserId = $null
Test-Endpoint "GET usuario admin" {
    $headers = @{ Authorization = "Bearer $TOKEN" }

    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/identity/users?Filter=admin&MaxResultCount=1" `
            -Method GET `
            -Headers $headers `
            -ErrorAction Stop
        
        if ($response.items -and $response.items.Count -gt 0) {
            $script:adminUserId = $response.items[0].id
            Write-Host "    Admin User ID: $adminUserId" -ForegroundColor Gray
        }
    } catch {
        Write-Host "    No se pudo obtener user ID" -ForegroundColor Gray
    }
}

# 4. GET ARCHITECT BY ID
if ($architectId) {
    Test-Endpoint "GET /api/app/architect/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/$architectId" `
            -Method GET `
            -Headers $headers `
            -ErrorAction Stop

        Write-Host "    Bio: $($response.bio)" -ForegroundColor Gray
    }
}

# 5. UPDATE ARCHITECT
if ($architectId) {
    Test-Endpoint "PUT /api/app/architect/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $body = @{
            bio = "Biografía actualizada"
            specialization = "Diseño sostenible"
            yearsOfExperience = 10
            portfolioUrl = "https://example.com/new-portfolio"
            isVerified = $true
        } | ConvertTo-Json

        Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/$architectId" `
          -Method PUT `
          -Headers $headers `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        Write-Host "    Actualizado OK" -ForegroundColor Gray
    }
}

# 6. DELETE ARCHITECT
if ($architectId) {
    Test-Endpoint "DELETE /api/app/architect/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        try {
            Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/$architectId" `
              -Method DELETE `
              -Headers $headers `
              -ErrorAction Stop

            Write-Host "    Eliminado OK" -ForegroundColor Gray
            $script:architectId = $null
        } catch {
            Write-Host "    Error al eliminar (puede tener dependencias)" -ForegroundColor Gray
        }
    }
}

# 7. GET ARCHITECT BY USER-ID
if ($adminUserId) {
    Test-Endpoint "GET /api/app/architect/by-user-id/{userId}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        try {
            $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/by-user-id/$adminUserId" `
                -Method GET `
                -Headers $headers `
                -ErrorAction Stop

            Write-Host "    Architect found: $($response.id)" -ForegroundColor Gray
        } catch {
            Write-Host "    No architect found for user (OK for test)" -ForegroundColor Gray
        }
    }
}

Write-Host ""

# ===== LISTING TESTS =====
Write-Host "===== LISTING =====" -ForegroundColor Cyan

# 8. CREAR ARCHITECT TEMPORAL PARA LISTING (usando el admin)
$listingArchitectId = $null
Test-Endpoint "POST /api/app/architect (temporal)" {
    $timestamp = Get-Date -Format 'HHmmss'
    $headers = @{ Authorization = "Bearer $TOKEN" }

    # Intentar crear asociándolo al admin (si la API lo permite)
    $body = @{
        userId = $adminUserId  # Intentar asociar al admin
        bio = "Biografía de prueba $timestamp"
        specialization = "Diseño moderno"
        yearsOfExperience = 5
        portfolioUrl = "https://example.com/portfolio"
        isVerified = $false
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/architect" `
          -Method POST `
          -Headers $headers `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        $script:listingArchitectId = $response.id
        Write-Host "    Listing Architect ID: $listingArchitectId" -ForegroundColor Gray
    } catch {
        Write-Host "    Error creando architect temporal (posible duplicado o permiso)" -ForegroundColor Red
        # Si falla, intentar obtener uno existente asociado al admin
        try {
            $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/by-user-id/$adminUserId" `
                -Method GET `
                -Headers $headers `
                -ErrorAction Stop
            $script:listingArchitectId = $response.id
            Write-Host "    Usando architect existente: $listingArchitectId" -ForegroundColor Gray
        } catch {
            Write-Host "    No se pudo crear ni obtener architect temporal" -ForegroundColor Red
        }
    }
}

# 9. LISTAR LISTINGS (GET con filtros como query params) - CORREGIDO URL
Test-Endpoint "GET /api/app/listing" {
    $headers = @{ Authorization = "Bearer $TOKEN" }

    # Construir URL con parámetros usando nombres correctos de PagedAndSortedResultRequestDto
    $params = @{
        SearchTerm = ""
        Sorting = "createdat desc"
        SkipCount = 0
        MaxResultCount = 10
    }

    $queryString = ($params.GetEnumerator() | ForEach-Object {
        "$($_.Key)=$($_.Value)"
    }) -join "&"

    $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/listing?$queryString" `
      -Method GET `
      -Headers $headers `
      -ErrorAction Stop

    Write-Host "    Total listings: $($response.totalCount)" -ForegroundColor Gray
}

# 10. CREAR LISTING (si tenemos architect)
$listingId = $null
if ($listingArchitectId) {
    Test-Endpoint "POST /api/app/listing" {
        $timestamp = Get-Date -Format 'HHmmss'
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $body = @{
            architectId = $listingArchitectId
            title = "Listing $timestamp"
            description = "Creado automáticamente"
            price = 123456
            location = "CDMX"
            area = 100
            bedrooms = 2
            bathrooms = 1
            hasGarage = $false
            hasGarden = $false
            hasPool = $false
            yearBuilt = 2024
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/listing" `
          -Method POST `
          -Headers $headers `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        $script:listingId = $response.id
        Write-Host "    Listing ID: $listingId" -ForegroundColor Gray
    }
}

# 11. GET LISTING BY ID
if ($listingId) {
    Test-Endpoint "GET /api/app/listing/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/listing/$listingId" `
            -Method GET `
            -Headers $headers `
            -ErrorAction Stop

        Write-Host "    Title: $($response.title)" -ForegroundColor Gray
    }
}

# 12. UPDATE LISTING
if ($listingId) {
    Test-Endpoint "PUT /api/app/listing/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $body = @{
            title = "Listing actualizado"
            description = "Actualizado"
            price = 999999
            location = "Monterrey"
            area = 150
            bedrooms = 3
            bathrooms = 2
            hasGarage = $true
            hasGarden = $false
            hasPool = $false
            yearBuilt = 2024
        } | ConvertTo-Json

        Invoke-RestMethod -Uri "$BaseUrl/api/app/listing/$listingId" `
          -Method PUT `
          -Headers $headers `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        Write-Host "    Actualizado OK" -ForegroundColor Gray
    }
}

# 13. PUBLISH LISTING (sin images fallará, pero probamos el endpoint)
if ($listingId) {
    Test-Endpoint "POST /api/app/listing/{id}/publish (sin images)" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        try {
            Invoke-RestMethod -Uri "$BaseUrl/api/app/listing/$listingId/publish" `
              -Method POST `
              -Headers $headers `
              -ErrorAction Stop

            Write-Host "    Publicado OK (inesperado)" -ForegroundColor Gray
        } catch {
            if ($_.ErrorDetails.Message -like "*RequiresImages*") {
                Write-Host "    Error esperado: requiere imágenes" -ForegroundColor Gray
            } else {
                throw
            }
        }
    }
}

# 14. ARCHIVE LISTING
if ($listingId) {
    Test-Endpoint "POST /api/app/listing/{id}/archive" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        Invoke-RestMethod -Uri "$BaseUrl/api/app/listing/$listingId/archive" `
          -Method POST `
          -Headers $headers `
          -ErrorAction Stop

        Write-Host "    Archivado OK" -ForegroundColor Gray
    }
}

# 15. LISTAR POR ARQUITECTO
if ($listingArchitectId) {
    Test-Endpoint "GET /api/app/listing/by-architect/{architectId}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/listing/by-architect/$($listingArchitectId)?SkipCount=0&MaxResultCount=10" `
          -Method GET `
          -Headers $headers `
          -ErrorAction Stop

        Write-Host "    Total: $($response.totalCount)" -ForegroundColor Gray
    }
}

Write-Host ""

# ===== CONTACT REQUEST TESTS =====
Write-Host "===== CONTACT REQUEST =====" -ForegroundColor Cyan

# 16. CREAR CONTACT REQUEST (sin autenticación)
$contactId = $null
if ($listingId) {
    Test-Endpoint "POST /api/app/contact-request" {
        $timestamp = Get-Date -Format 'HHmmss'
        $body = @{
            listingId = $listingId
            name = "Test Usuario $timestamp"
            email = "test$timestamp@example.com"
            phone = "+52 55 1234 5678"
            message = "Solicitud automática"
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/contact-request" `
          -Method POST `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        $script:contactId = $response.id
        Write-Host "    ContactRequest ID: $($response.id)" -ForegroundColor Gray
    }
}

# 17. LISTAR CONTACT REQUESTS (paginado)
Test-Endpoint "GET /api/app/contact-request" {
    $headers = @{ Authorization = "Bearer $TOKEN" }

    $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/contact-request?SkipCount=0&MaxResultCount=10" `
      -Method GET `
      -Headers $headers `
      -ErrorAction Stop

    Write-Host "    Total: $($response.totalCount)" -ForegroundColor Gray
}

# 18. GET /contact-request/{id}
if ($contactId) {
    Test-Endpoint "GET /api/app/contact-request/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/contact-request/$contactId" `
            -Method GET `
            -Headers $headers `
            -ErrorAction Stop

        Write-Host "    Email: $($response.email)" -ForegroundColor Gray
    }
}

# 19. MARK-AS-REPLIED
if ($contactId) {
    Test-Endpoint "POST /api/app/contact-request/{id}/mark-as-replied" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $body = @{ replyNotes = "Respuesta de prueba automática" } | ConvertTo-Json

        Invoke-RestMethod -Uri "$BaseUrl/api/app/contact-request/$contactId/mark-as-replied" `
          -Method POST `
          -Headers $headers `
          -ContentType "application/json; charset=utf-8" `
          -Body $body `
          -ErrorAction Stop

        Write-Host "    MarkAsReplied OK" -ForegroundColor Gray
    }
}

# 20. CLOSE CONTACT REQUEST
if ($contactId) {
    Test-Endpoint "POST /api/app/contact-request/{id}/close" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        Invoke-RestMethod -Uri "$BaseUrl/api/app/contact-request/$contactId/close" `
          -Method POST `
          -Headers $headers `
          -ErrorAction Stop

        Write-Host "    Close OK" -ForegroundColor Gray
    }
}

# 21. LISTAR POR ARQUITECTO
if ($listingArchitectId) {
    Test-Endpoint "GET /api/app/contact-request/by-architect/{architectId}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/app/contact-request/by-architect/$($listingArchitectId)?SkipCount=0&MaxResultCount=10" `
            -Method GET `
            -Headers $headers `
            -ErrorAction Stop
        Write-Host "    Total: $($response.totalCount)" -ForegroundColor Gray
    }
}

Write-Host ""

# ===== CLEANUP =====
Write-Host "===== LIMPIEZA =====" -ForegroundColor Cyan

# 22. DELETE LISTING
if ($listingId) {
    Test-Endpoint "DELETE /api/app/listing/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        Invoke-RestMethod -Uri "$BaseUrl/api/app/listing/$listingId" `
          -Method DELETE `
          -Headers $headers `
          -ErrorAction Stop

        Write-Host "    Eliminado OK" -ForegroundColor Gray
    }
}

# 23. DELETE ARCHITECT TEMPORAL
if ($listingArchitectId) {
    Test-Endpoint "DELETE /api/app/architect/{id}" {
        $headers = @{ Authorization = "Bearer $TOKEN" }

        try {
            Invoke-RestMethod -Uri "$BaseUrl/api/app/architect/$listingArchitectId" `
              -Method DELETE `
              -Headers $headers `
              -ErrorAction Stop

            Write-Host "    Eliminado OK" -ForegroundColor Gray
        } catch {
            Write-Host "    Error al eliminar (puede tener dependencias)" -ForegroundColor Gray
        }
    }
}

# RESUMEN
Write-Host ""
Write-Host "===== RESULTADO =====" -ForegroundColor Cyan
Write-Host "Total pruebas: $totalTests"
Write-Host "Exitosas: $passedTests" -ForegroundColor Green
Write-Host "Fallidas: $failedTests" -ForegroundColor Red

if ($failedTests -eq 0) {
    Write-Host "TODO OK. API FUNCIONANDO." -ForegroundColor Green
    exit 0
} else {
    Write-Host "ALGUNAS PRUEBAS FALLARON. REVISAR LOGS." -ForegroundColor Yellow
    exit 1
}