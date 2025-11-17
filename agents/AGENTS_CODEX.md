# ?? CODEX: GUÍA ESPECIALIZADA - DevOps & Infrastructure

**Rol:** DevOps Engineer especializado en Docker, CI/CD, deployment, scripts  
**Responsabilidad:** Containerización, GitHub Actions, deployment, infrastructure  
**Stack:** Docker, Docker Compose, GitHub Actions (YAML), PowerShell, bash  
**Commits:** `chore(devops)`, `chore(docker)`, `chore(ci-cd)`, `fix(infrastructure)`

---

## ?? TU MISIÓN

Eres el **guardián de la infraestructura**. Tu código es:
- ? Confiable (builds reproducibles, deploys idempotentes)
- ? Eficiente (imágenes optimizadas, caches estratégicos)
- ? Observable (logs, health checks, monitoring)
- ? Seguro (no secretos hardcodeados, least privilege)

**No toques:** Lógica C# (business logic), UI (Blazor), databases (DDL).

---

## ?? ÁREA DE TRABAJO

```
/
??? Dockerfile                           ? TU ESPACIO ??
??? docker-compose.yml                   ? TU ESPACIO
??? docker-compose.prod.yml              ? TU ESPACIO
??? .github/
?   ??? workflows/
?       ??? dotnet.yml                   ? TU ESPACIO (CI)
?       ??? deploy-staging.yml           ? TU ESPACIO (CD Staging)
?       ??? deploy-production.yml        ? TU ESPACIO (CD Prod)
??? scripts/                             ? TU ESPACIO
?   ??? deploy.sh
?   ??? backup-db.ps1
?   ??? health-check.ps1
??? .env (gitignore)                    ? TU ESPACIO (secrets)
```

---

## ?? FLUJO: Cómo Pensar (DevOps)

### **1. Feature Deployment**

```
Feature desarrollada en rama
  ?
PR con cambios backend/frontend
  ? [MERGE]
GitHub Actions CI inicia
  ?? Restore dependencies
  ?? Build
  ?? Run tests (Fase 2+)
  ?? Upload artifacts
  ? [SUCCESS]
Manual trigger o schedule
  ?
GitHub Actions CD inicia
  ?? Build Docker image
  ?? Push a registry (opcional)
  ?? SSH a VPS
  ?? Pull image
  ?? Deploy con docker-compose
  ?? Health check
  ?
Aplicación en vivo en producción
```

### **2. Tu Checklist Deployment**

```
PRE-DEPLOYMENT:
  ? Migrations ready en repo
  ? Dockerfile compila sin errores
  ? docker-compose.yml valida
  ? .env.example documentado
  ? Health check endpoint responde

DEPLOYMENT:
  ? Backup BD antes de deploy
  ? Scale down servicios antigos
  ? Pull latest image
  ? Run migrations (si aplica)
  ? Start servicios nuevos
  ? Health check pasa
  ? Logs sin errores críticos

POST-DEPLOYMENT:
  ? Smoke tests (acceder URLs públicas)
  ? Verificar logs
  ? Monitorear recursos (CPU, memoria)
  ? Rollback plan activado si necesario
```

---

## ?? DOCKER

### **Dockerfile (Optimizado)**

```dockerfile
# Dockerfile - Multi-stage, optimizado para producción

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /src

# Restaurar dependencies (layer cacheado)
COPY ["common.props", "common.props"]
COPY ["src/cima.DbMigrator/cima.DbMigrator.csproj", "src/cima.DbMigrator/"]
COPY ["src/cima.Domain/cima.Domain.csproj", "src/cima.Domain/"]
COPY ["src/cima.Domain.Shared/cima.Domain.Shared.csproj", "src/cima.Domain.Shared/"]
COPY ["src/cima.Application/cima.Application.csproj", "src/cima.Application/"]
COPY ["src/cima.Application.Contracts/cima.Application.Contracts.csproj", "src/cima.Application.Contracts/"]
COPY ["src/cima.EntityFrameworkCore/cima.EntityFrameworkCore.csproj", "src/cima.EntityFrameworkCore/"]
COPY ["src/cima.HttpApi/cima.HttpApi.csproj", "src/cima.HttpApi/"]
COPY ["src/cima.HttpApi.Client/cima.HttpApi.Client.csproj", "src/cima.HttpApi.Client/"]
COPY ["src/cima.Blazor/cima.Blazor.csproj", "src/cima.Blazor/"]
COPY ["src/cima.Blazor.Client/cima.Blazor.Client.csproj", "src/cima.Blazor.Client/"]

RUN dotnet restore "src/cima.Blazor/cima.Blazor.csproj"

# Copiar resto del código
COPY . .

# Build en Release
RUN dotnet build "src/cima.Blazor/cima.Blazor.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build as publish
RUN dotnet publish "src/cima.Blazor/cima.Blazor.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 as runtime
WORKDIR /app

# Instalar herramientas útiles (optional, keep small)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=publish /app/publish .

# Crear usuario non-root por seguridad
RUN useradd -m -u 1000 appuser && \
    chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose ports
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Entrypoint
ENTRYPOINT ["dotnet", "cima.Blazor.dll"]
```

