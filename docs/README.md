# ?? DOCUMENTACIÓN - PLATAFORMA INMOBILIARIA CIMA

## ?? Bienvenida

Esta carpeta `/docs` contiene toda la documentación técnica y operacional para **planear, ejecutar y mantener** la plataforma inmobiliaria CIMA.

---

## ?? DOCUMENTOS DISPONIBLES

### **1. ?? PLAN_2_SEMANAS.md** (Empezar aquí)
- **Propósito:** Plan maestro del proyecto (10 días laborales)
- **Contenido:**
  - Visión y scope funcional completo
  - Arquitectura técnica (stack elegido)
  - Timeline detallado por día
  - Entregables finales esperados
  - Backlog de fases futuras
- **Cuándo leer:** Antes de iniciar (lunes mañana)
- **Audiencia:** Product Managers, Stakeholders, Desarrollador

### **2. ?? DIA_1_GUIA_EJECUTIVA.md** (Guía del Lunes)
- **Propósito:** Instrucciones paso-a-paso para el primer día
- **Contenido:**
  - Timeline detallado (08:00 - 17:00)
  - 6 tareas específicas con código
  - Checklist de validación
  - Troubleshooting común
- **Cuándo usar:** Lunes de 08:00 a 17:00
- **Audiencia:** Desarrollador principal
- **Resultado esperado:** BD lista, modelo de dominio implementado

### **3. ??? ARQUITECTURA_TECNICA.md** (Referencia Técnica)
- **Propósito:** Documentación detallada de la arquitectura
- **Contenido:**
  - Diagrama de capas (frontend, backend, BD)
  - Estructura de proyectos
  - Modelo de dominio (DDD)
  - Flujos principales (catálogo, admin, contacto)
  - RBAC y autorización
  - Endpoints API
  - Esquema de BD (SQL)
  - Convenciones de código
- **Cuándo consultar:** Durante desarrollo (referencia permanente)
- **Audiencia:** Desarrollador, code reviews, onboarding nuevos devs

### **4. ?? AGENTS.md** (Guía para Agentes IA)
- **Propósito:** Convenciones y protocolos para Copilot, Gemini, Codex
- **Contenido:**
  - Dominios de responsabilidad por IA
  - Convenciones de commits (semántico en español)
  - Normas de PR y code quality
  - Patrones de colaboración
  - Archivos críticos (no modificar sin coordinación)
  - **REGLA: Sin emojis en código, logs, comentarios**
- **Cuándo consultar:** Al usar cualquier IA en el proyecto
- **Audiencia:** Copilot, Gemini, Codex, desarrolladores

### **5. ?? AGENTS_COPILOT.md** (Especialización Backend)
- **Propósito:** Guía dedicada para GitHub Copilot
- **Scope:** C#, .NET, Domain-Driven Design, EF Core, API REST
- **Contenido:**
  - Área de trabajo específica (cima.Domain, cima.Application, etc.)
  - Flujo de implementación (entidad ? DTO ? service ? controller)
  - Convenciones de código C#
  - Patrones DDD (agregados, repositories, value objects)
  - Antipatrones a evitar
  - Checklist pre-commit
- **Audiencia:** GitHub Copilot (backend specialist)

### **6. ?? AGENTS_GEMINI.md** (Especialización Frontend)
- **Propósito:** Guía dedicada para Google Gemini
- **Scope:** Blazor, Razor components, UX, CSS, Accesibilidad
- **Contenido:**
  - Área de trabajo específica (Pages, Shared, wwwroot)
  - Flujo de desarrollo UI (contenedor ? componentes ? estilos)
  - Convenciones Tailwind CSS
  - Accesibilidad (WCAG, ARIA, labels)
  - Componentes reutilizables
  - Antipatrones UI
  - Lighthouse audit checklist
- **Audiencia:** Google Gemini (frontend specialist)

### **7. ?? AGENTS_CODEX.md** (Especialización DevOps)
- **Propósito:** Guía dedicada para OpenAI Codex
- **Scope:** Docker, CI/CD, GitHub Actions, deployment, scripts
- **Contenido:**
  - Dockerfile multi-stage optimizado
  - docker-compose.yml (dev y prod)
  - GitHub Actions workflows (CI, CD staging, CD prod)
  - Scripts PowerShell/bash (backup, health-check, rollback)
  - Seguridad (secrets, non-root users, backups)
  - Troubleshooting común
- **Audiencia:** OpenAI Codex (DevOps specialist)

### **8. ?? COMMIT_CONVENTIONS.md** (Convenciones de Commits)
- **Propósito:** Formato semántico de commits en español
- **Contenido:**
  - Formato: type(scope): descripción
  - Types: feat, fix, refactor, docs, test, chore, style, perf
  - Scopes recomendados
  - Ejemplos por agente
  - Malos vs buenos commits
