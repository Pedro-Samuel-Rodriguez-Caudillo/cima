# ?? CONFIGURAR BRANCH PROTECTIONS - GUÍA RÁPIDA

## ? ACCIÓN INMEDIATA REQUERIDA

Después de pushear estos cambios, debes configurar las **branch protections** en GitHub para que los quality gates funcionen correctamente.

---

## ?? PASO A PASO

### **1. Ir a Settings de tu repositorio**
```
https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima/settings/branches
```

---

### **2. Proteger Branch `main`**

**Click en "Add branch protection rule"**

**Branch name pattern:**
```
main
```

**Configuración requerida:**

? **Require a pull request before merging**
   - [x] Require approvals: **1**
   - [x] Dismiss stale pull request approvals when new commits are pushed

? **Require status checks to pass before merging**
   - [x] Require branches to be up to date before merging
   - **Status checks** (buscar y seleccionar):
     - `build-and-test`
     - `test-domain / Domain Layer Tests (124 tests)`
     - `test-status / Test Status Check`

? **Require conversation resolution before merging**

? **Do not allow bypassing the above settings**

? **Restrict who can push to matching branches**
   - Agregar solo: **Admins** y/o **tu usuario**

**Click "Create"** ?

---

### **3. Proteger Branch `staging`**

**Click en "Add branch protection rule"**

**Branch name pattern:**
```
staging
```

**Configuración requerida:**

? **Require a pull request before merging**
   - [x] Require approvals: **1**
   - [x] Dismiss stale pull request approvals when new commits are pushed

? **Require status checks to pass before merging**
   - [x] Require branches to be up to date before merging
   - **Status checks** (buscar y seleccionar):
     - `build-and-test`
     - `test-domain / Domain Layer Tests (124 tests)`
     - `run-tests / Run Tests Before Deploy`

? **Require conversation resolution before merging**

?? **Allow force pushes** (solo para admins en emergencias)
   - [x] Specify who can force push
   - Agregar: **Solo admins**

**Click "Create"** ?

---

### **4. Proteger Branch `develop`**

**Click en "Add branch protection rule"**

**Branch name pattern:**
```
develop
```

**Configuración requerida:**

? **Require a pull request before merging**
   - [ ] Require approvals: **0** (opcional, para agilidad)
   - [x] Dismiss stale pull request approvals when new commits are pushed

? **Require status checks to pass before merging**
   - [x] Require branches to be up to date before merging
   - **Status checks** (buscar y seleccionar):
     - `build-and-test`
     - `test-domain / Domain Layer Tests (124 tests)`

?? **Allow administrators to bypass pull request requirements**
   - [x] (para urgencias solamente)

**Click "Create"** ?

---

## ?? RESULTADO ESPERADO

Después de configurar, tendrás:

### **Branch `main`**
```
?? Protegido completamente
? Requiere PR + 1 aprobación
? Requiere tests pasando (124 tests)
? Solo admins pueden hacer push directo
? No se puede bypass
```

### **Branch `staging`**
```
?? Protegido con flexibilidad
? Requiere PR + 1 aprobación
? Requiere tests pasando
? Quality gate antes de deploy
?? Force push solo admins (emergencias)
```

### **Branch `develop`**
```
?? Protegido para calidad
? Requiere PR (sin aprobación obligatoria)
? Requiere tests pasando
?? Admins pueden bypass (urgencias)
```

---

## ?? VERIFICAR CONFIGURACIÓN

### **Test 1: Crear PR a develop**
```bash
# 1. Crear feature branch
git checkout -b feature/test-protection

# 2. Hacer cambio
echo "test" > test.txt
git add test.txt
git commit -m "test: verificar branch protection"

# 3. Push
git push origin feature/test-protection

# 4. Crear PR en GitHub a develop
# Resultado esperado:
# ? GitHub Actions ejecuta automáticamente
# ? Tests deben pasar para poder merge
# ? Status check "build-and-test" aparece
# ? Status check "test-domain" aparece
```

### **Test 2: Intentar push directo a main**
```bash
git checkout main
git pull
echo "test" >> README.md
git add README.md
git commit -m "test: push directo"
git push origin main

# Resultado esperado:
# ? Error: protected branch
# ? Protección funcionando correctamente
```

---

## ?? IMPORTANTE

### **Después de configurar:**

1. ? **Todas las features nuevas** deben ir via PR
2. ? **Tests deben pasar** para merge
3. ? **Staging solo recibe de develop** via PR
4. ? **Main solo recibe de staging** via PR
5. ? **Deploy solo con tests passing**

### **Si necesitas hacer push urgente:**

Solo admins pueden bypass en `develop` para urgencias.
Para `main` y `staging`, **NUNCA bypass** - siempre via PR con tests.

---

## ?? CHECKLIST DE ACCIÓN

- [ ] 1. Push de los cambios de CI/CD actuales
- [ ] 2. Ir a GitHub Settings ? Branches
- [ ] 3. Configurar protección para `main`
- [ ] 4. Configurar protección para `staging`
- [ ] 5. Configurar protección para `develop`
- [ ] 6. Verificar que status checks aparezcan
- [ ] 7. Crear PR de prueba para validar
- [ ] 8. Confirmar que tests bloquean merge si fallan

---

## ?? UNA VEZ CONFIGURADO

Tu proyecto tendrá:

? **CI/CD Completo**
- Tests automáticos
- Quality gates
- Deploy validado

? **Código de Alta Calidad**
- Solo código testeado en producción
- Revisiones obligatorias
- Historial limpio

? **Desarrollo Ágil y Seguro**
- Feedback inmediato
- Merge confiable
- Producción estable

---

**Tiempo estimado:** 10-15 minutos
**Dificultad:** Fácil (solo click en GitHub UI)
**Impacto:** ALTO - protege todo tu código
