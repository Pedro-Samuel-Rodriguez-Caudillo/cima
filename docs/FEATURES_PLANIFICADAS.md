# Features Planificadas - CIMA

**Fecha de planificación**: 2024-01-XX  
**Estado**: Pendiente de implementación  
**Prioridad**: Alta

---

## Resumen Ejecutivo

Se han identificado 4 features de alto impacto para mejorar la conversión y engagement de la plataforma inmobiliaria CIMA. Estas features están diseñadas específicamente para el mercado local mexicano y priorizan la generación de leads de calidad.

---

## 1. SendGrid Email Service ??

### Objetivo
Automatizar notificaciones por email cuando usuarios contactan sobre propiedades.

### Alcance
- Email al arquitecto dueño cuando recibe una consulta
- Email de confirmación al usuario que contactó
- Templates HTML profesionales con branding CIMA

### Beneficios
- ? Respuesta inmediata al usuario (mejor UX)
- ?? Tracking de conversiones por propiedad
- ?? Arquitectos responden más rápido = más ventas

### Especificaciones Técnicas
- **Servicio**: SendGrid (100 emails/día gratis)
- **Archivos a crear**:
  - `src/cima.Application.Contracts/Email/IEmailService.cs`
  - `src/cima.Application/Email/SendGridEmailService.cs`
- **Modificaciones**:
  - `ContactRequestAppService.cs` (enviar emails en `CreateAsync`)
  - `appsettings.json` (configuración SendGrid)

### Configuración Requerida
```json
{
  "SendGrid": {
    "ApiKey": "SG.xxxxxxxxxxxxxxxxxxxxx",
    "FromEmail": "noreply@cima.com",
    "FromName": "CIMA Propiedades"
  }
}
```

