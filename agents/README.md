# GUÍA PARA AGENTES IA - PROYECTO CIMA

## Propósito

Este documento establece las **convenciones, patrones y protocolos** para que los agentes IA trabajen de manera coordinada en el proyecto CIMA.

## Estructura del Proyecto Actualizada

```
cima/
??? src/                          # Código fuente
?   ??? cima.Domain               # Entidades, agregados (DDD)
?   ??? cima.Application          # Servicios de aplicación
?   ??? cima.Blazor               # Web App Server (SSR)
?   ??? cima.Blazor.Client        # WASM interactivo
??? docs/                         # Documentación
?   ??? INDEX.md                  # Punto de entrada principal
?   ??? 01-getting-started/       # Guías de inicio
?   ??? 02-architecture/          # Arquitectura técnica
?   ??? 03-development/           # Guías de desarrollo
?   ?   ??? guides/               # Guías específicas
?   ?   ??? sprints/              # Historia de sprints
?   ??? 04-deployment/            # DevOps y CI/CD
?   ??? 05-troubleshooting/       # Solución de problemas
??? .github/                      # GitHub Actions y configuración
?   ??? BRANCHING_STRATEGY.md     # Estrategia de branching
?   ??? SETUP_BRANCH_PROTECTION.md
?   ??? WORKFLOW_VISUAL.md
??? agents/                       # Guías para agentes IA
?   ??? AGENTS.md                 # Este archivo
?   ??? AGENTS_COPILOT.md         # Guía para Copilot (backend)
?   ??? AGENTS_GEMINI.md          # Guía para Gemini (frontend)
?   ??? AGENTS_CODEX.md           # Guía para Codex (DevOps)
?   ??? COMMIT_CONVENTIONS.md     # Convenciones de commits
??? etc/scripts/                  # Scripts de automatización

```

## Dominios de Responsabilidad

| Agente | Especialidad | Scope | Archivos Principales |
|--------|-------------|-------|---------------------|
| **Copilot** | Backend, DDD, EF Core, API REST | C#, Domain, Services, Controllers | `agents/AGENTS_COPILOT.md` |
| **Gemini** | Frontend, Blazor, UX, CSS | Razor, Blazor components, CSS | `agents/AGENTS_GEMINI.md` |
| **Codex** | DevOps, Docker, CI/CD | PowerShell, YAML, Docker | `agents/AGENTS_CODEX.md` |

## Documentación Clave

### Para Todos los Agentes
- **`docs/INDEX.md`** - Punto de entrada principal de la documentación
- **`agents/COMMIT_CONVENTIONS.md`** - Convenciones de commits semánticos
- **`.github/BRANCHING_STRATEGY.md`** - Estrategia de branching y flujo de trabajo

### Para Copilot (Backend)
- **`agents/AGENTS_COPILOT.md`** - Guía completa de backend
- **`docs/02-architecture/technical-specs.md`** - Especificaciones técnicas
- **`docs/03-development/guides/migrations.md`** - Guía de migraciones EF

### Para Gemini (Frontend)
- **`agents/AGENTS_GEMINI.md`** - Guía completa de frontend
- **`docs/03-development/guides/tailwind.md`** - Configuración Tailwind CSS
- **`docs/03-development/guides/css-classes.md`** - Clases CSS del proyecto

### Para Codex (DevOps)
- **`agents/AGENTS_CODEX.md`** - Guía completa de DevOps
- **`docs/04-deployment/`** - Documentación de deployment
- **`.github/workflows/`** - Workflows de CI/CD

## Convenciones Generales

### Commits
Seguir convenciones semánticas definidas en `agents/COMMIT_CONVENTIONS.md`:
```
feat(scope): descripción
fix(scope): descripción
refactor(scope): descripción
docs(scope): descripción
```

### Branching
Seguir estrategia definida en `.github/BRANCHING_STRATEGY.md`:
- `main` ? Producción
- `staging` ? Pre-producción (Railway)
- `develop` ? Integración de features
- `feature/*` ? Nuevas funcionalidades
- `bugfix/*` ? Correcciones
- `hotfix/*` ? Fixes urgentes

### Reglas Críticas
1. **NO emojis en código, logs, comentarios** (solo en Markdown)
2. **Usar PR para mergear** a develop, staging, main
3. **Esperar a que CI pase** antes de mergear
4. **Eliminar branches** de features después de merge

## Flujo de Trabajo

### Desarrollar Nueva Feature
```bash
git checkout develop
git pull origin develop
git checkout -b feature/nombre-descriptivo

# Desarrollar...

git add .
git commit -m "feat: descripción del feature"
git push origin feature/nombre-descriptivo

# Crear PR en GitHub ? develop
```

### Deploy a Staging
```bash
# Crear PR en GitHub: develop ? staging
# Aprobar y mergear
# Railway deploya automáticamente
```

### Deploy a Producción
```bash
git checkout staging
git checkout -b release/v1.2.3

# Actualizar versión y changelog

git push origin release/v1.2.3
# Crear PR: release/v1.2.3 ? main
# Aprobar y mergear
# GitHub Actions deploya a producción
```

## Recursos Importantes

### Guías de Inicio
- `docs/01-getting-started/README.md` - Inicio aquí
- `docs/01-getting-started/quick-reference.md` - Referencia rápida

### Desarrollo
- `docs/03-development/guides/` - Guías específicas (migrations, tailwind, logging)
- `docs/03-development/sprints/` - Historia de desarrollo por sprints

### Deployment
- `docs/04-deployment/` - Configuración de Railway, GitHub Actions
- `.github/BRANCHING_STRATEGY.md` - Estrategia completa de branching

### Troubleshooting
- `docs/05-troubleshooting/` - Soluciones a problemas comunes

## Checklist Pre-Commit

- [ ] Código sigue convenciones del proyecto
- [ ] No hay emojis en código/logs/comentarios
- [ ] Tests pasan (si aplica)
- [ ] Commit message sigue formato semántico
- [ ] Branch correcto (feature/*, bugfix/*, etc.)
- [ ] Documentación actualizada si es necesario

## Coordinación Entre Agentes

### Copilot + Gemini
- Copilot crea DTOs y servicios backend
- Gemini consume esos DTOs en componentes Blazor
- Comunicación vía interfaces en `Application.Contracts`

### Copilot + Codex
- Copilot asegura API health checks funcionando
- Codex configura Docker y CI/CD para deployar
- Comunicación vía configuración en `appsettings.json`

### Gemini + Codex
- Gemini construye assets (CSS, JS)
- Codex asegura que build incluya esos assets
- Comunicación vía `Dockerfile` y build scripts

## Antipatrones a Evitar

### Todos los Agentes
- ? Push directo a main, staging, develop
- ? Commits sin mensaje descriptivo
- ? Cambios sin PR
- ? Emojis en código/logs/comentarios
- ? Hardcodear valores sensibles

### Copilot (Backend)
- ? Lógica de negocio en controllers
- ? DTOs con lógica
- ? Queries directos en services (usar repositories)

### Gemini (Frontend)
- ? Estilos inline
- ? Componentes gigantes (>300 líneas)
- ? JavaScript en Razor (usar C# cuando sea posible)

### Codex (DevOps)
- ? Secretos en archivos versionados
- ? Containers con root user
- ? Builds sin cache

## Soporte

- **Documentación:** `docs/INDEX.md`
- **Issues:** GitHub Issues del proyecto
- **Estrategia de branching:** `.github/BRANCHING_STRATEGY.md`
- **Workflow visual:** `.github/WORKFLOW_VISUAL.md`

---

**Última actualización:** Reorganización de documentación completada
**Versión:** 2.0
