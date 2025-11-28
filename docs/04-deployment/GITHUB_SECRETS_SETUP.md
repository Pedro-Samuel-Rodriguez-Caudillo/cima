# CONFIGURACION DE SECRETS DE GITHUB

Este documento describe como configurar los secrets necesarios para los workflows de CI/CD.

## SECRETS REQUERIDOS PARA STAGING

Navega a: Settings > Secrets and variables > Actions > New repository secret

### STAGING_HOST
- **Descripcion**: IP o hostname del servidor de staging
- **Ejemplo**: `staging.cima.com` o `192.168.1.100`

### STAGING_USER
- **Descripcion**: Usuario SSH para conectar al servidor de staging
- **Ejemplo**: `deploy` o `ubuntu`

### STAGING_SSH_KEY
- **Descripcion**: Clave privada SSH para autenticacion (formato OpenSSH)
- **Como obtenerla**:
  ```bash
  # En tu maquina local, genera un par de claves
  ssh-keygen -t ed25519 -C "github-actions-staging" -f ~/.ssh/cima_staging_deploy
  
  # Copia la clave publica al servidor de staging
  ssh-copy-id -i ~/.ssh/cima_staging_deploy.pub user@staging-server
  
  # Copia la clave privada completa (incluyendo headers)
  cat ~/.ssh/cima_staging_deploy
  ```
- **Formato**:
  ```
  -----BEGIN OPENSSH PRIVATE KEY-----
  b3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW
  ...
  -----END OPENSSH PRIVATE KEY-----
  ```

### STAGING_SSH_PORT (Opcional)
- **Descripcion**: Puerto SSH si no es el default (22)
- **Ejemplo**: `2222`

---

## SECRETS REQUERIDOS PARA PRODUCTION

### PRODUCTION_HOST
- **Descripcion**: IP o hostname del servidor de produccion
- **Ejemplo**: `cima.com` o `192.168.1.200`

### PRODUCTION_USER
- **Descripcion**: Usuario SSH para conectar al servidor de produccion
- **Ejemplo**: `deploy`

### PRODUCTION_SSH_KEY
- **Descripcion**: Clave privada SSH para autenticacion en produccion
- **Como obtenerla**: Mismo proceso que staging, pero con archivo diferente
  ```bash
  ssh-keygen -t ed25519 -C "github-actions-production" -f ~/.ssh/cima_prod_deploy
  ssh-copy-id -i ~/.ssh/cima_prod_deploy.pub user@production-server
  cat ~/.ssh/cima_prod_deploy
  ```

### PRODUCTION_SSH_PORT (Opcional)
- **Descripcion**: Puerto SSH si no es el default
- **Ejemplo**: `22`

---

## SECRETS DE DOCKER REGISTRY

Los workflows usan GitHub Container Registry (ghcr.io) que se autentica automaticamente con `GITHUB_TOKEN`.

Si necesitas usar Docker Hub u otro registry:

### DOCKER_USERNAME
- **Descripcion**: Usuario de Docker Hub
- **Ejemplo**: `pedro-samuel-rodriguez-caudillo`

### DOCKER_TOKEN
- **Descripcion**: Token de acceso de Docker Hub
- **Como obtenerlo**:
  1. Login en Docker Hub
  2. Account Settings > Security > New Access Token
  3. Nombrar: `github-actions-cima`
  4. Copiar el token generado

---

## CONFIGURACION DE ENVIRONMENTS EN GITHUB

### Crear Environment "staging"
1. Settings > Environments > New environment
2. Nombre: `staging`
3. Environment protection rules (opcional):
   - Required reviewers: 0
   - Wait timer: 0 minutes
   - Deployment branches: `develop`

### Crear Environment "production"
1. Settings > Environments > New environment
2. Nombre: `production`
3. Environment protection rules (RECOMENDADO):
   - Required reviewers: 1 (tu usuario)
   - Wait timer: 5 minutes
   - Deployment branches: `master` y tags `v*.*.*`

---

## VERIFICACION DE SECRETS

Para verificar que los secrets estan configurados correctamente:

```bash
# Prueba de conexion SSH a staging
ssh -i ~/.ssh/cima_staging_deploy user@staging-server "echo 'Conexion exitosa'"

# Prueba de conexion SSH a production
ssh -i ~/.ssh/cima_prod_deploy user@production-server "echo 'Conexion exitosa'"
```

