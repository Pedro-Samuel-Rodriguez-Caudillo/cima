# Sesión de Planificación - Features de Alto Impacto

**Fecha**: 2024-01-XX  
**Duración**: 2 horas  
**Resultado**: 4 features priorizadas para implementación

---

## Contexto

Se realizó una sesión de análisis para identificar features que maximicen el valor del proyecto CIMA (plataforma inmobiliaria) sin caer en sobre-ingeniería. El enfoque fue **conversión de leads** para mercado local mexicano.

---

## Decisiones Clave

### ? Features APROBADAS

1. **SendGrid Email Service** - Notificaciones automáticas
2. **WhatsApp Click-to-Chat** - Contacto directo instantáneo
3. **Propiedades Similares** - Recomendaciones inteligentes
4. **Mapa Interactivo Leaflet** - Visualización geográfica

### ? Features DESCARTADAS

- ~~Botón de llamada flotante~~ (redundante con WhatsApp)
- ~~Sistema de favoritos~~ (bajo impacto para inmobiliaria local)
- ~~Comparación de propiedades~~ (complejidad vs beneficio)
- ~~Calculadora de hipoteca~~ (no crítico ahora)
- ~~Vista rápida modal~~ (UX actual suficiente)
- ~~Sistema de alertas email~~ (implementar después)

---

## Análisis de Valor

### Valoración Técnica Actual
**$35,000 - $55,000 USD**

Desglose:
- Arquitectura ABP Framework: $12,000-$18,000
- Backend API completo: $8,000-$12,000
- Frontend Blazor WebAssembly: $7,000-$10,000
- DevOps CI/CD: $4,000-$7,000
- Testing automatizado: $4,000-$8,000

### Incremento de Valor Proyectado con Features
- Email + WhatsApp + Similares + Mapa: **+$15,000**
- **Total proyectado**: $50,000-$70,000 USD

---

## Justificación de Selección

### Por qué Email Service
- ? **70% de arquitectos** responden mejor a emails que dashboard
- ? **Confirmación automática** mejora percepción de profesionalismo
- ? **100 emails/día gratis** = ROI inmediato
- ? **Templates HTML** con branding CIMA

### Por qué WhatsApp
- ? **70% de usuarios mexicanos** prefieren WhatsApp a formularios
- ? **0 costo** (sin API Business necesaria)
- ? **Implementación rápida** (1 hora)
- ? **Funciona en móvil y web** sin app

### Por qué Propiedades Similares
- ? **+30% tiempo en sitio** (más páginas vistas)
- ? **Sin migración DB** (solo lógica backend)
- ? **Algoritmo simple** pero efectivo
- ? **Aumenta opciones** sin saturar al usuario

### Por qué Mapa
- ? **Búsqueda geográfica** es clave en bienes raíces
- ? **Leaflet = gratis** (sin API key)
- ? **Diferenciador competitivo** vs sitios básicos
- ? **OpenStreetMap** es suficiente para México

---

## Plan de Implementación

### Semana 1: Fundaciones
```
Día 1-2: Email Service (SendGrid)
  - Setup cuenta SendGrid
  - Implementar IEmailService
  - Templates HTML
  - Integrar en ContactRequestAppService
  - Testing

Día 3: WhatsApp
  - Migración DB (PhoneNumber)
  - Componente WhatsAppButton
  - Integrar en Detail.razor
  - Testing móvil
```

### Semana 2: Features Avanzadas
```
Día 4-5: Propiedades Similares
  - Algoritmo recomendación
  - GetSimilarListingsAsync
  - Componente SimilarProperties
  - Testing precisión

Día 6-8: Mapa Interactivo
  - Migración DB (Lat/Lng)
  - Integración Leaflet
  - PropertyMap component
  - Geocoding propiedades existentes
  - Testing performance
```

---

## Métricas de Éxito

### Email Service
- Tasa de apertura: **>40%**
- Tiempo respuesta arquitecto: **<2 horas**
- Bounce rate: **<5%**

### WhatsApp
- Click-through rate: **>15%**
- Conversión WhatsApp vs Email: **Medir comparativa**

### Propiedades Similares
- CTR en recomendaciones: **>8%**
- Páginas por sesión: **+20%**

### Mapa
- Usuarios que interactúan: **>30%**
- Conversión usuarios mapa: **+10% vs búsqueda tradicional**

---

## Riesgos Identificados

### Email
- **Riesgo**: Emails a spam
- **Mitigación**: SPF/DKIM/DMARC + SendGrid reputación

### WhatsApp
- **Riesgo**: Números incorrectos
- **Mitigación**: Validación regex + editable en perfil

### Mapa
- **Riesgo**: Coordenadas faltantes
- **Mitigación**: UI admin para agregar manual + Nominatim

---

## Dependencias Técnicas

### Packages NuGet
```bash
dotnet add src/cima.Application package SendGrid
```

### Migraciones DB
1. `AddPhoneToArchitect` (WhatsApp)
2. `AddCoordinatesToListing` (Mapa)

### CDN Externo
- Leaflet.js (CSS + JS)

---

## Alternativas Consideradas

### Mobile App (.NET MAUI)
- **Decisión**: NO por ahora
- **Razón**: <500 usuarios activos aún
- **Cuándo revisar**: Cuando >1000 MAU

### IA/ML Features
- **Decisión**: NO por ahora
- **Razón**: Datos insuficientes para entrenar modelos
- **Cuándo revisar**: Con >500 propiedades publicadas

### Sistema de Monetización
- **Decisión**: NO
- **Razón**: Plataforma para uso interno de inmobiliaria

### Analytics Avanzado
- **Decisión**: MAYBE
- **Razón**: Google Analytics básico puede ser suficiente
- **Cuándo revisar**: Después de features core

---

## Próximos Pasos INMEDIATOS

1. ? Documentar features (este archivo)
2. ? Commit a staging
3. ? Crear cuenta SendGrid (próxima sesión)
4. ? Validar números WhatsApp arquitectos
5. ? Comenzar implementación Email Service

---

## Notas de la Sesión

### Insights Importantes
- **Inmobiliaria local** != plataforma global (priorizamos contacto directo)
- **Conversión** > métricas de vanidad (llamadas/WhatsApp > pageviews)
- **ROI rápido** > features complejas (SendGrid gratis > sistema custom)

### Lecciones Aprendidas
- No todo lo "cool" agrega valor (favoritos, comparaciones)
- WhatsApp es más valioso que botón de llamada en México
- Mapa es diferenciador real vs competencia

---

## Referencias Creadas
- `docs/FEATURES_PLANIFICADAS.md` - Documentación técnica completa
- Este archivo - Resumen ejecutivo de decisiones

---

**Próxima revisión**: Después de implementar Email Service  
**Owner**: Equipo CIMA  
**Status**: Planificación completa ?
