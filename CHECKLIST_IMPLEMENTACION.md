# CHECKLIST DE IMPLEMENTACIÓN

Usa este checklist para implementar la nueva estrategia paso a paso.

## ? Fase 1: Reorganización (COMPLETADO)

- [x] Crear nueva estructura de carpetas en `docs/`
- [x] Copiar archivos a nueva ubicación
- [x] Crear `.github/BRANCHING_STRATEGY.md`
- [x] Crear `.github/SETUP_BRANCH_PROTECTION.md`
- [x] Actualizar workflows de GitHub Actions
- [x] Crear script de limpieza `cleanup-docs.ps1`
- [x] Crear `docs/INDEX.md`

## ?? Fase 2: Configuración GitHub (PENDIENTE)

### 2.1 Crear Branch `staging`

```bash
git checkout develop
git pull origin develop
git checkout -b staging
git push origin staging
git branch --set-upstream-to=origin/staging staging
```

- [ ] Ejecutar comandos arriba
- [ ] Verificar que `staging` existe en GitHub

### 2.2 Configurar Protección de Branches

Sigue: `.github/SETUP_BRANCH_PROTECTION.md`

**Para `main`:**
- [ ] Ir a Settings > Branches > Add rule
- [ ] Branch pattern: `main`
- [ ] ? Require pull request before merging
- [ ] ? Require approvals: 1
- [ ] ? Require status checks to pass
- [ ] ? Require branches to be up to date
- [ ] ? Do not allow bypassing
- [ ] ? Restrict who can push (solo admins)
- [ ] Click "Create"

**Para `staging`:**
- [ ] Add new rule
- [ ] Branch pattern: `staging`
- [ ] ? Require pull request before merging
- [ ] ? Require approvals: 1
- [ ] ? Require status checks to pass
- [ ] ? Require branches to be up to date
- [ ] Click "Create"

**Para `develop`:**
- [ ] Add new rule
- [ ] Branch pattern: `develop`
- [ ] ? Require pull request before merging
- [ ] ? Require approvals: 0
- [ ] ? Require status checks to pass
- [ ] ? Allow admins to bypass (emergencias)
- [ ] Click "Create"

### 2.3 Configurar Environment de Producción (Opcional)

- [ ] Settings > Environments > New environment
- [ ] Nombre: `production`
- [ ] Required reviewers: (agregar al menos 1)
- [ ] Deployment branches: Only `main`
- [ ] Save

## ?? Fase 3: Configuración Railway (PENDIENTE)

### 3.1 Cambiar Branch de Deploy

