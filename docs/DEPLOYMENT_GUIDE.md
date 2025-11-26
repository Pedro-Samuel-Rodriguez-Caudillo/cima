# DEPLOYMENT GUIDE - CIMA Inmobiliaria

Esta guia cubre el deployment completo de la aplicacion CIMA desde ambiente local hasta produccion.

---

## TABLA DE CONTENIDOS

1. [Pre-requisitos](#pre-requisitos)
2. [Arquitectura de Deployment](#arquitectura-de-deployment)
3. [Ambientes](#ambientes)
4. [Deployment Local](#deployment-local)
5. [Deployment a Staging](#deployment-a-staging)
6. [Deployment a Produccion](#deployment-a-produccion)
7. [Rollback](#rollback)
8. [Troubleshooting](#troubleshooting)
9. [Monitoring](#monitoring)

---

## PRE-REQUISITOS

### Infraestructura Requerida

#### Servidor Staging
- **OS**: Ubuntu 22.04 LTS o superior
- **CPU**: 2 cores minimo
- **RAM**: 4 GB minimo
- **Disco**: 50 GB minimo
- **Red**: IP publica o acceso via VPN

#### Servidor Produccion
- **OS**: Ubuntu 22.04 LTS o superior
- **CPU**: 4 cores minimo (recomendado 8)
- **RAM**: 8 GB minimo (recomendado 16 GB)
- **Disco**: 100 GB minimo (SSD recomendado)
- **Red**: IP publica estatica

### Software Requerido

#### En Servidores (Staging y Production)
```bash
# Docker Engine
sudo apt-get update
sudo apt-get install docker.io docker-compose -y

# Git
sudo apt-get install git -y

# Certbot (para SSL)
sudo apt-get install certbot -y
```

#### En Maquina Local
- Docker Desktop
- Git
- .NET 9 SDK
- PowerShell 7+
- Editor de codigo (VS Code, Visual Studio)

### Accesos y Credenciales

- [ ] Cuenta de GitHub con acceso al repositorio
- [ ] GitHub Container Registry habilitado
- [ ] SSH keys configuradas para servidores
- [ ] Dominio registrado y DNS configurado
- [ ] Correo de administrador configurado
- [ ] Passwords seguros generados para:
  - PostgreSQL production
  - PostgreSQL staging
  - Admin de aplicacion

---

## ARQUITECTURA DE DEPLOYMENT

### Diagrama de Componentes

```
???????????????????????????????????????????????????????????
?                    INTERNET                              ?
???????????????????????????????????????????????????????????
                  ?
                  ? HTTPS (443)
                  ?
???????????????????????????????????????????????????????????
?                NGINX (Reverse Proxy)                     ?
?  - SSL/TLS Termination                                   ?
?  - Rate Limiting                                         ?
?  - Static File Serving                                   ?
?  - Security Headers                                      ?
???????????????????????????????????????????????????????????
                  ?
                  ? HTTP (8080)
                  ?
???????????????????????????????????????????????????????????
?          BLAZOR APPLICATION (Container)                  ?
?  - ASP.NET Core 9                                        ?
?  - Blazor WebAssembly                                    ?
?  - Health Checks: /health, /health/ping, /health/ready  ?
???????????????????????????????????????????????????????????
                  ?
                  ? Port 5432
                  ?
???????????????????????????????????????????????????????????
?          POSTGRESQL (Container)                          ?
?  - Version 16-alpine                                     ?
?  - Persistent Volume                                     ?
?  - Automated Backups                                     ?
???????????????????????????????????????????????????????????
```

### Flujo de CI/CD

```
Developer Commit
      ?
      ?
GitHub Repository
      ?
      ??? CI Workflow (Automatic)
      ?   ?? Build .NET Solution
      ?   ?? Run Unit Tests
      ?   ?? Build Docker Image
      ?   ?? Push to GitHub Container Registry
      ?   ?? Tag: ghcr.io/username/cima:build-123
      ?
      ??? CD Staging (Manual Approval)
      ?   ?? Pull Docker Image
      ?   ?? Run Migrations
      ?   ?? Deploy to Staging Server
      ?   ?? Run Health Checks
      ?   ?? Notify Team
      ?
      ??? CD Production (Manual Approval)
          ?? Pull Docker Image
          ?? Backup Database
          ?? Run Migrations
          ?? Deploy with Zero Downtime
          ?? Run Health Checks
          ?? Smoke Tests
          ?? Notify Team
```

---

## AMBIENTES

### Development (Local)

**URL**: http://localhost:5000  
**Base de Datos**: localhost:5433  
**Docker Compose**: `docker-compose.dev.yml`

**Caracteristicas**:
- Hot reload habilitado
- Logs detallados
- Sin SSL
- Seed data automatico

### Staging

**URL**: https://staging.tudominio.com  
**Base de Datos**: Container interno  
**Docker Compose**: `docker-compose.staging.yml`

**Caracteristicas**:
- SSL con Let's Encrypt
- Configuracion similar a produccion
- Datos de prueba
- Monitoreo basico

### Production

**URL**: https://tudominio.com  
**Base de Datos**: Container interno  
**Docker Compose**: `docker-compose.prod.yml`

**Caracteristicas**:
- SSL con Let's Encrypt
- Backups automaticos
- Monitoreo completo
- Rate limiting estricto
- Datos reales

---

## DEPLOYMENT LOCAL

### Paso 1: Configurar Variables de Entorno

```powershell
# Copiar archivos ejemplo
Copy-Item .env.development.example .env.development

# Editar variables
notepad .env.development
```

Variables criticas en `.env.development`:
```env
POSTGRES_USER=cima_dev_user
POSTGRES_PASSWORD=dev_password_123
POSTGRES_DB=cima_dev
APP_SELF_URL=http://localhost:5000
BUILD_NUMBER=local
DOCKER_REGISTRY=cima
```

### Paso 2: Iniciar Servicios

```powershell
# Script automatico
.\etc\scripts\start-dev-environment.ps1

# O manualmente
docker-compose -f docker-compose.dev.yml up -d
```

### Paso 3: Verificar Health Checks

```powershell
# Esperar 30 segundos a que inicie
Start-Sleep -Seconds 30

# Probar endpoints
Invoke-WebRequest http://localhost:8080/health
Invoke-WebRequest http://localhost:8080/health/ping
Invoke-WebRequest http://localhost:8080/health/ready
```

### Paso 4: Acceder a la Aplicacion

Abrir navegador en: http://localhost:8080

**Credenciales Default**:
- Usuario: `admin`
- Password: `1q2w3E*`

### Paso 5: Detener Servicios

```powershell
.\etc\scripts\stop-dev-environment.ps1
```

---

## DEPLOYMENT A STAGING

### Pre-requisitos

1. Servidor staging preparado
2. Dominio configurado (staging.tudominio.com)
3. SSH access al servidor
4. GitHub Secrets configurados

### Paso 1: Preparar Servidor

```bash
# Conectar via SSH
ssh usuario@IP_SERVIDOR_STAGING

# Crear directorio del proyecto
sudo mkdir -p /opt/cima/staging
sudo chown -R $USER:$USER /opt/cima

# Clonar repositorio
cd /opt/cima/staging
git clone https://github.com/TU_USUARIO/cima.git .
```

### Paso 2: Configurar Variables de Entorno

```bash
# Copiar ejemplo
cp .env.staging.example .env.staging

# Editar con datos reales
nano .env.staging
```

Variables a actualizar:
```env
POSTGRES_USER=cima_staging_user
POSTGRES_PASSWORD=GENERAR_PASSWORD_SEGURO
POSTGRES_DB=cima_staging
APP_SELF_URL=https://staging.tudominio.com
BUILD_NUMBER=1.0.0
DOCKER_REGISTRY=ghcr.io/TU_USUARIO
```

### Paso 3: Configurar SSL

```bash
# Ejecutar script de SSL
sudo ./etc/scripts/setup-ssl.sh staging.tudominio.com admin@tudominio.com

# Verificar certificados
sudo ls -la /etc/letsencrypt/live/staging.tudominio.com/
```

### Paso 4: Iniciar Servicios

```bash
# Pull de imagenes
docker-compose -f docker-compose.staging.yml pull

# Iniciar servicios
docker-compose -f docker-compose.staging.yml up -d

# Ver logs
docker-compose -f docker-compose.staging.yml logs -f
```

### Paso 5: Verificar Deployment

```bash
# Health checks
curl https://staging.tudominio.com/health
curl https://staging.tudominio.com/health/ping
curl https://staging.tudominio.com/health/ready

# Ver estado de containers
docker-compose -f docker-compose.staging.yml ps
```

### Paso 6: Configurar GitHub Actions

En GitHub repository > Settings > Secrets:

```
STAGING_HOST=IP_SERVIDOR_STAGING
STAGING_USER=usuario_ssh
STAGING_SSH_KEY=CONTENIDO_DE_PRIVATE_KEY
DOCKER_USERNAME=tu_usuario_github
DOCKER_PASSWORD=github_personal_access_token
```

### Paso 7: Deploy via GitHub Actions

```powershell
# En tu maquina local
git tag v1.0.0-staging
git push origin v1.0.0-staging
```

GitHub Actions automaticamente:
1. Build la aplicacion
2. Crea imagen Docker
3. Push a GitHub Container Registry
4. Deploy a servidor staging
5. Ejecuta health checks

---

## DEPLOYMENT A PRODUCCION

### Pre-requisitos

1. Staging funcionando correctamente
2. Tests de smoke pasados en staging
3. Backup de produccion actual (si existe)
4. Ventana de mantenimiento acordada

### IMPORTANTE: Checklist Pre-Deployment

- [ ] Codigo en branch `master` probado en staging
- [ ] Migraciones de BD revisadas y probadas
- [ ] Backup de BD de produccion creado
- [ ] Variables de entorno actualizadas
- [ ] Certificados SSL renovados (si es necesario)
- [ ] Equipo notificado del deployment
- [ ] Plan de rollback listo

### Paso 1: Backup de Base de Datos

```bash
# Conectar al servidor de produccion
ssh usuario@IP_SERVIDOR_PROD

# Crear backup
docker exec cima-postgres-prod pg_dump -U cima_prod_user cima_prod > backup_$(date +%Y%m%d_%H%M%S).sql

# Copiar backup a servidor seguro
scp backup_*.sql usuario@backup-server:/backups/cima/
```

### Paso 2: Preparar Servidor

```bash
# Si es primer deployment
sudo mkdir -p /opt/cima/production
sudo chown -R $USER:$USER /opt/cima

cd /opt/cima/production
git clone https://github.com/TU_USUARIO/cima.git .
```

### Paso 3: Configurar Variables

```bash
cp .env.production.example .env.production
nano .env.production
```

Variables criticas:
```env
POSTGRES_USER=cima_prod_user
POSTGRES_PASSWORD=PASSWORD_MUY_SEGURO_GENERADO
POSTGRES_DB=cima_prod
APP_SELF_URL=https://tudominio.com
BUILD_NUMBER=1.0.0
DOCKER_REGISTRY=ghcr.io/TU_USUARIO
REDIS_CONNECTION=redis:6379
ASPNETCORE_ENVIRONMENT=Production
```

### Paso 4: Configurar SSL

```bash
sudo ./etc/scripts/setup-ssl.sh tudominio.com admin@tudominio.com
```

### Paso 5: Deploy con Zero Downtime

```bash
# Pull nueva imagen
docker-compose -f docker-compose.prod.yml pull

# Recrear solo el servicio de aplicacion
docker-compose -f docker-compose.prod.yml up -d --no-deps --build blazor-prod

# Verificar health
curl https://tudominio.com/health

# Si hay problemas, hacer rollback inmediatamente
```

### Paso 6: Ejecutar Migraciones

```bash
# Verificar migraciones pendientes
docker exec cima-blazor-prod dotnet ef database update --dry-run

# Aplicar migraciones
docker exec cima-blazor-prod dotnet ef database update

# Verificar endpoint ready
curl https://tudominio.com/health/ready
```

### Paso 7: Smoke Tests

```bash
# Health checks
curl https://tudominio.com/health | jq
curl https://tudominio.com/health/ping | jq
curl https://tudominio.com/health/ready | jq

# Probar login
curl -X POST https://tudominio.com/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"PASSWORD"}'

# Probar API
curl https://tudominio.com/api/listings | jq
```

### Paso 8: Monitoreo Post-Deployment

```bash
# Ver logs en tiempo real
docker-compose -f docker-compose.prod.yml logs -f blazor-prod

# Ver recursos
docker stats

# Ver health checks
watch -n 5 'curl -s https://tudominio.com/health | jq'
```

### Paso 9: Notificar Equipo

Enviar mensaje al equipo confirmando:
- Version deployada
- Timestamp del deployment
- Health checks pasados
- URL de produccion activa
- Cualquier issue conocido

---

## ROLLBACK

### Rollback Rapido (< 5 minutos)

Si detectas problemas inmediatamente despues del deployment:

```bash
# Opcion 1: Volver a version anterior de imagen
docker-compose -f docker-compose.prod.yml down
docker tag ghcr.io/usuario/cima:1.0.0 ghcr.io/usuario/cima:rollback
docker-compose -f docker-compose.prod.yml up -d

# Opcion 2: Usar imagen especifica anterior
# Editar docker-compose.prod.yml y cambiar BUILD_NUMBER
nano .env.production  # Cambiar BUILD_NUMBER=0.9.0
docker-compose -f docker-compose.prod.yml up -d --force-recreate
```

### Rollback con Restauracion de BD

Si las migraciones causaron problemas:

```bash
# 1. Detener aplicacion
docker-compose -f docker-compose.prod.yml stop blazor-prod

# 2. Restaurar backup de BD
docker exec -i cima-postgres-prod psql -U cima_prod_user cima_prod < backup_TIMESTAMP.sql

# 3. Volver a version anterior de codigo
git checkout v0.9.0
docker-compose -f docker-compose.prod.yml up -d --force-recreate

# 4. Verificar
curl https://tudominio.com/health
```

### Rollback Checklist

- [ ] Notificar al equipo del rollback
- [ ] Documentar la razon del rollback
- [ ] Restaurar BD si es necesario
- [ ] Deploy de version anterior
- [ ] Verificar health checks
- [ ] Confirmar funcionalidad basica
- [ ] Crear ticket para investigar el problema
- [ ] Planear nuevo deployment cuando este corregido

---

## TROUBLESHOOTING

### Problema: Container no inicia

**Sintomas**:
```bash
docker-compose ps
# STATUS: Restarting
```

**Diagnostico**:
```bash
# Ver logs
docker-compose -f docker-compose.prod.yml logs blazor-prod

# Verificar variables de entorno
docker exec cima-blazor-prod env | grep ASPNETCORE

# Verificar salud de BD
docker exec cima-postgres-prod pg_isready
```

**Soluciones**:
1. Verificar connection string de BD
2. Verificar que PostgreSQL esta running
3. Verificar permisos de archivos
4. Revisar health check timeout

### Problema: SSL Certificado Invalido

**Sintomas**:
- Browser muestra "Not Secure"
- `curl` falla con SSL error

**Diagnostico**:
```bash
# Verificar certificados
sudo certbot certificates

# Test SSL
openssl s_client -connect tudominio.com:443
```

**Soluciones**:
```bash
# Renovar certificados
sudo certbot renew

# Forzar renovacion
sudo certbot renew --force-renewal

# Reiniciar Nginx
docker-compose -f docker-compose.prod.yml restart nginx
```

### Problema: Health Check Falla

**Sintomas**:
```bash
curl https://tudominio.com/health
# 503 Service Unavailable
```

**Diagnostico**:
```bash
# Ver respuesta detallada
curl -v https://tudominio.com/health | jq

# Verificar conectividad de BD
docker exec cima-postgres-prod psql -U cima_prod_user -d cima_prod -c "SELECT 1;"

# Ver logs de aplicacion
docker logs cima-blazor-prod --tail 100
```

**Soluciones**:
1. Verificar que BD esta accesible
2. Verificar migraciones aplicadas
3. Reiniciar container de aplicacion
4. Verificar permisos de usuario de BD

### Problema: Alto Uso de Memoria

**Sintomas**:
```bash
docker stats
# cima-blazor-prod: 3.5 GB / 4 GB
```

**Diagnostico**:
```bash
# Ver procesos dentro del container
docker exec cima-blazor-prod ps aux

# Ver memoria por proceso
docker exec cima-blazor-prod top -b -n 1
```

**Soluciones**:
1. Aumentar limites en docker-compose.yml
2. Optimizar queries de BD
3. Habilitar response caching
4. Considerar escalado horizontal

### Problema: Migraciones Fallan

**Sintomas**:
```
The migration 'XXX' has already been applied to the database.
```

**Soluciones**:
```bash
# Ver migraciones aplicadas
docker exec cima-postgres-prod psql -U cima_prod_user -d cima_prod -c "SELECT * FROM __EFMigrationsHistory;"

# Revertir ultima migracion
docker exec cima-blazor-prod dotnet ef database update NOMBRE_MIGRACION_ANTERIOR

# Limpiar y re-aplicar
docker exec cima-blazor-prod dotnet ef database update 0
docker exec cima-blazor-prod dotnet ef database update
```

---

## MONITORING

### Health Checks Automaticos

Docker ya monitorea los health checks configurados en docker-compose:

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

### Monitoreo Manual

Script para monitorear continuamente:

```bash
# etc/scripts/monitor-production.sh
#!/bin/bash

while true; do
  echo "=== $(date) ==="
  
  # Health check
  curl -s https://tudominio.com/health | jq '.status'
  
  # Container status
  docker ps --filter "name=cima" --format "table {{.Names}}\t{{.Status}}"
  
  # Resource usage
  docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"
  
  echo ""
  sleep 60
done
```

### Logs Centralizados

```bash
# Ver todos los logs
docker-compose -f docker-compose.prod.yml logs -f

# Solo aplicacion
docker logs -f cima-blazor-prod

# Solo BD
docker logs -f cima-postgres-prod

# Solo Nginx
docker logs -f cima-nginx-prod
```

### Alertas (Opcional)

Configurar alertas cuando:
- Health check falla 3 veces consecutivas
- Uso de CPU > 80% por 5 minutos
- Uso de memoria > 90%
- Espacio en disco < 10%

Opciones:
- Email via SMTP
- Slack webhook
- PagerDuty
- Prometheus + Alertmanager

---

## MEJORES PRACTICAS

### 1. Deployment

- Siempre probar en staging primero
- Hacer deployments en horarios de bajo trafico
- Tener plan de rollback listo
- Documentar cada deployment

### 2. Backups

- Backup automatico diario de BD
- Retention: 30 dias
- Probar restauracion mensualmente
- Guardar backups en servidor separado

### 3. Security

- Cambiar passwords default inmediatamente
- Usar passwords fuertes (min 16 caracteres)
- Habilitar fail2ban en servidores
- Mantener Docker y OS actualizados
- Renovar certificados SSL antes de expirar

### 4. Monitoring

- Revisar logs diariamente
- Configurar alertas para errores criticos
- Monitorear uso de recursos
- Implementar APM (Application Performance Monitoring)

### 5. Documentacion

- Documentar todos los cambios
- Mantener runbook actualizado
- Documentar incidentes y soluciones
- Compartir conocimiento con el equipo

---

## COMANDOS RAPIDOS

### Health Checks
```bash
curl https://tudominio.com/health | jq
curl https://tudominio.com/health/ping | jq
curl https://tudominio.com/health/ready | jq
```

### Logs
```bash
docker logs -f cima-blazor-prod --tail 100
docker logs -f cima-postgres-prod --tail 50
```

### Reiniciar Servicios
```bash
docker-compose -f docker-compose.prod.yml restart blazor-prod
docker-compose -f docker-compose.prod.yml restart nginx
```

### Backup BD
```bash
docker exec cima-postgres-prod pg_dump -U cima_prod_user cima_prod > backup_$(date +%Y%m%d).sql
```

### Restore BD
```bash
docker exec -i cima-postgres-prod psql -U cima_prod_user cima_prod < backup_TIMESTAMP.sql
```

### Ver Recursos
```bash
docker stats
docker ps
docker images
```

---

## CONTACTO Y SOPORTE

Para problemas durante deployment:

1. Revisar esta guia y troubleshooting section
2. Revisar logs: `docker-compose logs`
3. Verificar health checks
4. Consultar documentacion en `/docs`
5. Crear issue en GitHub con detalles del problema

---

**Version**: 1.0.0  
**Ultima Actualizacion**: Dia 8  
**Mantenido por**: Equipo CIMA