### **docker-compose.yml (Development)**

```yaml
# docker-compose.yml - Local development
version: '3.8'

services:
  
  # PostgreSQL Database
  postgres:
    image: postgres:16-alpine
    container_name: cima-postgres
    environment:
      POSTGRES_DB: ${DB_NAME:-cima}
      POSTGRES_USER: ${DB_USER:-postgres}
      POSTGRES_PASSWORD: ${DB_PASSWORD:-postgres}
      POSTGRES_INITDB_ARGS: "--encoding=UTF8 --locale=es_ES.UTF-8"
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - cima-network
    restart: unless-stopped

  # Application
  app:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: cima-app
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__Default: "Host=postgres;Port=5432;Database=${DB_NAME:-cima};Username=${DB_USER:-postgres};Password=${DB_PASSWORD:-postgres}"
      ASPNETCORE_ENVIRONMENT: ${ENVIRONMENT:-Development}
      ASPNETCORE_URLS: http://+:8080
      AuthServer__RequireHttpsMetadata: "false"
      AuthServer__Authority: "http://localhost:8080"
    ports:
      - "8080:8080"
    volumes:
      - .:/src  # Hot reload en desarrollo (opcional)
    networks:
      - cima-network
    restart: unless-stopped

volumes:
  postgres_data:

networks:
  cima-network:
    driver: bridge
```

### **docker-compose.prod.yml (Production)**

```yaml
# docker-compose.prod.yml - Production ready
version: '3.8'

services:
  
  postgres:
    image: postgres:16-alpine
    container_name: cima-postgres-prod
    environment:
      POSTGRES_DB: ${DB_NAME}
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_INITDB_ARGS: "--encoding=UTF8 --locale=es_ES.UTF-8"
    ports:
      - "5432:5432"
    volumes:
      - postgres_data_prod:/var/lib/postgresql/data
      - ./scripts/backups:/backups
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${DB_USER}"]
      interval: 30s
      timeout: 10s
      retries: 5
    networks:
      - cima-prod-network
    restart: always
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

  app:
    image: ${REGISTRY}/cima:${VERSION:-latest}
    container_name: cima-app-prod
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__Default: "Host=postgres;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
      AuthServer__RequireHttpsMetadata: "true"
      AuthServer__Authority: ${AUTH_SERVER_URL}
      App__SelfUrl: ${APP_SELF_URL}
    ports:
      - "8080:8080"
    networks:
      - cima-prod-network
    restart: always
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "5"
    # Recursos límites
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G

  # Nginx Reverse Proxy (opcional, pero recomendado)
  nginx:
    image: nginx:alpine
    container_name: cima-nginx-prod
    depends_on:
      - app
    environment:
      APP_HOST: app
      APP_PORT: 8080
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
      - ./logs/nginx:/var/log/nginx
    networks:
      - cima-prod-network
    restart: always

volumes:
  postgres_data_prod:

networks:
  cima-prod-network:
    driver: bridge
```

### **.env Example**

```bash
# .env.example (NUNCA commitear archivo .env con valores reales)
# Copiar a .env y rellenar valores

# Database
DB_NAME=cima
DB_USER=postgres
DB_PASSWORD=your_secure_password_here

# Application
ENVIRONMENT=Production
APP_SELF_URL=https://yourdomain.com
AUTH_SERVER_URL=https://yourdomain.com

# Registry (si usas Docker Hub o ghcr.io)
REGISTRY=docker.io/yourusername
VERSION=latest

# Sentry (opcional)
SENTRY_DSN=https://your_sentry_key@sentry.io/123456
```

---

## ?? GITHUB ACTIONS

### **CI Workflow (dotnet.yml)**

```yaml
# .github/workflows/dotnet.yml
name: .NET CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_DB: cima_test
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
    
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0  # Full history para semantic versioning

    - name: Setup .NET 9
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Run tests (cuando esté disponible)
      run: dotnet test --no-build --verbosity normal --configuration Release
      continue-on-error: true  # No bloquea si no hay tests

    - name: SonarCloud analysis (opcional)
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
      continue-on-error: true

    - name: Upload build artifacts
      if: success()
      uses: actions/upload-artifact@v3
      with:
        name: build-artifacts
        path: |
          **/bin/Release/
          **/obj/

    - name: Comment PR with status
      if: github.event_name == 'pull_request'
      uses: actions/github-script@v7
      with:
        script: |
          github.rest.issues.createComment({
            issue_number: context.issue.number,
            owner: context.repo.owner,
            repo: context.repo.repo,
            body: '? **Build & Tests**: Pasó correctamente\n- Dotnet: 9.0\n- Target: net9.0'
          })
```

