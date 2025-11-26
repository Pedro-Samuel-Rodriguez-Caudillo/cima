# CONFIGURACION PENDIENTE - DOMINIO Y CORREO

Este archivo centraliza TODOS los lugares donde debes actualizar el dominio y correo de administrador cuando los tengas.

---

## INFORMACION REQUERIDA

Antes de deployment a produccion, necesitas:

1. **Dominio**: Ejemplo: `tuinmobiliaria.com` o `cima-propiedades.com`
2. **Correo Admin**: Ejemplo: `admin@tudominio.com` o `contacto@tudominio.com`
3. **IP del Servidor**: Ejemplo: `192.168.1.100` (staging) y `192.168.1.200` (production)

---

## ARCHIVOS A MODIFICAR

### 1. VARIABLES DE ENTORNO (.env)

#### .env.staging
```
Ubicacion: .env.staging
Linea: 6

ACTUAL:
APP_SELF_URL=https://staging.cima.com

CAMBIAR A:
APP_SELF_URL=https://staging.TU_DOMINIO.com
```

#### .env.production
```
Ubicacion: .env.production
Linea: 4

ACTUAL:
APP_SELF_URL=https://cima.com

CAMBIAR A:
APP_SELF_URL=https://TU_DOMINIO.com
```

---

### 2. NGINX CONFIGURATION

#### etc/nginx/conf.d/cima.conf
```
Ubicacion: etc/nginx/conf.d/cima.conf
Lineas: 10, 24, 168

ACTUAL (3 lugares):
server_name cima.com www.cima.com;

CAMBIAR A:
server_name TU_DOMINIO.com www.TU_DOMINIO.com;
```

---

### 3. SCRIPTS DE DEPLOYMENT

#### etc/scripts/setup-ssl.sh
```
Ubicacion: etc/scripts/setup-ssl.sh
Linea: 3-4 (comentarios de ejemplo)

ACTUAL:
# Uso: sudo ./etc/scripts/setup-ssl.sh cima.com admin@cima.com

CAMBIAR A:
# Uso: sudo ./etc/scripts/setup-ssl.sh TU_DOMINIO.com admin@TU_DOMINIO.com
```

#### docs/GITHUB_SECRETS_SETUP.md
```
Ubicacion: docs/GITHUB_SECRETS_SETUP.md
Multiples lineas con ejemplos

BUSCAR Y REEMPLAZAR:
cima.com -> TU_DOMINIO.com
admin@cima.com -> CORREO_ADMIN
staging.cima.com -> staging.TU_DOMINIO.com
```

---

### 4. DOCKER REGISTRY

#### .env.staging y .env.production
```
Ubicacion: Ambos archivos .env.staging y .env.production
Linea: 7

ACTUAL:
DOCKER_REGISTRY=ghcr.io/pedro-samuel-rodriguez-caudillo

OPCIONES:
1. Mantener si usas GitHub Container Registry con tu usuario
2. Cambiar a: ghcr.io/TU_USUARIO_GITHUB
3. Cambiar a Docker Hub: TU_USUARIO_DOCKERHUB
```

---

### 5. DOCUMENTACION

#### docs/NGINX_CONFIGURATION.md
```
Ubicacion: docs/NGINX_CONFIGURATION.md
Multiples menciones a cima.com

BUSCAR Y REEMPLAZAR:
cima.com -> TU_DOMINIO.com
staging.cima.com -> staging.TU_DOMINIO.com
admin@cima.com -> CORREO_ADMIN
```

#### docs/DIA_8_NGINX_COMPLETADO.md
```
Ubicacion: docs/DIA_8_NGINX_COMPLETADO.md

BUSCAR Y REEMPLAZAR:
cima.com -> TU_DOMINIO.com
admin@cima.com -> CORREO_ADMIN
```

#### docs/DEPLOYMENT_GUIDE.md (si existe)
```
Todas las referencias a dominios y correos
```