---

## PREPARACION DE SERVIDORES

### Servidor Staging

```bash
# Conectarse al servidor
ssh user@staging-server

# Crear directorio de aplicacion
sudo mkdir -p /opt/cima/staging
sudo chown -R $USER:$USER /opt/cima/staging
cd /opt/cima/staging

# Clonar repositorio
git clone https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima.git .
git checkout develop

# Copiar archivo de environment
cp .env.staging.example .env.staging

# Editar variables de entorno
nano .env.staging
# Actualizar:
# - POSTGRES_PASSWORD
# - APP_SELF_URL

# Instalar Docker y Docker Compose
sudo apt-get update
sudo apt-get install -y docker.io docker-compose
sudo usermod -aG docker $USER
```

### Servidor Production

```bash
# Conectarse al servidor
ssh user@production-server

# Crear directorio de aplicacion
sudo mkdir -p /opt/cima/production
sudo chown -R $USER:$USER /opt/cima/production
cd /opt/cima/production

# Clonar repositorio
git clone https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima.git .

# Copiar archivo de environment
cp .env.production.example .env.production

# IMPORTANTE: Editar variables de entorno
nano .env.production
# Actualizar:
# - POSTGRES_PASSWORD (usar password seguro)
# - APP_SELF_URL (dominio real)

# Crear script de backup
nano backup.sh
# (copiar contenido del workflow)
chmod +x backup.sh

# Instalar Docker y Docker Compose
sudo apt-get update
sudo apt-get install -y docker.io docker-compose
sudo usermod -aG docker $USER
```

---

## FLUJO DE DEPLOYMENT

### Deployment a Staging (Automatico)
1. Push a branch `develop`
2. GitHub Actions ejecuta workflow `cd-deploy-staging.yml`
3. Construye imagen Docker
4. Sube a ghcr.io
5. Conecta via SSH a servidor staging
6. Descarga nueva imagen
7. Reinicia contenedores
8. Verifica health check

### Deployment a Production (Manual Approval)
1. Push a branch `master` o crear tag `v1.0.0`
2. GitHub Actions ejecuta workflow `cd-deploy-production.yml`
3. Construye imagen Docker
4. Sube a ghcr.io
5. **ESPERA APROBACION MANUAL** (si configurado)
6. Crea backup de BD e imagenes
7. Conecta via SSH a servidor production
8. Descarga nueva imagen
9. Reinicia contenedores con graceful shutdown
10. Verifica health check (con retries)
11. Si falla, hace rollback automatico
12. Crea GitHub Release (si es tag)

---

## TROUBLESHOOTING

### Error: "Permission denied (publickey)"
**Solucion**: Verificar que la clave SSH esta agregada al servidor
```bash
ssh-copy-id -i ~/.ssh/cima_staging_deploy.pub user@server
```

### Error: "Docker: command not found"
**Solucion**: Instalar Docker en el servidor
```bash
sudo apt-get update && sudo apt-get install -y docker.io docker-compose
```

### Error: "Health check failed"
**Solucion**: Verificar logs del contenedor
```bash
ssh user@server
docker logs cima-blazor-staging
```

### Error: "No space left on device"
**Solucion**: Limpiar imagenes Docker antiguas
```bash
ssh user@server
docker system prune -a -f
```

---

## SEGURIDAD

1. **NUNCA** commitear archivos `.env` con passwords reales
2. Usar passwords fuertes para PostgreSQL en staging y production
3. Rotar SSH keys cada 6 meses
4. Habilitar firewall en servidores (solo puertos 22, 80, 443)
5. Configurar SSL/TLS con Let's Encrypt para dominios publicos
6. Revisar logs de deployment regularmente
7. Configurar alertas para fallos de deployment

---

## CHECKLIST FINAL

- [ ] Secrets de staging configurados en GitHub
- [ ] Secrets de production configurados en GitHub
- [ ] Environments creados (staging, production)
- [ ] Servidor staging preparado
- [ ] Servidor production preparado
- [ ] SSH keys copiadas a servidores
- [ ] Archivos .env configurados en servidores
- [ ] Docker instalado en ambos servidores
- [ ] Script de backup creado en production
- [ ] Health check endpoint funcional
- [ ] Prueba de deployment a staging exitosa
- [ ] Revision de seguridad completada