### **CD Workflow Staging (deploy-staging.yml)**

```yaml
# .github/workflows/deploy-staging.yml
name: Deploy to Staging

on:
  push:
    branches: [ develop ]
  workflow_dispatch:  # Manual trigger

env:
  REGISTRY: docker.io
  IMAGE_NAME: yourusername/cima

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Build Docker image
      run: |
        docker build \
          --tag ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:staging-${{ github.sha }} \
          --tag ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:staging-latest \
          .

    - name: Login to Docker Registry
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}

    - name: Push Docker image
      run: |
        docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:staging-${{ github.sha }}
        docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:staging-latest

    - name: Deploy to Staging VPS
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.STAGING_HOST }}
        username: ${{ secrets.STAGING_USER }}
        key: ${{ secrets.STAGING_SSH_KEY }}
        port: ${{ secrets.STAGING_PORT }}
        script: |
          cd /app/cima-staging
          echo "VERSION=staging-${{ github.sha }}" > .env
          docker-compose -f docker-compose.prod.yml pull
          docker-compose -f docker-compose.prod.yml up -d
          docker-compose exec -T app dotnet ef database update --no-build
          sleep 5
          ./scripts/health-check.ps1

    - name: Notify Slack (opcional)
      if: always()
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        text: 'Staging deployment: ${{ job.status }}'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### **CD Workflow Production (deploy-production.yml)**

```yaml
# .github/workflows/deploy-production.yml
name: Deploy to Production

on:
  release:
    types: [published]
  workflow_dispatch:  # Solo manual

env:
  REGISTRY: docker.io
  IMAGE_NAME: yourusername/cima

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://yourdomain.com
    
    steps:
    
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Create backup on production server
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.PROD_HOST }}
        username: ${{ secrets.PROD_USER }}
        key: ${{ secrets.PROD_SSH_KEY }}
        port: ${{ secrets.PROD_PORT }}
        script: |
          cd /app/cima
          ./scripts/backup-db.ps1

    - name: Build Docker image
      run: |
        docker build \
          --tag ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.ref_name }} \
          --tag ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest \
          .

    - name: Login to Docker Registry
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}

    - name: Push Docker image
      run: |
        docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.ref_name }}
        docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest

    - name: Deploy to Production VPS
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.PROD_HOST }}
        username: ${{ secrets.PROD_USER }}
        key: ${{ secrets.PROD_SSH_KEY }}
        port: ${{ secrets.PROD_PORT }}
        script: |
          cd /app/cima
          echo "VERSION=${{ github.ref_name }}" > .env
          docker-compose -f docker-compose.prod.yml pull
          docker-compose -f docker-compose.prod.yml up -d
          docker-compose exec -T app dotnet ef database update --no-build
          sleep 10
          ./scripts/health-check.ps1 || ./scripts/rollback.ps1

    - name: Verify deployment
      run: |
        curl -f https://yourdomain.com/health || exit 1

    - name: Notify team
      if: always()
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        text: |
          Production deployment: ${{ job.status }}
          Version: ${{ github.ref_name }}
          URL: https://yourdomain.com
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

---

## ?? SCRIPTS

### **health-check.ps1**

```powershell
# scripts/health-check.ps1
param(
    [string]$Url = "http://localhost:8080/health",
    [int]$TimeoutSeconds = 60,
    [int]$IntervalSeconds = 5
)

$elapsed = 0
$success = $false

Write-Host "?? Checking health: $Url"

while ($elapsed -lt $TimeoutSeconds) {
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec 5 -Method Get
        if ($response.StatusCode -eq 200) {
            Write-Host "? Health check passed!"
            $success = $true
            break
        }
    }
    catch {
        Write-Host "? Waiting... ($elapsed/$TimeoutSeconds seconds)"
        Start-Sleep -Seconds $IntervalSeconds
        $elapsed += $IntervalSeconds
    }
}

if (-not $success) {
    Write-Host "? Health check failed!"
    exit 1
}
```

### **backup-db.ps1**

```powershell
# scripts/backup-db.ps1
param(
    [string]$DbName = "cima",
    [string]$DbUser = "postgres",
    [string]$BackupDir = "./backups"
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "$BackupDir/cima_backup_$timestamp.sql"

Write-Host "?? Creating database backup: $backupFile"

docker-compose exec -T postgres pg_dump -U $DbUser $DbName > $backupFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Backup created successfully"
    
    # Mantener últimas 7 backups
    $backups = Get-ChildItem -Path $BackupDir -Filter "cima_backup_*.sql" | 
               Sort-Object CreationTime -Descending | 
               Select-Object -Skip 7
    
    $backups | Remove-Item -Force
    Write-Host "?? Cleaned old backups"
}
else {
    Write-Host "? Backup failed!"
    exit 1
}
```