- **Audiencia:** Todos los desarrolladores e IAs

### **9. ??? NAVIGATION_MAP.md** (Mapa de Navegación)
- **Propósito:** Guía visual para encontrar información
- **Contenido:**
  - Flujos de lectura recomendados
  - Búsqueda por objetivo
  - Acceso por dispositivo
  - Tabla de tiempos
- **Audiencia:** Todos

### **10. ?? SETUP_SIN_MULTITENANCY.md** (Recrear sin Multi-Tenancy)
- **Propósito:** Comandos y pasos para eliminar multi-tenancy del proyecto
- **Contenido:**
  - Comandos ABP para crear proyecto limpio
  - Pasos para desactivar multi-tenancy
  - Scripts para limpiar módulos relacionados
  - Verificación de eliminación exitosa
  - **REGLA: Sin emojis en código/logs/comentarios**
- **Audiencia:** DevOps, Desarrolladores

### **11. ?? REGLA_NO_EMOJIS.md** (Regla Crítica - SIN EMOJIS)
- **Propósito:** Especificar dónde se PROHIBEN emojis
- **Contenido:**
  - Regla universal: No emojis en código/logs/comentarios
  - Dónde SÍ (solo en Markdown)
  - Dónde NO (código, logs, commits, variables, métodos)
  - Ejemplos CORRECTO vs INCORRECTO
  - Control de calidad y checklists
  - CI/CD validation
- **Audiencia:** TODOS los agents (Copilot, Gemini, Codex)

---

## ??? NAVEGACIÓN RÁPIDA

### **Por Rol**

#### ????? Product Manager / Cliente
1. Lee: **PLAN_2_SEMANAS.md** ? "Scope Funcional"
2. Comprende: Qué se entrega en 2 semanas (¿MVP completo?)
3. Consulta: Timeline y hitos (cuándo está listo)

#### ????? Desarrollador (Primer Día)
1. Lee: **DIA_1_GUIA_EJECUTIVA.md** ? Completo
2. Ejecuta: 6 tareas en orden (08:00 - 17:00)
3. Valida: Checklist final antes de irte

#### ????? Desarrollador (Próximos Días)
1. Consulta: **ARQUITECTURA_TECNICA.md** ? Secciones relevantes
2. Sigue: Día 2+ del plan (PLAN_2_SEMANAS.md)
3. Copia: Convenciones de código para mantener consistencia

#### ?? DevOps / Infrastructure
1. Lee: **PLAN_2_SEMANAS.md** ? "Arquitectura Técnica"
2. Prepara: Docker, GitHub Actions, VPS (semana 2)
3. Consulta: Endpoints de health check, logs, deployment

---

## ?? FLUJO DE USO POR DÍA

```
LUNES (Setup)
?? 08:00 ? Lee DIA_1_GUIA_EJECUTIVA.md (5 min)
?? 08:05 ? Tarea 1: Validar setup (55 min)
?? 09:00 ? Tarea 2: PostgreSQL (90 min)
?? 10:30 ? Tarea 3: Modelo dominio (90 min)
?? 12:00 ? Descanso (60 min)
?? 13:00 ? Tarea 4: DTOs (90 min)
?? 14:30 ? Tarea 5: DbContext + Migrations (150 min)
?? 17:00 ? Checklist final + git commit
?? EOD ? ? Setup completado

MARTES (Seeders + Repos)
?? 08:00 ? Consulta ARQUITECTURA_TECNICA.md § Modelo Dominio
?? ? Implementa data seeders (usuarios, propiedades demo)
?? ? Crea repositorios
?? EOD ? ? BD con datos de prueba + repos funcionales

MIÉRCOLES+ (Application Services ? Frontend ? DevOps)
?? Consulta ARQUITECTURA_TECNICA.md § Flujos Principales
?? Implementa servicio por servicio
?? Sigue convenciones de código
?? EOD ? Features completadas según plan
```

---

## ?? BÚSQUEDA POR PREGUNTA

### **¿Qué hago el lunes?**
? Lee: **DIA_1_GUIA_EJECUTIVA.md** (Completo)

### **¿Cuál es la arquitectura del proyecto?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Visión General"

### **¿Cómo se relacionan las entidades?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Modelo de Dominio"

### **¿Cuáles son los endpoints API?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Endpoints API Principales"

### **¿Cuánto tiempo tarda cada fase?**
? Lee: **PLAN_2_SEMANAS.md** § "Timeline Detallado (2 Semanas)"

### **¿Qué permisos tiene cada rol?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Autorización (RBAC)"

### **¿Cómo se carga una propiedad?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Flujo 2: Admin Publica Propiedad"

### **¿Qué tablas tiene la BD?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Base de Datos (PostgreSQL)"