### Dependencias
- Package NuGet: `SendGrid` (>=9.28.0)
- API Key de SendGrid (obtener en https://sendgrid.com)

### Tiempo Estimado
2-3 horas

---

## 2. WhatsApp Click-to-Chat ??

### Objetivo
Facilitar contacto directo vía WhatsApp desde detalle de propiedad.

### Alcance
- Botón "Contactar por WhatsApp" en página de detalle
- Pre-llena mensaje con información de la propiedad
- Funciona en web y móvil (abre WhatsApp Web o app)

### Beneficios
- ?? 70% de usuarios mexicanos prefieren WhatsApp
- ? Contacto instantáneo sin formularios
- ?? Mayor tasa de conversión que email

### Especificaciones Técnicas
- **Migración DB requerida**: Agregar `PhoneNumber` a tabla `Architects`
- **Archivos a crear**:
  - `src/cima.Blazor.Client/Components/Public/WhatsAppButton.razor`
- **Modificaciones**:
  - `ArchitectDto.cs` (agregar `PhoneNumber`, `AllowWhatsAppContact`)
  - `Architect.cs` (agregar propiedades)
  - `Detail.razor` (integrar botón)
- **Migración**:
  ```csharp
  migrationBuilder.AddColumn<string>(
      name: "PhoneNumber",
      table: "Architects",
      type: "character varying(20)",
      maxLength: 20,
      nullable: true);
  ```

### Formato de Número
- Internacional: `+5215512345678`
- Validación: Regex `^\+52[0-9]{10}$`

### Ejemplo de Uso
```csharp
// URL generada
https://wa.me/5215512345678?text=Hola,%20me%20interesa%20la%20propiedad:%20Casa%20en%20Polanco
```

### Tiempo Estimado
1 hora (sin migración) + 30 min (migración)

---

## 3. Propiedades Similares ???

### Objetivo
Mostrar 3 propiedades similares cuando usuario ve un detalle, aumentando tiempo en sitio y opciones de contacto.

### Alcance
- Algoritmo de recomendación basado en:
  - Precio (±30% de tolerancia)
  - Ubicación (misma ciudad)
  - Tipo de propiedad (casa/departamento)
  - Recámaras (±1)
- Componente visual al final de página de detalle

### Beneficios
- ?? Aumenta páginas vistas por sesión
- ?? Usuario encuentra alternativas sin salir del sitio
- ?? Mayor probabilidad de conversión

### Especificaciones Técnicas
- **Sin migración DB requerida**
- **Archivos a crear**:
  - `src/cima.Blazor.Client/Components/Public/SimilarProperties.razor`
- **Modificaciones**:
  - `ListingAppService.cs` (agregar `GetSimilarListingsAsync`)
  - `IListingAppService.cs` (agregar método al contrato)
  - `Detail.razor` (integrar componente)

### Algoritmo de Recomendación
```csharp
1. Obtener propiedad actual
2. Calcular rango de precio (±30%)
3. Extraer ciudad de ubicación
4. Filtrar por:
   - Mismo tipo (PropertyType)
   - Precio en rango
   - Misma ciudad
   - Recámaras ±1
5. Ordenar por proximidad de precio
6. Retornar top 3
```

### Performance
- Cache: Considerar cachear resultados por 1 hora
- Query optimization: Index en `Price`, `Location`, `Type`

### Tiempo Estimado
2-3 horas

---

## 4. Mapa Interactivo con Leaflet ???

### Objetivo
Visualizar propiedades en mapa interactivo OpenStreetMap, permitiendo búsqueda geográfica.

### Alcance
- Mapa en página de índice de propiedades
- Markers por propiedad con popup de información
- Click en marker abre detalle de propiedad
- Filtros se reflejan en mapa en tiempo real

### Beneficios
- ?? Usuarios buscan por zona/colonia visualmente
- ??? Mejor comprensión de ubicación
- ?? Diferenciador vs competencia

### Especificaciones Técnicas
- **Librería**: Leaflet.js (gratuita, sin API key)
- **Tiles**: OpenStreetMap (gratuito)
- **Migración DB requerida**: Agregar `Latitude`, `Longitude` a tabla `Listings`
- **Archivos a crear**:
  - `src/cima.Blazor.Client/Components/Public/PropertyMap.razor`
  - `src/cima.Blazor.Client/wwwroot/js/propertyMap.js`
- **Modificaciones**:
  - `ListingDto.cs` (agregar coordenadas)
  - `Listing.cs` (agregar propiedades)
  - `Index.razor` (integrar mapa)
  - `App.razor` (agregar CDN de Leaflet)

### Migración DB
```csharp
migrationBuilder.AddColumn<decimal>(
    name: "Latitude",
    table: "Listings",
    type: "numeric(10,7)",
    nullable: true);

migrationBuilder.AddColumn<decimal>(
    name: "Longitude",
    table: "Listings",
    type: "numeric(10,7)",
    nullable: true);
```

### CDN Requerido (App.razor)
```html
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
```

### Geocoding (Opcional)
Para convertir direcciones a coordenadas automáticamente:
- **Servicio gratuito**: Nominatim (OpenStreetMap)
- **API**: `https://nominatim.openstreetmap.org/search`
- **Límite**: 1 request/segundo

### Tiempo Estimado
3-4 horas (sin geocoding automático)

---

## Orden de Implementación Recomendado

### Semana 1: Quick Wins
1. **Email Service** (Día 1-2)
   - Setup SendGrid
   - Implementar servicio
   - Testing con emails reales

2. **WhatsApp** (Día 3)
   - Migración DB
   - Componente botón
   - Testing en móvil

### Semana 2: Features Avanzadas
3. **Propiedades Similares** (Día 4-5)
   - Implementar algoritmo
   - Componente UI
   - Testing de recomendaciones

4. **Mapa Interactivo** (Día 6-8)
   - Migración DB
   - Integración Leaflet
   - Geocoding de propiedades existentes
   - Testing en diferentes zooms

---

## Dependencias Técnicas

### Packages NuGet a Instalar
```bash
# Email Service
dotnet add src/cima.Application package SendGrid

# (WhatsApp y Mapa no requieren packages)
```

### Migraciones DB Requeridas
1. `AddPhoneToArchitect` (WhatsApp)
2. `AddCoordinatesToListing` (Mapa)

### Scripts de Migración
```bash
# Crear migraciones
dotnet ef migrations add AddPhoneToArchitect -p src/cima.EntityFrameworkCore
dotnet ef migrations add AddCoordinatesToListing -p src/cima.EntityFrameworkCore

# Aplicar migraciones
dotnet run --project src/cima.DbMigrator
```

---

## Configuración de Entorno

### Variables de Entorno (Production)
```bash
# SendGrid
SENDGRID_API_KEY=SG.xxxxxxxxxxxx
SENDGRID_FROM_EMAIL=noreply@cima.com
SENDGRID_FROM_NAME=CIMA Propiedades

# WhatsApp (opcional - para analytics)
WHATSAPP_BUSINESS_NUMBER=+5215512345678
```

### appsettings.Secrets.json (Development)
```json
{
  "SendGrid": {
    "ApiKey": "SG.development_key_here",
    "FromEmail": "dev@cima.com",
    "FromName": "CIMA DEV"
  }
}
```

---

## Métricas de Éxito

### KPIs a Trackear
1. **Email Service**
   - Tasa de apertura (target: >40%)
   - Tiempo promedio de respuesta arquitecto (target: <2h)
   - Emails rebotados (target: <5%)

2. **WhatsApp**
   - Click-through rate en botón (target: >15%)
   - Conversiones WhatsApp vs Email (comparativa)

3. **Propiedades Similares**
   - CTR en propiedades similares (target: >8%)
   - Páginas por sesión (antes vs después)

4. **Mapa**
   - % usuarios que interactúan con mapa (target: >30%)
   - Conversión usuarios que usan mapa (target: +10% vs búsqueda tradicional)

---

## Riesgos y Mitigaciones

### Riesgo 1: Emails marcados como SPAM
- **Mitigación**: Configurar SPF, DKIM, DMARC en dominio
- **Backup**: Twilio SendGrid tiene buena reputación por defecto

### Riesgo 2: Números WhatsApp incorrectos
- **Mitigación**: Validación en frontend + backend
- **Backup**: Permitir editar número en perfil de arquitecto

### Riesgo 3: Coordenadas GPS faltantes
- **Mitigación**: UI para agregar manualmente en admin
- **Backup**: Geocoding automático con Nominatim

### Riesgo 4: Performance con muchos markers en mapa
- **Mitigación**: Clustering con Leaflet.markercluster
- **Backup**: Limitar a 100 propiedades visibles simultáneamente

---

## Checklist Pre-Implementación

### Antes de comenzar Email Service
- [ ] Crear cuenta SendGrid (gratis)
- [ ] Verificar dominio en SendGrid
- [ ] Configurar SPF/DKIM/DMARC en DNS
- [ ] Obtener API Key
- [ ] Diseñar templates HTML de emails

### Antes de comenzar WhatsApp
- [ ] Validar números de teléfono de arquitectos existentes
- [ ] Definir formato estándar (internacional)
- [ ] Preparar script de migración DB
- [ ] Testing en WhatsApp Web y móvil

### Antes de comenzar Propiedades Similares
- [ ] Analizar distribución de precios actual
- [ ] Validar que existan suficientes propiedades por zona
- [ ] Definir tolerancias (precio, recámaras)

### Antes de comenzar Mapa
- [ ] Recopilar coordenadas GPS de propiedades actuales
- [ ] Configurar Nominatim o servicio de geocoding
- [ ] Testing de performance con 100+ markers
- [ ] Preparar script de migración DB

---

## Referencias

- [SendGrid Documentation](https://docs.sendgrid.com/)
- [Leaflet.js Documentation](https://leafletjs.com/)
- [WhatsApp Click to Chat API](https://faq.whatsapp.com/general/chats/how-to-use-click-to-chat)
- [Nominatim Geocoding](https://nominatim.org/release-docs/latest/)

---

## Notas Adicionales

### Consideraciones de Negocio
- Estas features están diseñadas para **inmobiliaria local mexicana**
- Priorizan **conversión sobre métricas de vanidad**
- Requieren **mínima inversión** (SendGrid gratis, Leaflet gratis, WhatsApp gratis)

### Alternativas Evaluadas y Descartadas
? **Sistema de Favoritos**: Poco impacto para inmobiliaria local  
? **Comparación de Propiedades**: Complejidad alta, beneficio bajo  
? **Calculadora de Hipoteca**: Requiere integración bancaria  
? **Tours Virtuales 360°**: Depende de contenido de arquitectos  
? **Notificaciones Push**: Requiere app móvil (no prioridad)

### Próximos Pasos
1. Revisar y aprobar este documento
2. Crear issues en GitHub para cada feature
3. Asignar sprint/milestone
4. Comenzar implementación en orden recomendado

---

**Documento creado**: 2024-01-XX  
**Última actualización**: 2024-01-XX  
**Autor**: Equipo CIMA  
**Aprobado por**: Pendiente
