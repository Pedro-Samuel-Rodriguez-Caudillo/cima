# ESTRATEGIA DE BRANCHING Y DEPLOYMENT

## ?? ESTRUCTURA DE RAMAS

### Ramas Principales

```
master (protected)
?? Producción
?? Deploy automático a servidor producción
?? Solo merge desde develop con PR
?? Tags para releases (v1.0.0, v1.1.0, etc.)

develop (protected)
?? Staging/Pre-producción
?? Deploy automático a Railway
?? Merge desde feature/* branches
?? Testing y QA

feature/* (temporal)
?? Desarrollo de nuevas features
?? Branch desde develop
?? Merge a develop con PR
?? Eliminar después de merge
```

---

## ?? FLUJO DE TRABAJO

### 1. Desarrollo de Nueva Feature

```bash
# Desde develop, crear rama de feature
git checkout develop
git pull origin develop
git checkout -b feature/nombre-de-la-feature

# Trabajar en la feature
git add .
git commit -m "feat(scope): descripción"

# Push a GitHub
git push origin feature/nombre-de-la-feature

# Crear Pull Request en GitHub:
# feature/nombre-de-la-feature ? develop
```

### 2. Merge a Staging (develop)

```bash
# Después de aprobar PR
git checkout develop
git pull origin develop

# Railway detecta cambios automáticamente
# Deploy a: https://cima-staging.up.railway.app
```

### 3. Release a Producción (master)

```bash
# Cuando develop está estable
git checkout master
git pull origin master
git merge develop
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin master --tags

# GitHub Actions hace deploy a producción
```

---

## ?? CONVENCIONES DE NOMBRES

### Ramas

```
master              # Producción
develop             # Staging
feature/crud-listings    # Nueva feature
feature/fix-login       # Fix en feature
hotfix/security-patch   # Fix urgente en producción
release/v1.0.0         # Preparación de release
```

### Commits (Conventional Commits)

```
feat(listings): agregar filtro por precio
fix(auth): corregir logout en Safari
docs(readme): actualizar guía de instalación
style(css): mejorar responsividad en mobile
refactor(api): simplificar validación
test(listings): agregar tests unitarios
chore(deps): actualizar dependencias
ci(railway): configurar deploy automático
```

---

## ?? CONFIGURACIÓN RAILWAY

### Ambientes

Railway detecta automáticamente las ramas:

```
Rama develop ? Ambiente "staging"
  ?? URL: https://cima-staging.up.railway.app
  ?? PostgreSQL: cima-staging
  ?? Variables: .env.staging

Rama master ? Ambiente "production"
  ?? URL: https://cima.com (cuando tengas dominio)
  ?? PostgreSQL: cima-production
  ?? Variables: .env.production
```

### Variables por Ambiente

**Staging (develop):**
```bash
ASPNETCORE_ENVIRONMENT=Staging
DATABASE_NAME=cima_staging
APP_SELF_URL=https://cima-staging.up.railway.app
```

**Production (master):**
```bash
ASPNETCORE_ENVIRONMENT=Production
DATABASE_NAME=cima_production
APP_SELF_URL=https://cima.com
```

---

## ??? PROTECCIÓN DE RAMAS

### En GitHub Settings > Branches

**master:**
- ?? Require pull request reviews (1 aprobación mínimo)
- ?? Require status checks to pass (CI debe pasar)
- ?? Require branches to be up to date
- ?? Include administrators
- ?? Restrict who can push (solo maintainers)

**develop:**
- ?? Require pull request reviews (opcional para equipo pequeño)
- ?? Require status checks to pass (CI debe pasar)
- ?? Require branches to be up to date

---

## ?? WORKFLOW TÍPICO

### Día a Día

