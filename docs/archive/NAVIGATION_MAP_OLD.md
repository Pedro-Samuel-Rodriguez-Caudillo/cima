# ??? MAPA VISUAL - NAVEGACIÓN DE DOCUMENTACIÓN

## ?? "ESTOY AQUÍ, ¿A DÓNDE VOY?"

### **Primera Vez en el Proyecto**
```
START ? README.md
    ? (5 min lectura)
    ??? PLAN_2_SEMANAS.md (entender qué se entrega)
    ??? ARQUITECTURA_TECNICA.md (ver diagrama de capas)
    ??? DIA_1_GUIA_EJECUTIVA.md (si es lunes 08:00)
```

### **Soy Desarrollador (Humano)**
```
¿Qué hago hoy?
    ?? LUNES ? DIA_1_GUIA_EJECUTIVA.md
    ?? MARTES-VIERNES ? PLAN_2_SEMANAS.md (día correspondiente)
    ?? DURANTE CÓDIGO ? ARQUITECTURA_TECNICA.md (referencia)
    ?? ANTES COMMIT ? COMMIT_CONVENTIONS.md (formato semántico)
```

### **Soy GitHub Copilot (Backend IA)**
```
START ? AGENTS_COPILOT.md (esta es TU guía)
    ?
    ?? Entender tu área (Domain, Application, HttpApi, EfCore)
    ?? Ver ejemplos de código completo
    ?? Seguir flujo: Entidad ? DTO ? Service ? Controller
    ?? Aplicar convenciones C# y DDD
    ?? Commitar en formato: feat(domain): descripción
    
Referencia ? AGENTS.md (reglas compartidas con Gemini y Codex)
```

### **Soy Google Gemini (Frontend IA)**
```
START ? AGENTS_GEMINI.md (esta es TU guía)
    ?
    ?? Entender tu área (Pages, Shared, wwwroot)
    ?? Ver ejemplos de componentes Razor completos
    ?? Seguir flujo: Contenedor ? Componentes ? Estilos
    ?? Aplicar Tailwind CSS y accesibilidad
    ?? Commitar en formato: feat(blazor): descripción
    
Referencia ? AGENTS.md (reglas compartidas con Copilot y Codex)
```

### **Soy OpenAI Codex (DevOps IA)**
```
START ? AGENTS_CODEX.md (esta es TU guía)
    ?
    ?? Entender tu área (Docker, CI/CD, scripts, infrastructure)
    ?? Ver ejemplos completos: Dockerfile, compose, workflows
    ?? Seguir flujo: Build ? Test ? Deploy ? Health Check
    ?? Aplicar security best practices
    ?? Commitar en formato: chore(devops): descripción
    
Referencia ? AGENTS.md (reglas compartidas con Copilot y Gemini)
```

---

## ?? POR OBJETIVO

### **"Quiero entender la arquitectura"**
```
ARQUITECTURA_TECNICA.md
    ?? § Visión General (diagrama de capas)
    ?? § Estructura de Proyectos (carpetas)
    ?? § Modelo de Dominio (DDD)
    ?? § Flujos Principales (catálogo, admin, contacto)
    ?? § Base de Datos (tablas, índices)
```

### **"Quiero hacer commits correctamente"**
```
COMMIT_CONVENTIONS.md
    ?? Formato: <type>(<scope>): <subject>
    ?? Types: feat, fix, refactor, docs, chore, etc.
    ?? Scopes: domain, application, blazor, docker, etc.
    ?? Ejemplos por agente (Copilot, Gemini, Codex)
```

### **"Quiero saber qué hace cada IA"**
```
AGENTS.md
    ?? § Dominios de Responsabilidad (tabla roles)
    ?? § Protocolos de Colaboración (cómo trabajan juntas)
    ?? § Convenciones de Código (por lenguaje)
    ?? § Áreas de Enfoque (qué toca cada una)
```

### **"Necesito código de ejemplo [Copilot]"**
```
AGENTS_COPILOT.md
    ?? § Flujo: Cómo Pensar (Backend)
    ?? Paso 1: Entidad (Domain)
    ?? Paso 2: DTOs (Domain.Shared)
    ?? Paso 3: Validator (Application)
    ?? Paso 4: Repository (EntityFrameworkCore)
    ?? Paso 5: AppService (Application)
    ?? Paso 6: Controller (HttpApi)
    ?? Paso 7: DbContext mapping
    ?? Paso 8: AutoMapper profile
    ?? Paso 9: Permisos
```

