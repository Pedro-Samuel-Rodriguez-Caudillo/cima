# Comandos de Prueba - Endpoints Reorganizados

## ?? Antes de Comenzar

1. **Detener el proceso Blazor en ejecución**
2. **Compilar la solución:**
   ```powershell
   dotnet clean
   dotnet build
   ```
3. **Ejecutar la aplicación:**
   ```powershell
   cd src\cima.Blazor
   dotnet run
   ```

## ?? Obtener Token de Autenticación

```powershell
$loginResponse = Invoke-RestMethod -Uri "https://localhost:44345/api/account/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"userNameOrEmailAddress":"admin","password":"1q2w3E*"}' `
    -SessionVariable session

$token = $loginResponse.access_token
$headers = @{ "Authorization" = "Bearer $token" }
```

## ?? Pruebas de Endpoints - Properties

### 1. GET /api/app/property (Lista con Filtros)

```powershell
# Sin filtros
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property" `
    -Headers $headers

# Con filtros de precio
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property?MinPrice=100000&MaxPrice=500000" `
    -Headers $headers

# Con búsqueda de texto
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property?SearchTerm=casa" `
    -Headers $headers

# Con filtros combinados
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property?Status=1&MinBedrooms=2&MinPrice=200000&SortBy=price&SortDescending=true&SkipCount=0&MaxResultCount=10" `
    -Headers $headers