---

## CONFIGURACION DNS

Cuando tengas el dominio, configura los registros DNS:

```
Tipo    Nombre              Valor                   TTL
A       @                   IP_SERVIDOR_PROD        3600
A       www                 IP_SERVIDOR_PROD        3600
A       staging             IP_SERVIDOR_STAGING     3600
```

Ejemplo con dominio real:
```
Tipo    Nombre              Valor                   TTL
A       @                   192.168.1.200           3600
A       www                 192.168.1.200           3600
A       staging             192.168.1.100           3600
```

---

## PASOS DE ACTUALIZACION

### PASO 1: Buscar y Reemplazar Global

Usar busqueda en todo el proyecto:

```
BUSCAR: cima.com
REEMPLAZAR CON: TU_DOMINIO.com

BUSCAR: www.cima.com
REEMPLAZAR CON: www.TU_DOMINIO.com

BUSCAR: staging.cima.com
REEMPLAZAR CON: staging.TU_DOMINIO.com

BUSCAR: admin@cima.com
REEMPLAZAR CON: TU_CORREO_ADMIN
```

Excluir de la busqueda:
- Carpeta: `bin/`
- Carpeta: `obj/`
- Carpeta: `node_modules/`
- Carpeta: `.git/`
- Archivos: `*.dll`, `*.exe`

### PASO 2: Actualizar Variables de Entorno

```powershell
# Editar archivos .env
notepad .env.staging
notepad .env.production

# Verificar cambios
Get-Content .env.staging | Select-String -Pattern "APP_SELF_URL"
Get-Content .env.production | Select-String -Pattern "APP_SELF_URL"
```

### PASO 3: Actualizar Nginx Config

```powershell
# Editar configuracion de Nginx
notepad etc\nginx\conf.d\cima.conf

# Buscar todas las lineas con server_name
Get-Content etc\nginx\conf.d\cima.conf | Select-String -Pattern "server_name"

# Debe mostrar 3 lineas, todas con tu nuevo dominio
```

### PASO 4: Actualizar Scripts

```powershell
# Editar script de SSL
notepad etc\scripts\setup-ssl.sh

# Verificar que los ejemplos usan tu dominio
Get-Content etc\scripts\setup-ssl.sh | Select-String -Pattern "cima.com"
# No debe mostrar resultados si ya reemplazaste
```

### PASO 5: Actualizar Documentacion

```powershell
# Lista de archivos de documentacion a revisar
$docs = @(
    "docs\GITHUB_SECRETS_SETUP.md",
    "docs\NGINX_CONFIGURATION.md",
    "docs\DIA_8_NGINX_COMPLETADO.md",
    "docs\DIA_8_FASE_1_COMPLETADA.md"
)

foreach ($doc in $docs) {
    Write-Host "Revisando: $doc" -ForegroundColor Cyan
    $content = Get-Content $doc -Raw
    if ($content -match "cima\.com") {
        Write-Host "  Necesita actualizacion" -ForegroundColor Yellow
    } else {
        Write-Host "  OK" -ForegroundColor Green
    }
}
```

---

## VERIFICACION FINAL

Antes de deployment, ejecutar este script:

```powershell
# Script de verificacion de configuracion
$dominioPrueba = "cima.com"
$tuDominio = "TU_DOMINIO.com"  # ACTUALIZA ESTO

$archivos = @(
    ".env.staging",
    ".env.production",
    "etc\nginx\conf.d\cima.conf",
    "etc\scripts\setup-ssl.sh"
)

Write-Host "Verificando configuracion de dominio..." -ForegroundColor Cyan
Write-Host ""

$errores = 0

foreach ($archivo in $archivos) {
    if (Test-Path $archivo) {
        $content = Get-Content $archivo -Raw
        if ($content -match $dominioPrueba) {
            Write-Host "ERROR: $archivo aun contiene '$dominioPrueba'" -ForegroundColor Red
            $errores++
        } else {
            Write-Host "OK: $archivo" -ForegroundColor Green
        }
    } else {
        Write-Host "WARNING: $archivo no existe" -ForegroundColor Yellow
    }
}

Write-Host ""
if ($errores -eq 0) {
    Write-Host "Verificacion completa - Listo para deployment" -ForegroundColor Green
} else {
    Write-Host "Encontrados $errores errores - Revisa la configuracion" -ForegroundColor Red
}
```