### **"Necesito código de ejemplo [Gemini]"**
```
AGENTS_GEMINI.md
    ?? § Flujo: Cómo Pensar (Frontend)
    ?? Paso 1: Contenedor (Página)
    ?? Paso 2: Componente PropertyCard
    ?? Paso 3: Filtros
    ?? Paso 4: Detalle de Propiedad
    ?? Convenciones Tailwind CSS
    ?? Accesibilidad (a11y)
    ?? Componentes reutilizables
```

### **"Necesito código de ejemplo [Codex]"**
```
AGENTS_CODEX.md
    ?? § Flujo: Cómo Pensar (DevOps)
    ?? Dockerfile (multi-stage optimizado)
    ?? docker-compose.yml (dev)
    ?? docker-compose.prod.yml (prod)
    ?? GitHub Actions (CI)
    ?? GitHub Actions (CD staging)
    ?? GitHub Actions (CD prod)
    ?? Scripts PowerShell (backup, health-check)
    ?? Seguridad y troubleshooting
```

### **"¿Qué debo hacer el lunes?"**
```
DIA_1_GUIA_EJECUTIVA.md
    ?? 08:00-09:00 ? Validar setup
    ?? 09:00-10:30 ? PostgreSQL local
    ?? 10:30-12:00 ? Modelo de dominio
    ?? 12:00-13:00 ? ? Descanso
    ?? 13:00-14:30 ? DTOs
    ?? 14:30-16:00 ? DbContext
    ?? 16:00-17:00 ? Migrations
    ?? Checklist final
```

### **"Necesito el plan general"**
```
PLAN_2_SEMANAS.md
    ?? § Visión del Proyecto
    ?? § Scope Funcional (qué se entrega)
    ?? § Arquitectura Técnica (stack)
    ?? § Timeline Detallado (por día)
    ?   ?? Semana 1: Backend
    ?   ?? Semana 2: Frontend + DevOps
    ?? § Día 1: Setup & Infraestructura
    ?? § Entregables Finales
    ?? § Backlog & Fases Futuras
```

---

## ?? FLUJO DE LECTURA RECOMENDADO

### **Opción 1: RÁPIDO (30 minutos)**
```
1. README.md (5 min)
   ? Lee "PRIMEROS PASOS"
   
2. PLAN_2_SEMANAS.md (10 min)
   ? Lee "Scope Funcional" + "Timeline"
   
3. ARQUITECTURA_TECNICA.md (10 min)
   ? Lee "Visión General" + "Modelo de Dominio"
   
4. Tu guía especializada (5 min)
   ? AGENTS_COPILOT.md / AGENTS_GEMINI.md / AGENTS_CODEX.md
   ? Lee "Área de Trabajo"
```

### **Opción 2: COMPLETO (2 horas)**
```
1. README.md (10 min)
2. PLAN_2_SEMANAS.md (25 min) - completo
3. ARQUITECTURA_TECNICA.md (25 min) - completo
4. Tu guía especializada (40 min) - completo
5. AGENTS.md (10 min)
6. COMMIT_CONVENTIONS.md (10 min)
```

### **Opción 3: LUNES EXECUTION (DÍA 1)**
```
08:00 ? DIA_1_GUIA_EJECUTIVA.md (5 min)
        + AGENTS_COPILOT.md/GEMINI/CODEX (browse)
        + COMMIT_CONVENTIONS.md (quick ref)
        
08:05-17:00 ? Ejecutar 6 tareas del día
```

---

## ?? ACCESO POR DISPOSITIVO

### **Desktop (VS Code / IDE)**
```
Abre: /docs/README.md ? Usar como índice
Mantén abiertos:
  - ARQUITECTURA_TECNICA.md (pestaña permanente)
  - Tu guía especializada (AGENTS_COPILOT/GEMINI/CODEX)
  - COMMIT_CONVENTIONS.md (referencia rápida)
```

### **Segundo Monitor**
```
Monitor 1: Código + IDE
Monitor 2: Docs en navegador web
  - README.md en índice
  - Tu guía especializada
  - COMMIT_CONVENTIONS.md bookmark
```

### **Mobile / Tablet**
```
No recomendado para lectura detallada
Úsalo para:
  - COMMIT_CONVENTIONS.md (copypaste de ejemplos)
  - Búsqueda rápida en README.md
```

