# CIMA - Plataforma Inmobiliaria

Sistema de gestión inmobiliaria desarrollado con ABP Framework y Blazor WebAssembly.

## 📚 Documentación

- **[docs/INDEX.md](docs/INDEX.md)** - Punto de entrada principal de la documentación
- **[.github/BRANCHING_STRATEGY.md](.github/BRANCHING_STRATEGY.md)** - Estrategia de branching y flujo de trabajo
- **[agents/README.md](agents/README.md)** - Guía para agentes IA (Copilot, Gemini, Codex)

### Inicio Rápido

1. **Primera vez en el proyecto?** → [docs/01-getting-started/README.md](docs/01-getting-started/README.md)
2. **Desarrollar nueva feature?** → [.github/BRANCHING_STRATEGY.md](.github/BRANCHING_STRATEGY.md)
3. **Configurar ambiente?** → [docs/01-getting-started/setup-inicial.md](docs/01-getting-started/setup-inicial.md)

## About this solution

This is a layered startup solution based on [Domain Driven Design (DDD)](https://abp.io/docs/latest/framework/architecture/domain-driven-design) practises. All the fundamental ABP modules are already installed. Check the [Application Startup Template](https://abp.io/docs/latest/solution-templates/layered-web-application) documentation for more info.

## Stack Tecnológico

- **.NET 9** - Framework principal
- **ABP Framework 9.0** - Application framework
- **Blazor WebAssembly** - Frontend interactivo
- **Entity Framework Core** - ORM
- **PostgreSQL** - Base de datos
- **Tailwind CSS** - Estilos
- **Docker** - Containerización
- **Railway** - Hosting staging
- **GitHub Actions** - CI/CD

### Pre-requirements

* [.NET9.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)
* [PostgreSQL 14+](https://www.postgresql.org/download/)

### Configurations

The solution comes with a default configuration that works out of the box. However, you may consider to change the following configuration before running your solution:

### Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies. This step is automatically done when you create a new solution, if you didn't especially disabled it. However, you should run it yourself if you have first cloned this solution from your source control, or added a new client-side package dependency to your solution.
* Run `cima.DbMigrator` to create the initial database. This step is also automatically done when you create a new solution, if you didn't especially disabled it. This should be done in the first run. It is also needed if a new database migration is added to the solution later.

## Flujo de Trabajo Git

Seguimos Git Flow simplificado. Ver [.github/BRANCHING_STRATEGY.md](.github/BRANCHING_STRATEGY.md) para detalles completos.

### Desarrollar Nueva Feature
```bash
git checkout develop
git pull origin develop
git checkout -b feature/nombre-feature
# ... desarrollar ...
git add .
git commit -m "feat: descripción"
git push origin feature/nombre-feature
# Crear PR en GitHub → develop
```

### Deploy a Staging
```bash
# Crear PR: develop → staging en GitHub
# Aprobar y mergear
# Railway deploya automáticamente
```

### Deploy a Producción
```bash
git checkout staging
git checkout -b release/v1.2.3
# Actualizar versión y changelog
git push origin release/v1.2.3
# Crear PR: release/v1.2.3 → main
# GitHub Actions deploya a producción
```

## Comandos Útiles

Ver [docs/01-getting-started/quick-reference.md](docs/01-getting-started/quick-reference.md) para lista completa.

### Desarrollo Local
```bash
.\etc\scripts\start-dev-environment.ps1   # Iniciar ambiente
.\etc\scripts\start-tailwind-watch.ps1    # Watch Tailwind CSS
```

### Base de Datos
```bash
.\etc\scripts\actualizar-migraciones.ps1  # Crear migraciones
.\etc\scripts\reset-database.ps1          # Reset database
.\etc\scripts\seed-dev-data.ps1           # Seed datos de desarrollo
```

### Testing
```bash
.\etc\scripts\test-api.ps1                # Probar API
.\etc\scripts\verificar-datos.ps1         # Verificar datos
```

## Contribuir

1. Lee [.github/BRANCHING_STRATEGY.md](.github/BRANCHING_STRATEGY.md)
2. Sigue [agents/COMMIT_CONVENTIONS.md](agents/COMMIT_CONVENTIONS.md)
3. Crea PR y espera a que CI pase
4. No uses emojis en código/logs/comentarios

## Soporte

- **Documentación:** [docs/INDEX.md](docs/INDEX.md)
- **Issues:** [GitHub Issues](https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/issues)
- **ABP Docs:** [docs.abp.io](https://docs.abp.io)

## Documentacion reorganizada

- **[docs/DOCUMENTATION_CATALOG.md](docs/DOCUMENTATION_CATALOG.md)** - catalogo actualizado de los puntos de entrada activos, sprint y notas archivadas.
- **[docs/archive/root-notes/](docs/archive/root-notes/)** - los resumenes, checklists y reportes historicos movidos desde la raiz para mantener limpio el directorio principal.
- **[docs/03-development/sprints/](docs/03-development/sprints/)** - sprint 1-3 concentran los dias activos y los resumenes DIA_9 a DIA_12 dentro de sprint-03.
- **[docs/archive/sprints/](docs/archive/sprints/)** - archivos extra consolidados por sprint y dia (un archivo por dia) que reemplazan las carpetas antiguas de day-logs.


---

**Versión:** 2.0  
**Última actualización:** 2024