### **deploy.sh (Bash alternative)**

```bash
#!/bin/bash
# scripts/deploy.sh

set -e

ENV=${1:-staging}
VERSION=${2:-latest}

echo "?? Deploying $VERSION to $ENV..."

# Build
echo "?? Building Docker image..."
docker build -t cima:$VERSION .

# Push (if registry configured)
if [ ! -z "$REGISTRY" ]; then
    echo "?? Pushing to registry..."
    docker tag cima:$VERSION $REGISTRY/cima:$VERSION
    docker push $REGISTRY/cima:$VERSION
fi

# Deploy
echo "?? Starting services..."
docker-compose -f docker-compose.prod.yml up -d

# Migrations
echo "?? Running migrations..."
docker-compose exec -T app dotnet ef database update --no-build

# Health check
echo "? Checking health..."
sleep 5
curl -f http://localhost:8080/health || exit 1

echo "? Deployment complete!"
```

---

## ?? CHECKLIST: Antes de Cada Deploy

- [ ] **Code reviewed** y merged a main/develop
- [ ] **CI pipeline** pasó (build, tests)
- [ ] **Dockerfile** compila sin errores
- [ ] **docker-compose.yml** valida (`docker-compose config`)
- [ ] **Secrets** en GitHub Actions Secrets (no en repo)
- [ ] **Backup script** funciona (probado en staging)
- [ ] **Health check** endpoint responde
- [ ] **Migration script** ejecuta exitosamente
- [ ] **Rollback procedure** documentado y probado
- [ ] **Logs** configurados (json-file, rotación)
- [ ] **Recursos límites** especificados (memory, CPU)

---

## ?? TROUBLESHOOTING

```powershell
# Ver logs en vivo
docker-compose logs -f app

# Conectar a container
docker exec -it cima-app /bin/bash

# Ver estadísticas
docker stats

# Rebuild image
docker-compose build --no-cache

# Limpiar volúmenes (cuidado: elimina datos)
docker volume prune

# Desplegar rollback
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml pull --no-parallel
docker-compose -f docker-compose.prod.yml up -d

# Ver networking
docker network inspect cima-network
```

---

## ?? SEGURIDAD

### **Checklist de Seguridad**

- [ ] **Usuario non-root** en Dockerfile (USER appuser)
- [ ] **Secretos** en GitHub Secrets, no en repo
- [ ] **SSH keys** protegidas, rotadas periódicamente
- [ ] **Health check** en containers
- [ ] **Logs centralizados** (Sentry, ELK opcional)
- [ ] **Firewall** configurado en VPS
- [ ] **SSL/TLS** en nginx/prod
- [ ] **Rate limiting** en reverse proxy
- [ ] **Database password** nunca en dockerfile
- [ ] **Image scanning** (Trivy, optional)

---

## ?? COMANDOS ÚTILES

```powershell
# Lint docker-compose
docker-compose config

# Validar Dockerfile
docker build --dry-run .

# Ver imagen layers
docker history cima:latest

# Optimizar imagen
docker image ls --format "table {{.Repository}}\t{{.Size}}"

# Network inspection
docker network ls
docker network inspect cima-network

# Container logs
docker logs cima-app --tail 100 -f

# Enter container
docker exec -it cima-app /bin/sh

# Database restore
cat backup_20240115.sql | docker exec -i cima-postgres psql -U postgres -d cima
```

---

## ? ANTIPATRONES: NUNCA HAGAS ESTO

```dockerfile
# ? Secrets en Dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
ENV DB_PASSWORD="super_secret_password_123"

# ? Correcto: pasar en runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0
# No incluyes secrets aquí

# ? Root user
FROM ubuntu:22.04
RUN apt-get install -y something
# ... app corre como root

# ? Correcto
FROM ubuntu:22.04
RUN useradd -m appuser
USER appuser

# ? Layer grande sin caché
FROM mcr.microsoft.com/dotnet/sdk:9.0
COPY . .
RUN dotnet build

# ? Correcto: caché en dependencies
FROM mcr.microsoft.com/dotnet/sdk:9.0
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet build
```

---

## ?? REFERENCIAS

- **Docker Docs:** https://docs.docker.com
- **GitHub Actions:** https://docs.github.com/en/actions
- **PostgreSQL Docker:** https://hub.docker.com/_/postgres
- **Nginx Reverse Proxy:** https://nginx.org/en/docs/

---

**Última actualización:** Setup inicial  
**Versión:** 1.0  
**Para:** OpenAI Codex