---

## ?? BÚSQUEDA RÁPIDA (Ctrl+F)

```
¿Busco?                          Archivo + Sección
?????????????????????????????????????????????????????????
Diagrama de capas               ARQUITECTURA_TECNICA.md § Visión General
Entidades del proyecto          ARQUITECTURA_TECNICA.md § Modelo de Dominio
Endpoints API                   ARQUITECTURA_TECNICA.md § Endpoints API
Tablas BD                        ARQUITECTURA_TECNICA.md § Base de Datos
Convención commits              COMMIT_CONVENTIONS.md (completo)
Qué hace cada IA                AGENTS.md § Dominios de Responsabilidad
Código backend ejemplo          AGENTS_COPILOT.md § Paso 1-9
Código frontend ejemplo         AGENTS_GEMINI.md § Paso 1-4
Código DevOps ejemplo           AGENTS_CODEX.md § Dockerfile/Compose/Actions
Checklist QA                    AGENTS_COPILOT/GEMINI/CODEX § Checklist Pre-Commit
Timeline proyecto              PLAN_2_SEMANAS.md § Timeline Detallado
Qué hacer lunes                DIA_1_GUIA_EJECUTIVA.md (completo)
Flujos principales             ARQUITECTURA_TECNICA.md § Flujos Principales
RBAC / Permisos               ARQUITECTURA_TECNICA.md § Autorización (RBAC)
Antipatrones                  AGENTS.md § Antipatrones a Evitar
```

---

## ?? INICIO RÁPIDO POR ROL

### **Eres PM/Client Manager?**
```
1. Lee: PLAN_2_SEMANAS.md (20 min)
   ? Qué se entrega, timeline, scope
2. Consulta: README.md cuando necesites algo
3. Comparte: ADMIN_GUIDE.md (cuando esté lista, Fase 2)
```

### **Eres Desarrollador (humano)?**
```
1. Lee: README.md (10 min)
2. Lee: DIA_1_GUIA_EJECUTIVA.md o Día N del PLAN_2_SEMANAS.md
3. Mantén: ARQUITECTURA_TECNICA.md abierto
4. Usa: COMMIT_CONVENTIONS.md para commits
5. Consulta: AGENTS.md si trabajas con IAs
```

### **Eres IA (Copilot/Gemini/Codex)?**
```
1. Lee: AGENTS.md (guía general)
2. Lee: Tu guía especializada (AGENTS_COPILOT/GEMINI/CODEX)
3. Referencia: COMMIT_CONVENTIONS.md para cada commit
4. Consulta: ARQUITECTURA_TECNICA.md para contexto técnico
```

---

## ?? TAMAÑO ESTIMADO POR LECTURA

| Documento | Tamaño | Lectura | Ref Permanente |
|-----------|--------|---------|-----------------|
| README.md | ?? Pequeño | 5 min | SÍ (índice) |
| PLAN_2_SEMANAS.md | ?? Mediano | 20 min | SÍ (timeline) |
| DIA_1_GUIA_EJECUTIVA.md | ?? Mediano | 15 min | SÍ (lunes) |
| ARQUITECTURA_TECNICA.md | ?? Grande | 30 min | SÍ (permanente) |
| AGENTS.md | ?? Mediano | 15 min | SÍ (protocolos) |
| AGENTS_COPILOT.md | ?? Mediano | 20 min | SÍ (si eres Copilot) |
| AGENTS_GEMINI.md | ?? Mediano | 20 min | SÍ (si eres Gemini) |
| AGENTS_CODEX.md | ?? Mediano | 20 min | SÍ (si eres Codex) |
| COMMIT_CONVENTIONS.md | ?? Pequeño | 10 min | SÍ (referencia) |

**Total recomendado inicial:** 1.5-2 horas  
**Total lectura lunes ejecución:** 30 minutos + 8 horas de trabajo

---

## ? TIPS

1. **Usa Ctrl+F para buscar dentro de docs**
2. **Copia-pega código directamente (está ready-to-use)**
3. **Si algo no está documentado, crea issue o PR**
4. **Actualiza docs si cambias arquitectura**
5. **Mantén archivos en sincronía (no versiones desactualizadas)**

---

**Última actualización:** Setup inicial  
**Próxima revisión:** Viernes EOD Semana 1  
**Versión:** 1.0 - MVP Documentation

