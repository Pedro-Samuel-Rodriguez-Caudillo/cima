# Git Branching Strategy - CIMA

## Estructura de Branches

### Branches Permanentes
- **`main`** - Producción (protegida)
- **`staging`** - Pre-producción para QA (protegida)
- **`develop`** - Integración continua de features (protegida)

### Branches Temporales
- **`feature/[nombre]`** - Nuevas funcionalidades
- **`bugfix/[nombre]`** - Correcciones de bugs en develop
- **`hotfix/[nombre]`** - Fixes urgentes en producción
- **`release/v[X.X.X]`** - Preparación de releases

## Flujo de Trabajo

### Para Features Nuevas
```
1. Crear branch desde develop:
   git checkout develop
   git pull origin develop
   git checkout -b feature/nombre-descriptivo

2. Desarrollar y commitear cambios:
   git add .
   git commit -m "feat: descripción del feature"

3. Push y crear PR a develop:
   git push origin feature/nombre-descriptivo
   
4. Después de merge, eliminar branch local y remoto:
   git branch -d feature/nombre-descriptivo
   git push origin --delete feature/nombre-descriptivo
```

### Para Bugfixes
```
1. Crear branch desde develop:
   git checkout develop
   git pull origin develop
   git checkout -b bugfix/nombre-bug

2. Desarrollar fix y commitear:
   git add .
   git commit -m "fix: descripción del fix"

3. Push y crear PR a develop:
   git push origin bugfix/nombre-bug
```

### Deploy a Staging (Testing)
```
1. Cuando develop está listo para testing:
   - Crear PR de develop ? staging
   - Requiere aprobación de 1 reviewer
   - CI/CD debe pasar completamente
   
2. Al hacer merge:
   - Railway deploya automáticamente
   - Realizar testing manual completo
```

### Deploy a Producción
```
1. Crear release branch desde staging:
   git checkout staging
   git pull origin staging
   git checkout -b release/v1.2.3

2. Actualizar versión y changelog:
   - Actualizar versión en archivos .csproj
   - Actualizar CHANGELOG.md
   
3. Crear PR de release/v1.2.3 ? main:
   - Requiere aprobación de 1+ reviewers
   - CI/CD completo debe pasar
   
4. Después del merge:
   - GitHub Actions deploya a producción
   - Crear tag de versión:
     git tag v1.2.3
     git push origin v1.2.3
   
5. Mergear cambios de vuelta a develop:
   git checkout develop
   git merge release/v1.2.3
   git push origin develop
```

### Hotfixes Urgentes
```
1. Crear branch desde main:
   git checkout main
   git pull origin main
   git checkout -b hotfix/nombre-critico

2. Desarrollar fix urgente:
   git add .
   git commit -m "hotfix: descripción del fix crítico"

3. Crear PR a main (fast-track):
   - Requiere aprobación urgente
   - CI/CD debe pasar
   
4. Después del merge a main:
   - Mergear hotfix también a develop y staging:
     git checkout develop
     git merge hotfix/nombre-critico
     git push origin develop
     
     git checkout staging
     git merge hotfix/nombre-critico
     git push origin staging
```

## Políticas de Protección de Branches

### Branch: `main`
- ? Require pull request before merging
- ? Require approvals: 1
- ? Require status checks to pass before merging
  - build-and-test
  - security-scan (si se configura)
- ? Require branches to be up to date before merging
- ? Do not allow bypassing the above settings
- ? Restrict who can push to matching branches (solo admins)

### Branch: `staging`
- ? Require pull request before merging
- ? Require approvals: 1
- ? Require status checks to pass before merging
  - build-and-test
- ? Require branches to be up to date before merging
- ?? Allow force pushes (solo para admins, casos extremos)

### Branch: `develop`
- ? Require pull request before merging
- ?? Require approvals: 0 (opcional para agilidad)
- ? Require status checks to pass before merging
  - build-and-test
- ?? Allow administrators to bypass pull request requirements (para urgencias)

## CI/CD por Branch

### `feature/*` y `bugfix/*`
```yaml
on:
  push:
    branches:
      - 'feature/**'
      - 'bugfix/**'

jobs:
  - Build
  - Run Tests
  - Code Quality Check
```

### `develop`
```yaml
on:
  push:
    branches:
      - develop

jobs:
  - Build
  - Run Tests
  - Code Quality Check
  - [OPCIONAL] Deploy to Dev Environment
```

### `staging`
```yaml
on:
  push:
    branches:
      - staging

jobs:
  - Build
  - Run Tests
  - Security Scan
  - Deploy to Railway Staging
  - Run Integration Tests
```

### `main`
```yaml
on:
  push:
    branches:
      - main

jobs:
  - Build
  - Run Tests
  - Security Scan
  - Deploy to Production
  - Create Release Notes
  - Notify Team
```

## Convenciones de Nombres

### Branches
- `feature/add-payment-gateway`
- `feature/improve-search-performance`
- `bugfix/fix-login-redirect`
- `bugfix/correct-listing-validation`
- `hotfix/fix-critical-security-issue`
- `release/v1.2.3`

### Commits (Conventional Commits)
```
feat: add payment gateway integration
fix: correct listing image upload validation
refactor: improve database query performance
docs: update API documentation
test: add unit tests for listing service
chore: update dependencies
hotfix: patch critical security vulnerability
```

## Escenarios Comunes

### Scenario 1: Nueva Feature
```bash
# Desde develop
git checkout -b feature/add-listing-filters
# ... develop ...
git push origin feature/add-listing-filters
# Create PR ? develop
# Merge PR
```

### Scenario 2: Deploy a Staging
```bash
# Cuando develop está estable
# Create PR: develop ? staging en GitHub
# Revisar, aprobar y merge
# Railway deploya automáticamente
```

### Scenario 3: Deploy a Producción
```bash
# Crear release
git checkout staging
git checkout -b release/v1.5.0

# Actualizar versión
# Update CHANGELOG.md

git push origin release/v1.5.0
# Create PR: release/v1.5.0 ? main
# Aprobar y merge
# GitHub Actions deploya a producción
# Create tag v1.5.0

# Sync back to develop
git checkout develop
git merge release/v1.5.0
git push origin develop
```

## Comandos útiles

### Ver branches locales y remotos
```bash
git branch -a
```

### Limpiar branches eliminados del remoto
```bash
git fetch --prune
```

### Ver branches mergeados a develop
```bash
git branch --merged develop
```

### Eliminar branches locales mergeados
```bash
git branch --merged develop | grep -v "develop" | xargs git branch -d
```

## Configurar Protección de Branches en GitHub

1. Ve a tu repositorio en GitHub
2. Settings ? Branches ? Branch protection rules
3. Add rule para `main`, `staging`, `develop`
4. Configura según las políticas arriba especificadas

## Notas Importantes

- **NUNCA** hacer push directo a `main`, `staging`, o `develop`
- **SIEMPRE** usar PRs para mergear
- **SIEMPRE** esperar a que CI/CD pase antes de mergear
- **ELIMINAR** branches de features después de merge
- **MANTENER** `develop` actualizado regularmente
- **TESTING** completo en staging antes de producción
