# CONFIGURACIÓN GITHUB PAGES COMO STAGING

## ?? ESCENARIO ACTUAL

Tu aplicación usa **Blazor Web App con WebAssembly interactivo**, no Blazor WebAssembly standalone. Esto significa:

- ? El proyecto `cima.Blazor` (servidor) sirve la app
- ? El proyecto `cima.Blazor.Client` (wasm) se descarga al navegador
- ? No puede ejecutarse 100% standalone en GitHub Pages sin servidor

## ?? OPCIONES DISPONIBLES

### OPCIÓN 1: GitHub Pages para Frontend Público (RECOMENDADO)

Publicar **solo las páginas públicas** (sin autenticación) en GitHub Pages como demo/staging.

**Ventajas:**
- ? Fácil de configurar
- ? No requiere backend
- ? Perfecto para mostrar diseño y UX
- ? Gratis con GitHub Pages

**Limitaciones:**
- ?? Solo páginas públicas (sin login)
- ?? Datos mock/estáticos

### OPCIÓN 2: Desplegar Blazor Server a Servicio Gratuito

Usar servicios como **Railway**, **Render**, o **Azure Web Apps Free Tier** para staging completo.

**Ventajas:**
- ? Funcionalidad completa
- ? Base de datos incluida
- ? Autenticación funciona

**Limitaciones:**
- ?? Requiere configuración adicional
- ?? Limitaciones en tier gratuito

### OPCIÓN 3: Docker Local + ngrok para Testing

Ejecutar localmente y exponer con ngrok para testing temporal.

**Ventajas:**
- ? Funcionalidad completa
- ? Fácil debugging

**Limitaciones:**
- ?? No es permanente
- ?? Requiere PC encendida

---

## ?? IMPLEMENTACIÓN OPCIÓN 1: GitHub Pages (Páginas Públicas)

### Paso 1: Crear Proyecto Standalone para GitHub Pages

Voy a crear un proyecto Blazor WebAssembly standalone solo con las páginas públicas.

**Estructura:**
```
src/
  cima.Blazor.GHPages/     ? NUEVO: Solo frontend público
    Pages/
      Index.razor          ? Hero + Featured
      Properties/
        Index.razor        ? Listado de propiedades
        Detail.razor       ? Detalle de propiedad
    Services/
      MockDataService.cs   ? Datos de ejemplo
    wwwroot/
      sample-data/
        properties.json    ? JSON con propiedades de ejemplo
```

### Paso 2: Configurar GitHub Actions

El workflow `.github/workflows/cd-deploy-staging.yml` ya está actualizado para:
1. Build Blazor WebAssembly
2. Configurar base path para `/cima`
3. Deploy a GitHub Pages

### Paso 3: Habilitar GitHub Pages en tu Repositorio

1. Ir a: https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/settings/pages
2. En **Source**, seleccionar: **GitHub Actions**
3. Guardar

### Paso 4: Crear Secretos (Opcional)

Si quieres conectar a una API de staging externa:
```
STAGING_API_URL=https://tu-api-staging.com
STAGING_AUTH_URL=https://tu-api-staging.com
```

---

## ?? IMPLEMENTACIÓN OPCIÓN 2: Railway (Staging Completo)

### ¿Qué es Railway?
Servicio PaaS gratuito que permite deployar Docker containers con BD PostgreSQL incluida.

**Límites del tier gratuito:**
- ? $5 crédito mensual
- ? PostgreSQL incluido
- ? 500 MB RAM
- ? Suficiente para staging

### Paso 1: Crear Cuenta en Railway
1. Ir a: https://railway.app/
2. Login con GitHub
3. Crear nuevo proyecto

### Paso 2: Configurar Variables de Entorno
```bash
POSTGRES_USER=cima_staging
POSTGRES_PASSWORD=<generar_password_seguro>
POSTGRES_DB=cima_staging
APP_SELF_URL=https://cima-staging.up.railway.app
```

### Paso 3: Deploy Automático desde GitHub
Railway detectará el `Dockerfile` y hará deploy automático.

---

## ?? IMPLEMENTACIÓN OPCIÓN 3: Azure Web Apps (Tier Gratuito)

### Límites del tier gratuito:
- ? 60 minutos CPU/día
- ? 1 GB RAM
- ? 1 GB storage
- ? Azure SQL Database (250 MB)

### Paso 1: Crear Azure Web App
```bash
az webapp create \
  --resource-group cima-staging \
  --plan cima-staging-plan \
  --name cima-staging \
  --runtime "DOTNETCORE|9.0"
```

### Paso 2: Configurar Connection String
```bash
az webapp config connection-string set \
  --resource-group cima-staging \
  --name cima-staging \
  --connection-string-type PostgreSQL \
  --settings DefaultConnection="Host=..;Database=.."
```

---

## ?? COMPARACIÓN DE OPCIONES

| Característica | GitHub Pages | Railway | Azure Web Apps |
|----------------|-------------|---------|----------------|
| **Costo** | Gratis | $5/mes gratis | Gratis (limitado) |
| **Backend** | ? No | ? Si | ? Si |
| **Base de Datos** | ? No | ? PostgreSQL | ? SQL/PostgreSQL |
| **Autenticación** | ? No | ? Si | ? Si |
| **Setup** | ????? Muy fácil | ???? Fácil | ??? Medio |
| **Ideal para** | Demo público | Staging completo | Producción light |

---

## ?? RECOMENDACIÓN

**Para tu caso (sin servidor de producción aún):**

### Plan de 2 Etapas:

#### ETAPA 1 (AHORA): GitHub Pages para Demo
- ? Deploy páginas públicas a GitHub Pages
- ? Mostrar diseño y UX
- ? Sin backend, datos estáticos
- ? URL: `https://pedro-samuel-rodriguez-caudillo.github.io/cima/`

#### ETAPA 2 (CUANDO TENGAS DOMINIO): Railway para Staging Completo
- ? Deploy completo con backend
- ? Base de datos PostgreSQL
- ? Autenticación funcional
- ? URL: `https://cima-staging.up.railway.app`

---

## ? ACCIÓN INMEDIATA

### Si quieres GitHub Pages (demo público):

1. **Habilitar GitHub Pages:**
   - Ir a Settings > Pages
   - Source: GitHub Actions

2. **Crear proyecto standalone:**
   ```powershell
   # Te creo el proyecto ahora
   ```

3. **Push y deploy automático:**
   ```powershell
   git add .
   git commit -m "feat(ghpages): setup GitHub Pages deployment"
   git push origin develop
   ```

### Si quieres Railway (staging completo):

1. **Crear cuenta en Railway**
2. **Conectar repositorio**
3. **Configurar variables de entorno**
4. **Deploy automático**

---

## ? ¿QUÉ PREFIERES?

**Opción A:** GitHub Pages (demo público sin backend)
- Más rápido de setup
- Solo frontend
- Perfecto para mostrar diseño

**Opción B:** Railway (staging completo)
- Setup en 10 minutos
- Funcionalidad completa
- Gratis con $5 crédito

**Opción C:** Ambos
- GitHub Pages para demo
- Railway para staging interno

---

**Dime qué opción prefieres y procedo con la configuración completa.**
