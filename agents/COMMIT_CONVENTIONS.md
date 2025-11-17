# ?? CONVENCIONES DE COMMITS - PROYECTO CIMA

**Formato Semántico en Español**

Todos los commits del proyecto CIMA siguen el formato semántico de commits con mensajes en **español**.

---

## ?? ESTRUCTURA

```
<type>(<scope>): <subject>

<body>

<footer>
```

- **type:** Tipo de cambio (feat, fix, refactor, docs, etc.)
- **scope:** Área del código (opcional pero recomendado)
- **subject:** Descripción breve (imperativo, minúsculas)
- **body:** Descripción detallada (opcional, 72 caracteres por línea)
- **footer:** Referencias a issues (opcional)

---

## ??? TYPES

| Type | Descripción | Ejemplo |
|------|-----------|---------|
| **feat** | Nueva funcionalidad | `feat(domain): agregar entidad ContactRequest` |
| **fix** | Bug fix | `fix(httpapi): corregir autorización en endpoint` |
| **refactor** | Refactorización (sin cambio funcional) | `refactor(application): optimizar queries LINQ` |
| **docs** | Cambios en documentación | `docs(architecture): actualizar diagrama EER` |
| **test** | Agregar/actualizar tests | `test(application): tests para PropertyAppService` |
| **chore** | Cambios en build, deps, setup | `chore(devops): actualizar versión Docker base` |
| **style** | Formato, convenciones (sin lógica) | `style(code): formatear PropertyService.cs` |
| **perf** | Optimización de performance | `perf(efcore): agregar índice a tabla Properties` |
| **ci** | Cambios en CI/CD | `ci: actualizar GitHub Actions workflow` |

---

## ?? SCOPES RECOMENDADOS

```
domain              ? Cambios en src/cima.Domain (entidades, agregados)
domain-shared       ? Cambios en src/cima.Domain.Shared (DTOs, enums, constantes)
application         ? Cambios en src/cima.Application (servicios, validadores)
application-contracts ? Cambios en src/cima.Application.Contracts (interfaces)
efcore              ? Cambios en src/cima.EntityFrameworkCore (ORM, migrations)
httpapi             ? Cambios en src/cima.HttpApi (controllers)
httpapi-client      ? Cambios en src/cima.HttpApi.Client
blazor              ? Cambios en src/cima.Blazor (SSR, páginas, componentes server)
blazor-client       ? Cambios en src/cima.Blazor.Client (WASM, interactividad)
dbmigrator          ? Cambios en src/cima.DbMigrator
database            ? Cambios en esquema, migraciones
docker              ? Cambios en Dockerfile
docker-compose      ? Cambios en docker-compose.yml
ci-cd               ? Cambios en .github/workflows
infrastructure      ? Scripts, deployment, configuración
docs                ? Documentación (README, guías técnicas, etc.)
```

---

## ?? EJEMPLOS POR AGENTE

### **Copilot (Backend)**

```
feat(domain): agregar agregado ContactRequest con validaciones
feat(application): implementar ContactRequestAppService CRUD
feat(httpapi): crear endpoints para contact requests
fix(application): corregir validación de email en PropertyService
refactor(efcore): optimizar queries con Include() para lazy loading
perf(database): agregar índice a tabla ContactRequests (Status, CreatedAt)
docs(architecture): actualizar diagrama de entidades
test(application): agregar tests para PropertyAppService

# Commit real
commit -m "feat(domain): agregar ContactRequest con estados New/Replied/Closed"
commit -m "feat(application): ContactRequestAppService con autorización granular"
commit -m "feat(httpapi): endpoints POST /api/contact-requests (público) y GET (admin)"
```

### **Gemini (Frontend)**

```
feat(blazor): crear página de catálogo con filtros y paginación
feat(blazor): componente PropertyCard responsive
feat(blazor-client): galería de imágenes interactiva con carousel
fix(blazor): corregir responsividad en tablets
style(blazor): aplicar Tailwind CSS a formularios
a11y(blazor): agregar ARIA labels y alt text a imágenes
perf(blazor): lazy loading de imágenes
docs(architecture): actualizar diagrama de rutas

# Commit real
commit -m "feat(blazor): página Properties/Index con filtros responsive"
commit -m "feat(shared): componente PropertyCard reutilizable"
commit -m "style(blazor): aplicar Tailwind a formulario de contacto"
commit -m "a11y: agregar aria-labels a botones y links"
```

### **Codex (DevOps)**

```
feat(docker): crear Dockerfile multi-stage optimizado
feat(docker-compose): configurar dev y prod compose files
feat(ci-cd): GitHub Actions workflow para CI
feat(ci-cd): GitHub Actions workflow para deploy staging/prod
fix(docker): reducir tamaño de imagen optimizando layers
chore(devops): actualizar PostgreSQL a 16-alpine
chore(infrastructure): scripts de backup y health-check
docs(deployment): guía de deploy a VPS

# Commit real
commit -m "feat(docker): Dockerfile multi-stage con health checks"
commit -m "feat(docker-compose): dev y prod configs con postgres"
commit -m "feat(ci): GitHub Actions workflow para build y test"
commit -m "chore(infrastructure): script backup-db.ps1 con rotación"
commit -m "fix(docker): reducir imagen base de 512MB a 180MB"
```