- [ ] Ir a [Railway Dashboard](https://railway.app)
- [ ] Seleccionar proyecto CIMA
- [ ] Settings > Service
- [ ] Buscar "Source"
- [ ] Cambiar "Branch" de `develop` ? `staging`
- [ ] Save changes

### 3.2 Verificar Variables de Entorno

- [ ] En Railway, ir a Variables
- [ ] Verificar que existan todas las necesarias:
  - [ ] `ASPNETCORE_ENVIRONMENT=Staging`
  - [ ] `ConnectionStrings__Default`
  - [ ] Variables de OpenIddict
  - [ ] Otras variables de `appsettings.Staging.json`

### 3.3 Trigger Deploy Manual (Prueba)

- [ ] En Railway, click "Deploy"
- [ ] Seleccionar `staging` branch
- [ ] Verificar que build sea exitoso
- [ ] Verificar que app esté corriendo
- [ ] Probar URL de staging

## ?? Fase 4: Limpieza de Documentación (PENDIENTE)

### 4.1 Ejecutar Script de Limpieza

```powershell
.\etc\scripts\cleanup-docs.ps1
```

- [ ] Ejecutar script
- [ ] Verificar que archivos se movieron a `archive/`
- [ ] Verificar que temporales se eliminaron
- [ ] Review final de estructura en `docs/`

### 4.2 Limpiar Agents (Opcional)

```bash
# Opcional: renombrar AGENTS.md a README.md
Move-Item -Path "agents\AGENTS.md" -Destination "agents\README.md"
```

- [ ] Decidir si hacer cambio arriba
- [ ] Agregar referencia a branching strategy en cada agent doc

## ?? Fase 5: Prueba del Nuevo Flujo (PENDIENTE)

### 5.1 Crear Feature de Prueba

```bash
git checkout develop
git pull origin develop
git checkout -b feature/test-new-flow

# Hacer cambio mínimo
echo "# Test" >> TEST.md
git add TEST.md
git commit -m "feat: test new branching flow"
git push origin feature/test-new-flow
```

- [ ] Ejecutar comandos
- [ ] Crear PR en GitHub: `feature/test-new-flow` ? `develop`
- [ ] Verificar que CI corre
- [ ] Verificar que puede mergear sin aprobación
- [ ] Mergear PR
- [ ] Verificar que Railway NO deployó (correcto!)

### 5.2 Probar Deploy a Staging

```bash
# Crear PR en GitHub UI: develop ? staging
```

- [ ] Crear PR: `develop` ? `staging` en GitHub
- [ ] Verificar que CI corre
- [ ] Verificar que requiere aprobación
- [ ] Aprobar PR
- [ ] Mergear
- [ ] Verificar que Railway deployó automáticamente
- [ ] Verificar que staging está actualizado

### 5.3 Limpiar Feature de Prueba

```bash
git checkout develop
git pull origin develop
git branch -d feature/test-new-flow
git push origin --delete feature/test-new-flow

# Remover TEST.md
git rm TEST.md
git commit -m "chore: remove test file"
git push origin develop
```

- [ ] Ejecutar comandos
- [ ] Verificar que branch de prueba se eliminó

## ?? Fase 6: Documentar para el Equipo (PENDIENTE)

### 6.1 Comunicar Cambios

- [ ] Compartir `REORGANIZACION_COMPLETADA.md` con el equipo
- [ ] Compartir `.github/BRANCHING_STRATEGY.md`
- [ ] Explicar el nuevo flujo de trabajo

### 6.2 Actualizar README Principal (Opcional)

- [ ] Agregar link a `docs/INDEX.md` en README.md principal
- [ ] Agregar link a `.github/BRANCHING_STRATEGY.md`
- [ ] Actualizar badges si aplica

## ?? Verificación Final

### Checklist de Validación

- [ ] Branch `main` existe y está protegido
- [ ] Branch `staging` existe y está protegido
- [ ] Branch `develop` existe y está protegido
- [ ] Railway deploya desde `staging` (no `develop`)
- [ ] CI corre en `feature/*`, `bugfix/*`, `develop`
- [ ] Docs organizados en carpetas
- [ ] Scripts de limpieza disponibles
- [ ] Team informado del nuevo flujo

### Test de Protecciones

**Test 1: No push directo a main**
```bash
git checkout main
echo "test" > test.txt
git add test.txt
git commit -m "test"
git push origin main
```
- [ ] Debe ser rechazado ?

**Test 2: PR requiere approval en main**
- [ ] Crear PR a main desde cualquier branch
- [ ] Intentar mergear sin aprobación
- [ ] Debe estar bloqueado ?

**Test 3: CI debe pasar**
- [ ] Crear PR con código que no compile
- [ ] Debe bloquearse hasta que CI pase ?

## ?? Resultado Esperado

Al completar este checklist:

? Estructura de docs limpia y organizada
? Branches protegidos en GitHub
? Railway deploying solo desde `staging`
? Flujo de trabajo claro: feature ? develop ? staging ? main
? CI/CD funcionando correctamente
? Team capacitado en nuevo flujo

## ?? Si Tienes Problemas

### Problema: No puedo crear regla de protección
**Solución:** Necesitas permisos de admin en el repo. Contacta al owner.

### Problema: CI no reconoce el status check
**Solución:** El workflow debe correr al menos una vez antes de poder seleccionarlo.

### Problema: Railway no deploya
**Solución:** 
1. Verifica que branch sea `staging`
2. Verifica que haya cambios en `staging`
3. Revisa logs en Railway

### Problema: Merge bloqueado
**Solución:** Revisa qué falta:
- Aprobaciones?
- CI pasó?
- Conversaciones resueltas?

## ?? Próximos Pasos Después de Completar

1. **Desarrollar siguiente feature** usando el nuevo flujo
2. **Documentar lecciones aprendidas** en `docs/03-development/`
3. **Optimizar CI/CD** si es necesario
4. **Configurar producción** cuando esté listo

---

**Tiempo estimado total:** 2-3 horas

**Dificultad:** Media

**Recomendación:** Hacer en horario de bajo tráfico para pruebas