```bash
# 1. Nueva feature
git checkout develop
git pull origin develop
git checkout -b feature/nueva-funcionalidad

# 2. Desarrollo
git add .
git commit -m "feat(scope): nueva funcionalidad"
git push origin feature/nueva-funcionalidad

# 3. Pull Request en GitHub
# feature/nueva-funcionalidad ? develop
# Esperar CI pase ?
# Aprobar PR
# Merge

# 4. Railway deploy automático a staging
# Probar en: https://cima-staging.up.railway.app

# 5. Si todo OK, crear release
git checkout master
git merge develop
git tag v1.0.1
git push origin master --tags

# 6. GitHub Actions deploy a producción
```

---

## ?? EJEMPLO VISUAL

```
Time ?????????????????????????????????????????????>

feature/login-fix
  ?
  ?? commit: fix login
  ?
  ?? PR ? develop
           ?
           ?? CI ?
           ?? Merge
           ?? Railway deploy ? staging
                                ?
                                ?? Test en staging
                                ?
                                ?? PR ? master
                                         ?
                                         ?? CI ?
                                         ?? Tag v1.0.1
                                         ?? Merge
                                         ?? GitHub Actions ? production
```

---

## ?? HOTFIX (Producción Rota)

```bash
# Si hay bug crítico en producción
git checkout master
git pull origin master
git checkout -b hotfix/critical-bug

# Fix rápido
git add .
git commit -m "hotfix(auth): fix critical security issue"
git push origin hotfix/critical-bug

# PR a master (bypass algunas reglas si es crítico)
# Merge a master
git tag v1.0.2
git push origin master --tags

# Después, merge master a develop para sincronizar
git checkout develop
git merge master
git push origin develop
```

---

## ? CHECKLIST SETUP INICIAL

### Ramas

- [x] `develop` creada desde `master`
- [ ] `develop` pushed a GitHub
- [ ] Protección configurada en GitHub para `master`
- [ ] Protección configurada en GitHub para `develop`

### Railway

- [ ] Proyecto conectado a GitHub
- [ ] PostgreSQL agregado
- [ ] Variables configuradas para staging (develop)
- [ ] Deploy automático funcionando

### CI/CD

- [ ] GitHub Actions configurado para `master`
- [ ] GitHub Actions configurado para `develop`
- [ ] Railway webhook configurado (automático)

---

## ?? COMANDOS ÚTILES

```bash
# Ver ramas
git branch -a

# Cambiar de rama
git checkout develop
git checkout master

# Actualizar rama local
git pull origin develop

# Ver estado
git status

# Ver log
git log --oneline --graph --all

# Limpiar ramas locales eliminadas en remote
git fetch --prune

# Ver diferencias entre ramas
git diff master..develop

# Ver archivos modificados en rama
git diff --name-only master..develop
```

---

## ?? MEJORES PRÁCTICAS

### Commits

? **BUENO:**
```bash
git commit -m "feat(listings): add price filter to search"
git commit -m "fix(auth): resolve logout redirect issue"
git commit -m "docs(readme): update Railway setup guide"
```

? **MALO:**
```bash
git commit -m "changes"
git commit -m "fix stuff"
git commit -m "wip"
```

### Pull Requests

? **BUENO:**
- Título descriptivo: "feat(listings): Add advanced search filters"
- Descripción completa de cambios
- Screenshots si hay cambios UI
- Tests pasando
- Code review solicitado

? **MALO:**
- Título: "Updates"
- Sin descripción
- Tests fallando
- Self-merge sin review

### Ramas

? **BUENO:**
```
feature/user-authentication
feature/listing-crud
feature/image-upload
hotfix/sql-injection
```

? **MALO:**
```
test
dev
my-branch
changes
```

---

## ?? RECURSOS

- **Conventional Commits:** https://www.conventionalcommits.org/
- **Git Flow:** https://nvie.com/posts/a-successful-git-branching-model/
- **Railway Docs:** https://docs.railway.app/
- **GitHub Flow:** https://docs.github.com/en/get-started/quickstart/github-flow

---

**ESTADO ACTUAL:**
- ? Rama `develop` creada
- ? `railway.json` configurado
- ? `Procfile` configurado
- ? Pendiente: Push a GitHub
- ? Pendiente: Configurar Railway