### **¿Cuáles son las convenciones de código?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Convenciones de Código"

### **¿Qué tecnologías se usan?**
? Lee: **ARQUITECTURA_TECNICA.md** § "Tecnologías & Versiones"

### **¿Cómo debo hacer commits?**
? lee **COMMIT_CONVENTIONS.md** (Completo - formato semántico en español)

### **¿Cuál es la regla sobre emojis?**
? Lee **REGLA_NO_EMOJIS.md** (SIN emojis en código, logs, comentarios)

### **¿Cómo recreo el proyecto sin multi-tenancy?**
? Lee **SETUP_SIN_MULTITENANCY.md** (Comandos y pasos)

### **¿Qué debe hacer cada IA (Copilot, Gemini, Codex)?**
? lee **AGENTS.md** § "Dominios de Responsabilidad"

### **Soy Copilot, ¿qué debo hacer?**
? lee **AGENTS_COPILOT.md** (Guía especializada para backend)

### **Soy Gemini, ¿qué debo hacer?**
? lee **AGENTS_GEMINI.md** (Guía especializada para frontend)

### **Soy Codex, ¿qué debo hacer?**
? lee **AGENTS_CODEX.md** (Guía especializada para DevOps)

### **¿Cuáles son los antipatrones a evitar?**
? Consulta **AGENTS.md** § "Antipatrones a Evitar"

---

## ? CHECKPOINTS POR SEMANA

### **SEMANA 1: Backend Infrastructure**

| Día | Objetivo | Validación | Doc |
|-----|----------|-----------|-----|
| **L** | Setup, model, migrations | BD con esquema | DIA_1, AGENTS_COPILOT |
| **M** | Seeders + Repositories | Datos de prueba | PLAN/Día 2, AGENTS_COPILOT |
| **X** | App Services + Validación | CRUD services | PLAN/Día 3, AGENTS_COPILOT |
| **J** | HTTP API + Swagger | Endpoints documentados | PLAN/Día 4, AGENTS_COPILOT |
| **V** | Auth + OpenIddict | Usuarios + permisos | PLAN/Día 5, AGENTS_COPILOT |

**EOD Viernes:** Backend 100% funcional (Copilot)

### **SEMANA 2: Frontend & DevOps**

| Día | Objetivo | Validación | Doc |
|-----|----------|-----------|-----|
| **L** | Catálogo UI | Página responsive | PLAN/Día 6, AGENTS_GEMINI |
| **M** | Admin CRUD UI | Panel funcional | PLAN/Día 7, AGENTS_GEMINI |
| **X** | Portafolios + Estadísticas | Dashboard KPI | PLAN/Dia 8, AGENTS_GEMINI |
| **J** | Docker + CI/CD | Deploys automatizados | PLAN/Día 9, AGENTS_CODEX |
| **V** | QA + Documentación | MVP pulido | PLAN/Día 10, Todos |

**EOD Viernes:** MVP 100% listo para producción (Gemini + Codex)

---

## ?? PRIMEROS PASOS

### **Paso 1: Entender el Plan (5 min)**
```
Lee: PLAN_2_SEMANAS.md
?? Secciones: Visión, Scope Funcional, Arquitectura, Timeline
```

### **Paso 2: Entender la Arquitectura (10 min)**
```
Lee: ARQUITECTURA_TECNICA.md
?? Secciones: Visión General, Estructura de Proyectos, Modelo de Dominio
```

### **Paso 3: Ejecutar Lunes (8 horas)**
```
Lee: DIA_1_GUIA_EJECUTIVA.md (5 min)
? Ejecuta: Tareas 1-5 (470 min)
? Valida: Checklist final (5 min)
```

### **Paso 4: Continuar Martes+ (según plan)**
```
Consulta ARQUITECTURA_TECNICA.md según necesites
Lee cada sección del día en PLAN_2_SEMANAS.md
```

---

## ?? SOPORTE & TROUBLESHOOTING

### **Problema: No sé dónde empezar**
? Ve a **DIA_1_GUIA_EJECUTIVA.md** sección "TAREAS ESPECÍFICAS"

### **Problema: Falla PostgreSQL**
? Ve a **DIA_1_GUIA_EJECUTIVA.md** sección "TROUBLESHOOTING COMÚN"

### **Problema: No entiendo la arquitectura**
? Lee **ARQUITECTURA_TECNICA.md** § "Visión General" + diagramas

### **Problema: ¿Qué código debo escribir hoy?**
? Busca tu día en **PLAN_2_SEMANAS.md** o **DIA_1_GUIA_EJECUTIVA.md**

### **Problema: No encuentro un endpoint**
? Consulta **ARQUITECTURA_TECNICA.md** § "Endpoints API Principales"

### **Problema: ¿Cuáles son las validaciones?**
? Consulta **ARQUITECTURA_TECNICA.md** § "Modelo de Dominio"

