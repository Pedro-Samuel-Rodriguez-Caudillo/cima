# CIMA - Documentación

Sistema de gestión inmobiliaria desarrollado con ABP Framework y Blazor WebAssembly.

## Estructura de Documentación

### ?? [01-getting-started/](01-getting-started/)
Guías de inicio rápido y configuración inicial
- [README.md](01-getting-started/README.md) - Inicio aquí
- [quick-reference.md](01-getting-started/quick-reference.md) - Referencia rápida
- [setup-inicial.md](01-getting-started/setup-inicial.md) - Configuración inicial del proyecto

### ??? [02-architecture/](02-architecture/)
Arquitectura del sistema y especificaciones técnicas
- [technical-specs.md](02-architecture/technical-specs.md) - Especificaciones técnicas

### ?? [03-development/](03-development/)
Guías de desarrollo y historia de sprints

#### Guías
- [guides/migrations.md](03-development/guides/migrations.md) - Migraciones de Entity Framework
- [guides/tailwind.md](03-development/guides/tailwind.md) - Configuración de Tailwind CSS
- [guides/logging.md](03-development/guides/logging.md) - Sistema de logging
- [guides/css-classes.md](03-development/guides/css-classes.md) - Guía de clases CSS

#### Sprints
- [sprints/sprint-01/](03-development/sprints/sprint-01/) - Sprint 1: Configuración inicial y CRUD básico
- [sprints/sprint-02/](03-development/sprints/sprint-02/) - Sprint 2: Sitio público y mejoras UX

### ?? [04-deployment/](04-deployment/)
Configuración de despliegue y CI/CD
- [branching-strategy.md](04-deployment/branching-strategy.md) - Estrategia de branching Git
- Railway, GitHub Actions y configuración de ambientes

### ?? [05-troubleshooting/](05-troubleshooting/)
Solución de problemas y fixes documentados
- Correcciones de API
- Fixes de migraciones
- Soluciones de health checks

### ?? [archive/](archive/)
Documentación histórica archivada

## Inicio Rápido

1. **Primera vez en el proyecto?** ? [01-getting-started/README.md](01-getting-started/README.md)
2. **Referencia rápida de comandos?** ? [01-getting-started/quick-reference.md](01-getting-started/quick-reference.md)
3. **Configurar ambiente de desarrollo?** ? [01-getting-started/setup-inicial.md](01-getting-started/setup-inicial.md)
4. **Trabajar con branches?** ? [04-deployment/branching-strategy.md](04-deployment/branching-strategy.md)

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

## Flujo de Trabajo (Ver detalle completo en [04-deployment/branching-strategy.md](04-deployment/branching-strategy.md))

### Desarrollar Nueva Feature
```bash
git checkout develop
git pull origin develop
git checkout -b feature/nombre-feature
# ... desarrollar ...
git add .
git commit -m "feat: descripción"
git push origin feature/nombre-feature
# Crear PR en GitHub ? develop
```

### Deploy a Staging
```bash
# Crear PR: develop ? staging en GitHub
# Aprobar y merge
# Railway deploya automáticamente
```

### Deploy a Producción
```bash
git checkout staging
git checkout -b release/v1.2.3
# Actualizar versión y changelog
git push origin release/v1.2.3
# Crear PR: release/v1.2.3 ? main
# GitHub Actions deploya a producción
```

---

**Última actualización:** 2024