```

### 2. POST /api/app/property (Crear)

```powershell
$newProperty = @{
    title = "Casa Moderna en Centro"
    description = "Hermosa casa con acabados de lujo"
    location = "Centro, CDMX"
    price = 350000
    area = 120.5
    bedrooms = 3
    bathrooms = 2
    architectId = "GUID-DEL-ARQUITECTO"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44345/api/app/property" `
    -Method POST `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $newProperty
```

### 3. GET /api/app/property/{id} (Obtener Detalle)

```powershell
$propertyId = "GUID-DE-PROPIEDAD"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property/$propertyId" `
    -Headers $headers
```

### 4. PUT /api/app/property/{id} (Actualizar)

```powershell
$propertyId = "GUID-DE-PROPIEDAD"
$updateProperty = @{
    title = "Casa Moderna Actualizada"
    description = "Descripción actualizada"
    location = "Centro, CDMX"
    price = 380000
    area = 120.5
    bedrooms = 3
    bathrooms = 2
    architectId = "GUID-DEL-ARQUITECTO"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44345/api/app/property/$propertyId" `
    -Method PUT `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $updateProperty
```

### 5. POST /api/app/property/{id}/publish (Publicar)

```powershell
$propertyId = "GUID-DE-PROPIEDAD"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property/$propertyId/publish" `
    -Method POST `
    -Headers $headers
```

### 6. POST /api/app/property/{id}/archive (Archivar)

```powershell
$propertyId = "GUID-DE-PROPIEDAD"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property/$propertyId/archive" `
    -Method POST `
    -Headers $headers
```

### 7. DELETE /api/app/property/{id} (Eliminar)

```powershell
$propertyId = "GUID-DE-PROPIEDAD"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property/$propertyId" `
    -Method DELETE `
    -Headers $headers
```

## ?? Pruebas de Endpoints - Architects

### 1. POST /api/app/architect (Crear Perfil)

```powershell
$newArchitect = @{
    bio = "Arquitecto con 10 años de experiencia"
    portfolioUrl = "https://miportfolio.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44345/api/app/architect" `
    -Method POST `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $newArchitect
```

### 2. GET /api/app/architect/{id}

```powershell
$architectId = "GUID-DEL-ARQUITECTO"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/architect/$architectId" `
    -Headers $headers
```

### 3. GET /api/app/architect/by-user-id/{userId}

```powershell
$userId = "GUID-DEL-USER"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/architect/by-user-id/$userId" `
    -Headers $headers
```

### 4. PUT /api/app/architect/{id}

```powershell
$architectId = "GUID-DEL-ARQUITECTO"
$updateArchitect = @{
    bio = "Arquitecto especializado en diseño sustentable"
    portfolioUrl = "https://nuevo-portfolio.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:44345/api/app/architect/$architectId" `
    -Method PUT `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $updateArchitect
```

## ?? Pruebas de Endpoints - Contact Requests

### 1. POST /api/app/contact-request (Sin Autenticación)

```powershell
$contactRequest = @{
    propertyId = "GUID-DE-PROPIEDAD"
    name = "Juan Pérez"
    email = "juan@example.com"
    phone = "5551234567"
    message = "Me interesa esta propiedad"
} | ConvertTo-Json

# Sin headers (público)
Invoke-RestMethod -Uri "https://localhost:44345/api/app/contact-request" `
    -Method POST `
    -ContentType "application/json" `
    -Body $contactRequest
```

### 2. GET /api/app/contact-request (Lista - Requiere Auth)

```powershell
Invoke-RestMethod -Uri "https://localhost:44345/api/app/contact-request?SkipCount=0&MaxResultCount=10" `
    -Headers $headers
```

### 3. GET /api/app/contact-request/by-architect/{architectId}

```powershell
$architectId = "GUID-DEL-ARQUITECTO"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/contact-request/by-architect/$architectId?SkipCount=0&MaxResultCount=10" `
    -Headers $headers
```

### 4. POST /api/app/contact-request/{id}/mark-as-replied

```powershell
$contactId = "GUID-DEL-CONTACTO"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/contact-request/$contactId/mark-as-replied?replyNotes=Contactado+vía+email" `
    -Method POST `
    -Headers $headers
```

### 5. POST /api/app/contact-request/{id}/close

```powershell
$contactId = "GUID-DEL-CONTACTO"
Invoke-RestMethod -Uri "https://localhost:44345/api/app/contact-request/$contactId/close" `
    -Method POST `
    -Headers $headers
```

## ?? Verificación de Endpoints Generados

### Obtener Swagger JSON (Todos los Endpoints)

```powershell
Invoke-RestMethod -Uri "https://localhost:44345/swagger/v1/swagger.json" | 
    ConvertTo-Json -Depth 10 | 
    Out-File swagger-endpoints.json

# Ver solo paths de Property
$swagger = Invoke-RestMethod -Uri "https://localhost:44345/swagger/v1/swagger.json"
$swagger.paths | Get-Member -MemberType NoteProperty | Where-Object { $_.Name -like "*property*" }
```

### Verificar UI de Swagger

Abrir en navegador:
```
https://localhost:44345/swagger
```

## ?? Troubleshooting

### Si sigues viendo 400 Bad Request en GET /api/app/property

```powershell
# Verificar que los query params se están enviando correctamente
Invoke-RestMethod -Uri "https://localhost:44345/api/app/property" `
    -Headers $headers `
    -Verbose

# Verificar en logs del servidor que PropertyFiltersDto se está bindeando
```

### Si ves 403 Forbidden

```powershell
# Verificar que el token es válido
$token

# Verificar permisos del usuario actual
Invoke-RestMethod -Uri "https://localhost:44345/api/app/permission-management/permissions?providerName=R&providerKey=admin" `
    -Headers $headers
```

### Si ves 404 Not Found

```powershell
# Verificar que las convenciones están configuradas
# Revisar cimaHttpApiModule.cs
# Verificar que los namespaces son correctos

# Listar todos los endpoints disponibles
$swagger = Invoke-RestMethod -Uri "https://localhost:44345/swagger/v1/swagger.json"
$swagger.paths.PSObject.Properties.Name | Sort-Object
```

## ?? Resultados Esperados

### ? Éxito
```json
{
  "totalCount": 10,
  "items": [
    {
      "id": "guid",
      "title": "Casa Moderna",
      "price": 350000,
      ...
    }
  ]
}
```

### ? Error 400 (Si PropertyFiltersDto no se bindea)
```json
{
  "error": {
    "code": "400",
    "message": "One or more validation errors occurred.",
    "validationErrors": [...]
  }
}
```

### ? Error 403 (Permisos)
```json
{
  "error": {
    "code": "Volo.Abp.Authorization.AbpAuthorizationException",
    "message": "Authorization failed!"
  }
}
```

### ? Error 404 (Endpoint no encontrado)
```json
{
  "error": {
    "code": "404",
    "message": "The specified endpoint was not found."
  }
}
```

## ?? Checklist de Verificación

- [ ] Blazor detenido antes de compilar
- [ ] `dotnet clean` ejecutado
- [ ] `dotnet build` exitoso sin errores
- [ ] Aplicación corriendo en puerto 44345
- [ ] Token de autenticación obtenido
- [ ] GET /api/app/property funciona sin filtros
- [ ] GET /api/app/property funciona CON filtros (MinPrice, MaxPrice, etc.)
- [ ] POST /api/app/property crea correctamente
- [ ] Permisos verificados en UI admin
- [ ] Swagger UI muestra todos los endpoints esperados