---

## ?? REGLAS

1. **Usa IMPERATIVO, no pasado:** ? "agregar validación" ? "agregó validación"
2. **Minúsculas en subject:** ? "feat(domain): agregar" ? "feat(domain): Agregar"
3. **Sin punto final en subject:** ? "agregar validación" ? "agregar validación."
4. **Scope entre paréntesis:** ? `feat(domain):` ? `feat domain:` ? `feat [domain]:`
5. **Subject máx 50 caracteres:** Mantén concisidad
6. **Body máx 72 caracteres por línea:** Si es necesario explicación detallada
7. **Issue references en footer:** `Closes #123` o `Fixes #456`

---

## ?? EJEMPLOS COMPLETOS

### **Ejemplo 1: Agregar Feature (Backend)**

```
feat(domain): agregar entidad ContactRequest

- Crea agregado ContactRequest como raíz
- Estados: New, Replied, Closed
- Métodos de dominio: MarkAsReplied(), Close()
- Relaciones: Property (FK), Architect (FK)
- Validaciones básicas en constructor

Closes #42
```

### **Ejemplo 2: Bug Fix**

```
fix(httpapi): corregir autorización en endpoint contact-requests

El endpoint POST /api/contact-requests requería [Authorize] pero debe ser
[AllowAnonymous] para permitir formularios públicos. Solo Reply requiere
[Authorize(CimaPermissions.ContactRequests.Reply)].

Fixes #87
```

### **Ejemplo 3: Refactorización**

```
refactor(application): extraer validación a FluentValidator

Mueve lógica de validación de PropertyAppService.CreateAsync() a
CreateUpdatePropertyDtoValidator implementando AbstractValidator.
Mejora testabilidad y reutilización.

No hay cambio funcional - solo reorganización.
```

### **Ejemplo 4: Feature Frontend**

```
feat(blazor): galería de imágenes responsive con lightbox

- Componente PropertyImageGallery.razor reutilizable
- Soporte para múltiples imágenes con indicador
- Lightbox modal para vista completa
- Lazy loading de imágenes
- Mobile: swipe, Desktop: arrows y dots
- Alt text y accesibilidad WCAG AA

Closes #101
```

### **Ejemplo 5: DevOps**

```
feat(ci-cd): GitHub Actions workflow para deploy a staging

- Trigger: push a rama develop
- Steps: build ? test ? docker build ? push ? SSH deploy
- Healthcheck post-deploy
- Notificación Slack
- Rollback automático si falla healthcheck

Closes #156
```

### **Ejemplo 6: Documentación**

```
docs(architecture): actualizar diagrama de agregados y relaciones

- Agrega PropertyImage como Value Object
- Clarifica relaciones Many-to-One
- Documenta métodos de dominio por agregado
- Actualiza sección de RBAC

Ver ARQUITECTURA_TECNICA.md
```

---

## ?? WORKFLOW CON GIT

```bash
# 1. Crear rama feature
git checkout -b feat/contact-requests

# 2. Hacer cambios
# ... edita archivos ...

# 3. Stage changes
git add src/cima.Domain/Entities/ContactRequest.cs
git add src/cima.Application/Services/ContactRequestAppService.cs

# 4. Commit con mensaje semántico
git commit -m "feat(domain): agregar ContactRequest como agregado
  
- Estados: New, Replied, Closed
- Métodos de dominio para cambios de estado
- Relaciones a Property y Architect"

# 5. Push a rama
git push origin feat/contact-requests

# 6. PR con título: "feat(domain): agregar ContactRequest"

# 7. Merge a main/develop
git checkout main
git pull origin main
git merge --no-ff feat/contact-requests -m "feat(domain): agregar ContactRequest"
git push origin main
```

---

## ? MALOS EJEMPLOS

```
? git commit -m "WIP"
? git commit -m "feat(domain): agregar ContactRequest (WIP)"

? git commit -m "Fixed stuff"
? git commit -m "fix(application): corregir validación de email"

? git commit -m "Agregué validaciones, cambié UI, actualicé BD"
? Múltiples commits:
   1. feat(domain): agregar validaciones
   2. feat(blazor): actualizar UI
   3. chore(database): actualizar schema

? git commit -m "feat(domain) Agregar ContactRequest."
? git commit -m "feat(domain): agregar ContactRequest"

? git commit -m "feat: agregué un montón de cosas en todo el proyecto"
? git commit -m "feat(domain): agregar ContactRequest"
```

---

## ?? INTEGRACIÓN CON GITHUB

Los commits semánticos se integran con:
- **GitHub Releases:** Genera changelogs automáticos
- **Semantic Versioning:** Calcula next version (major.minor.patch)
- **Git history:** Fácil de navegar y entender cambios

---

## ?? AUTOMACIÓN (Opcional - Fase 2+)

Con **Husky + commitlint** puedes validar commits automáticamente:

```bash
npm install husky commitlint --save-dev
npx husky install
npx commitlint --init
```

---

**Referencia:** https://www.conventionalcommits.org/  
**Última actualización:** Setup inicial  
**Versión:** 1.0