---

## OBTENER CERTIFICADOS SSL

Una vez que tengas dominio configurado en DNS:

```bash
# Conectar a servidor de produccion
ssh user@IP_SERVIDOR_PROD

# Navegar a directorio del proyecto
cd /opt/cima/production

# Ejecutar script de SSL con TU dominio y correo
sudo ./etc/scripts/setup-ssl.sh TU_DOMINIO.com TU_CORREO_ADMIN

# Ejemplo real:
# sudo ./etc/scripts/setup-ssl.sh inmobiliaria-xyz.com admin@inmobiliaria-xyz.com
```

---

## CHECKLIST FINAL PRE-DEPLOYMENT

Antes de hacer deployment a produccion:

- [ ] Dominio comprado y configurado en registrar
- [ ] DNS apuntando a IP del servidor (A records)
- [ ] Correo de admin configurado y funcionando
- [ ] Archivo .env.staging actualizado
- [ ] Archivo .env.production actualizado
- [ ] etc/nginx/conf.d/cima.conf actualizado
- [ ] etc/scripts/setup-ssl.sh actualizado
- [ ] Documentacion actualizada
- [ ] Script de verificacion ejecutado sin errores
- [ ] Servidor de production preparado
- [ ] Servidor de staging preparado
- [ ] SSH keys configuradas
- [ ] GitHub Secrets configurados con nuevo dominio

---

## EJEMPLO COMPLETO

Si tu dominio es `inmobiliaria-premium.com` y tu correo es `contacto@inmobiliaria-premium.com`:

### .env.production
```env
POSTGRES_USER=cima_prod_user
POSTGRES_PASSWORD=MI_PASSWORD_SEGURO
POSTGRES_DB=cima_prod
APP_SELF_URL=https://inmobiliaria-premium.com
BUILD_NUMBER=1.0.0
DOCKER_REGISTRY=ghcr.io/mi-usuario
REDIS_CONNECTION=redis:6379
ASPNETCORE_ENVIRONMENT=Production
```

### etc/nginx/conf.d/cima.conf
```nginx
server {
    listen 80;
    server_name inmobiliaria-premium.com www.inmobiliaria-premium.com;
    # ...
}

server {
    listen 443 ssl http2;
    server_name inmobiliaria-premium.com www.inmobiliaria-premium.com;
    # ...
}
```

### Comando SSL
```bash
sudo ./etc/scripts/setup-ssl.sh inmobiliaria-premium.com contacto@inmobiliaria-premium.com
```

---

## NOTAS IMPORTANTES

1. **NO COMMITEAR** archivos .env con passwords reales
2. **SIEMPRE** usar `.env.*.example` para plantillas
3. **VERIFICAR DNS** antes de solicitar certificados SSL
4. **PROBAR** en staging antes de production
5. **DOCUMENTAR** los cambios de configuracion

---

## SOPORTE

Si tienes dudas sobre la configuracion:

1. Revisar `docs/NGINX_CONFIGURATION.md`
2. Revisar `docs/GITHUB_SECRETS_SETUP.md`
3. Revisar `docs/DIA_8_FASE_1_COMPLETADA.md`
4. Ejecutar script de verificacion

---

**IMPORTANTE**: Este archivo es una guia. Los cambios reales deben hacerse archivo por archivo verificando el contexto.

**RECOMENDACION**: Hacer un branch separado para estos cambios y probar en staging antes de production.