---

## ?? ESTRUCTURA VISUAL

```
docs/
?? README.md (este archivo - índice maestro)
?? INICIO.md (resumen ejecutivo)
?? PLAN_2_SEMANAS.md (plan maestro 10 días)
?? DIA_1_GUIA_EJECUTIVA.md (guía lunes detallada)
?? ARQUITECTURA_TECNICA.md (referencia técnica permanente)
?? AGENTS.md (guía maestro para IAs - convenciones, protocolos)
?? AGENTS_COPILOT.md (guía especializada - Backend, .NET, DDD)
?? AGENTS_GEMINI.md (guía especializada - Frontend, Blazor, UX)
?? AGENTS_CODEX.md (guía especializada - DevOps, Docker, CI/CD)
?? COMMIT_CONVENTIONS.md (convenciones de commits semánticos)
?? NAVIGATION_MAP.md (mapa visual de documentación)
?? SETUP_SIN_MULTITENANCY.md (comandos para recrear sin MT)
?? REGLA_NO_EMOJIS.md (regla crítica: sin emojis en código)
?
?? (PRÓXIMAMENTE - Fase 2+)
? ?? API.md (documentación endpoints completa)
? ?? DATABASE.md (esquema SQL, migraciones, backup)
? ?? DEPLOYMENT.md (guía paso-a-paso producción)
? ?? DEVELOPMENT.md (setup local, debugging, testing)
? ?? ADMIN_GUIDE.md (manual operación para cliente)
?
?? (ARCHIVOS GENERADOS)
  ?? migrations/ (sql scripts)
  ?? logs/ (deployment logs)
  ?? backups/ (db backups)
```

---

## ?? CICLO DE ACTUALIZACIÓN

| Documento | Frecuencia | Responsable | Triggers |
|-----------|-----------|------------|----------|
| PLAN_2_SEMANAS.md | EOD Viernes S1 | Dev Lead | Después checkpoint semana 1 |
| DIA_1_GUIA_EJECUTIVA.md | EOD Lunes | Dev Lead | Después completar lunes |
| ARQUITECTURA_TECNICA.md | EOD Día X/Semana | Dev | Cambios arquitectura significativos |
| (Futuros docs) | EOD cada fase | Dev | Fin de cada entregable |

---

## ?? RECURSOS EXTERNOS

### Oficiales
- [ABP Framework Docs](https://docs.abp.io) - DDD, modularidad, permisos
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor) - UI framework
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) - ORM
- [PostgreSQL Docs](https://www.postgresql.org/docs/) - Base de datos
- [OpenIddict](https://documentation.openiddict.com/) - OAuth2 / OIDC

### Comunidad
- [ABP Studio](https://abp.studio) - Scaffolding y templates
- [Stack Overflow](https://stackoverflow.com/questions/tagged/abp) - Q&A
- GitHub Issues del proyecto

---

## ? TIPS DE PRODUCTIVIDAD

1. **Lunes mañana:** Lee DIA_1_GUIA_EJECUTIVA.md **completo** antes de empezar
2. **Durante desarrollo:** Mantén ARQUITECTURA_TECNICA.md abierto en segundo monitor
3. **Cuando atascado:** Busca en este README la sección relevante
4. **Antes de hacer cambio:** Consulta convenciones en ARQUITECTURA_TECNICA.md
5. **EOD cada día:** Commit al repo, actualiza plan si hay cambios

---

## ?? OBJETIVO FINAL

**Después de 2 semanas (10 días laborales):**

- ? MVP 100% funcional (catálogo, admin, auth)
- ? BD PostgreSQL en producción
- ? Docker & CI/CD automatizado
- ? Documentación completa
- ? Code clean y testeable
- ? **Listo para escalar**

---

## ?? DOCUMENTOS CREADOS (Nuevos en esta sesión)

? **AGENTS.md** - Guía maestro para IAs (Copilot, Gemini, Codex)  
? **AGENTS_COPILOT.md** - Especialización: Backend, .NET, DDD  
? **AGENTS_GEMINI.md** - Especialización: Frontend, Blazor, UX  
? **AGENTS_CODEX.md** - Especialización: DevOps, Docker, CI/CD  
? **COMMIT_CONVENTIONS.md** - Convenciones de commits semánticos en español  
? **NAVIGATION_MAP.md** - Mapa visual de navegación de documentos  
? **SETUP_SIN_MULTITENANCY.md** - Documentación para recrear sin multi-tenancy
? **REGLA_NO_EMOJIS.md** - Documentación de regla crítica para el uso de emojis

---

**Última actualización:** Sesión AGENTS completada  
**Próxima revisión:** Viernes EOD S1  
**Versión:** 1.0 - MVP Plan + IA Agents + Commit Conventions

