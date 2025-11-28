# ? INFORMACIÓN NECESARIA ANTES DE CONFIGURAR RAILWAY

## ? YA TENGO

- ? Rama `develop` creada
- ? Archivos Railway configurados (`railway.json`, `Procfile`)
- ? Documentación completa
- ? Scripts de setup

---

## ? NECESITO SABER

### 1. ¿Ya tienes cuenta en Railway?

**Opciones:**
- [ ] **SÍ** - Ya tengo cuenta en Railway
- [ ] **NO** - Necesito crear cuenta (2 minutos)

**Si NO:** 
1. Ir a https://railway.app/
2. Clic "Login with GitHub"
3. Autorizar Railway
4. ¡Listo! No requiere tarjeta de crédito

---

### 2. ¿Quieres que configure variables automáticamente?

Railway genera algunas variables automáticamente (PostgreSQL), pero necesito saber:

**¿Tienes datos sensibles adicionales?**
- [ ] NO - Solo usar las variables estándar
- [ ] SÍ - Tengo API keys, secrets, etc.

**Si SÍ, ¿cuáles?** (opcional ahora, puedes agregarlas después)
```
Por ejemplo:
- GOOGLE_MAPS_API_KEY=...
- SENDGRID_API_KEY=...
- STORAGE_ACCOUNT_KEY=...
```

---

### 3. ¿Qué nombre prefieres para el proyecto en Railway?

**Opciones:**
- [ ] `cima` (simple)
- [ ] `cima-staging` (descriptivo)
- [ ] Otro: `________________`

**Nota:** Railway genera URL como `[nombre]-production-xyz.up.railway.app`

---

### 4. ¿Quieres dominio personalizado para staging?

**Opciones:**
- [ ] NO - Usar URL generada por Railway (`cima-staging-xyz.up.railway.app`)
- [ ] SÍ - Usar dominio propio (`staging.cima.com` o similar)

**Si SÍ:**
- Dominio: `____________________`
- ¿Ya tienes el dominio? [ ] SÍ [ ] NO

---

### 5. ¿Configuramos protección de ramas en GitHub ahora?

**Opciones:**
- [ ] SÍ - Configurar protección para `master` y `develop`
- [ ] NO - Lo configuramos después

**Si SÍ, necesito:**
- ¿Quieres require PR reviews? [ ] SÍ [ ] NO
- ¿Cuántas aprobaciones mínimo? [ ] 1 [ ] 2
- ¿Quiénes pueden hacer merge? `________________`

---

### 6. ¿Ejecuto el push ahora o prefieres revisarlo primero?

**Opciones:**
- [ ] **PUSH AHORA** - Ejecutar script automático
- [ ] **REVISAR PRIMERO** - Mostrarme qué se va a pushear

---

## ?? RESUMEN DE LO QUE SE VA A PUSHEAR

### Archivos Nuevos

```
railway.json                          # Config Railway
Procfile                              # Comando inicio
docs/GIT_BRANCHING_STRATEGY.md        # Estrategia Git
docs/RAILWAY_STAGING_CONFIG.md        # Config Railway detallada
docs/RAILWAY_SETUP_EJECUTIVO.md       # Resumen ejecutivo
etc/scripts/setup-railway-staging.ps1 # Script setup
```

### Archivos Modificados

```
.github/workflows/cd-deploy-staging.yml  # Workflow para develop
```

### Rama

```
develop (nueva rama desde master)
```

---

## ? RESPUESTAS RÁPIDAS (RECOMENDADAS)

**Si quieres ir rápido y usar configuración estándar:**

```
1. Cuenta Railway: NO (créala en 2 min)
2. Variables adicionales: NO
3. Nombre proyecto: cima-staging
4. Dominio personalizado: NO
5. Protección ramas: NO (después)
6. Push: PUSH AHORA
```

Con estas respuestas, puedo configurar todo en **10 minutos**.

---

## ?? COMANDO CUANDO RESPONDAS

Después de que me digas tus preferencias, ejecutaré:

```powershell
# Opción automática
.\etc\scripts\setup-railway-staging.ps1

# O si prefieres revisar primero
git status
git diff --cached
```

---

## ?? TUS RESPUESTAS

**Copia y pega esto con tus respuestas:**

```
1. Cuenta Railway: [ ] SÍ / [ ] NO
2. Variables adicionales: [ ] SÍ / [ ] NO
   (Si SÍ, listar: ________________)
3. Nombre proyecto: ________________
4. Dominio personalizado: [ ] SÍ / [ ] NO
   (Si SÍ, dominio: ________________)
5. Protección ramas: [ ] SÍ / [ ] NO
6. Push: [ ] AHORA / [ ] REVISAR PRIMERO
```

---

**Cuando me respondas, procedo con la configuración completa.**
