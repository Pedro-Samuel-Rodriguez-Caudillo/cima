# VERIFICACIÓN Y CORRECCIÓN .gitignore - CRÍTICO

## ESTADO ACTUAL

### Problema Identificado
**SEVERIDAD: CRÍTICA**

El archivo `src/cima.DbMigrator/appsettings.secrets.json` contiene credenciales en texto plano y **está subido al repositorio Git**.

**Contenido expuesto:**
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=cima;Username=cima_app;Password=cima_dev"
  }
}
```

**Archivos sensibles en Git:**
- `src/cima.DbMigrator/appsettings.secrets.json` (CONTIENE CREDENCIALES)
- `test/cima.HttpApi.Client.ConsoleTestApp/appsettings.secrets.json` (vacío)
- `test/cima.TestBase/appsettings.secrets.json` (vacío)

---

## ACCIONES APLICADAS

### 1. .gitignore Actualizado

Se agregaron las siguientes reglas críticas al `.gitignore`:

```gitignore
###################
# SECRETS & CREDENTIALS - CRITICO
###################
# User Secrets (NO subir a Git)
**/appsettings.secrets.json
**/appsettings.*.secrets.json
secrets.json
*.secrets.json

# OpenIddict certificates
openiddict.pfx

# Environment variables
.env
.env.local
.env.development
.env.production
.env.*.local

# Connection strings personalizadas
appsettings.Development.json.user
appsettings.Production.json.user

# User Secrets directory
**/UserSecrets/
secrets/

###################
# ABP FRAMEWORK SPECIFIC
###################
*.abp.json
.abp/
.abpstudio/
**/Logs/*.txt
**/Logs/*.log

###################
# DOCKER COMPOSE OVERRIDES
###################
docker-compose.override.yml
docker-compose.*.override.yml

###################
# DATABASE BACKUPS
###################
*.sql.bak
*.backup
*.dump
```

---

## ACCIONES REQUERIDAS INMEDIATAMENTE

### OPCIÓN A: Eliminar del Repositorio (Recomendado)

```powershell
# 1. Eliminar archivos del staging area
git rm --cached src/cima.DbMigrator/appsettings.secrets.json
git rm --cached test/cima.HttpApi.Client.ConsoleTestApp/appsettings.secrets.json
git rm --cached test/cima.TestBase/appsettings.secrets.json

# 2. Commit los cambios
git add .gitignore
git commit -m "fix(security): eliminar archivos secrets del repositorio y actualizar gitignore

- Eliminados appsettings.secrets.json del repositorio
- Agregadas reglas gitignore para secrets, .env, pfx
- Agregadas reglas para ABP Framework y Docker
- IMPORTANTE: Usar User Secrets para credenciales

BREAKING CHANGE: Credenciales expuestas anteriormente deben ser rotadas"

# 3. Push al repositorio
git push origin master
```

### OPCIÓN B: Eliminar del Historial Completo (MÁS SEGURO)

**ADVERTENCIA:** Esto reescribe el historial de Git. Solo hacer si el repositorio es privado o no tiene colaboradores.

```powershell
# Usar BFG Repo-Cleaner o git filter-branch
# 1. Instalar BFG
# scoop install bfg
# o descargar desde https://rtyley.github.io/bfg-repo-cleaner/

# 2. Limpiar archivos del historial
bfg --delete-files appsettings.secrets.json

# 3. Limpiar referencias
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 4. Force push (PELIGROSO - solo si estás seguro)
git push origin --force --all
```

---

## ROTACIÓN DE CREDENCIALES

**CRÍTICO:** Las credenciales expuestas deben ser cambiadas inmediatamente:

### PostgreSQL Docker

```powershell
# 1. Conectar al contenedor
docker exec -it cima-postgres psql -U postgres

# 2. Cambiar contraseña del usuario
ALTER USER cima_app WITH PASSWORD 'NUEVA_CONTRASEÑA_SEGURA';

# 3. Salir
\q
```

### Actualizar User Secrets

```powershell
# DbMigrator
cd src/cima.DbMigrator
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=cima;User Id=cima_app;Password=NUEVA_CONTRASEÑA_SEGURA;Pooling=true;Maximum Pool Size=100;"

# Blazor
cd ../cima.Blazor
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=cima;User Id=cima_app;Password=NUEVA_CONTRASEÑA_SEGURA;Pooling=true;Maximum Pool Size=100;"
```

---

## VERIFICACIÓN POST-CORRECCIÓN

### 1. Verificar que archivos secrets no están en Git

```powershell
# No debe retornar ningún archivo
git ls-files | Select-String -Pattern "secrets|\.env|\.pfx"
```

**Resultado esperado:** Solo `etc/scripts/setup-user-secrets.ps1`

### 2. Verificar .gitignore funciona

```powershell
# Crear archivo de prueba
echo "{}" > test_secret.secrets.json

# Verificar que Git lo ignora
git status

# Debe decir: "nothing to commit" o no mostrar test_secret.secrets.json

# Limpiar
Remove-Item test_secret.secrets.json
```

### 3. Verificar archivos sensibles locales

```powershell
# Verificar que estos archivos existen SOLO localmente
Get-ChildItem -Recurse -Filter "appsettings.secrets.json" | Select-Object FullName
Get-ChildItem -Recurse -Filter "*.pfx" | Select-Object FullName
Get-ChildItem -Recurse -Filter ".env" | Select-Object FullName
```

---

## PREVENCIÓN FUTURA

### 1. Pre-commit Hook (Recomendado)

Crear hook que previene commits de archivos sensibles:

```powershell
# Crear archivo .git/hooks/pre-commit
@'
#!/bin/sh
# Pre-commit hook para prevenir commits de archivos sensibles

if git diff --cached --name-only | grep -E "(secrets\.json|\.env|\.pfx)"; then
    echo "ERROR: Intento de commit de archivos sensibles detectado!"
    echo "Archivos bloqueados:"
    git diff --cached --name-only | grep -E "(secrets\.json|\.env|\.pfx)"
    echo ""
    echo "Usa User Secrets en su lugar:"
    echo "  dotnet user-secrets set \"ConnectionStrings:Default\" \"...\""
    exit 1
fi
'@ | Out-File -FilePath .git/hooks/pre-commit -Encoding ASCII

# Hacer ejecutable (Linux/Mac)
# chmod +x .git/hooks/pre-commit
```

### 2. git-secrets (Herramienta de AWS)

```powershell
# Instalar git-secrets
# https://github.com/awslabs/git-secrets

# Configurar
git secrets --install
git secrets --register-aws
git secrets --add 'password.*=.*'
git secrets --add 'connectionstring.*=.*'
```

### 3. Educación del Equipo

**Regla de oro:** NUNCA subir credenciales al repositorio

**Usar siempre:**
- User Secrets para desarrollo local
- Variables de entorno para CI/CD
- Azure Key Vault / AWS Secrets Manager para producción

---

## CHECKLIST FINAL

```
.gitignore:
[x] Reglas para *.secrets.json agregadas
[x] Reglas para .env agregadas
[x] Reglas para *.pfx agregadas
[x] Reglas para ABP Framework agregadas
[x] Reglas para Docker Compose agregadas

Archivos sensibles:
[ ] Eliminados del repositorio Git (PENDIENTE - EJECUTAR OPCIÓN A o B)
[ ] Credenciales rotadas (PENDIENTE)
[ ] User Secrets actualizados con nuevas credenciales (PENDIENTE)

Verificación:
[ ] git ls-files no muestra archivos sensibles
[ ] .gitignore funciona correctamente
[ ] Pre-commit hook instalado (OPCIONAL)

Documentación:
[x] Este archivo creado
[ ] Equipo notificado sobre cambio de credenciales (si aplica)
```

---

## COMANDOS RÁPIDOS

```powershell
# 1. Eliminar archivos sensibles del repo
git rm --cached src/cima.DbMigrator/appsettings.secrets.json
git rm --cached test/cima.HttpApi.Client.ConsoleTestApp/appsettings.secrets.json
git rm --cached test/cima.TestBase/appsettings.secrets.json

# 2. Commit y push
git add .gitignore
git commit -m "fix(security): eliminar secrets del repo y actualizar gitignore"
git push origin master

# 3. Rotar credenciales PostgreSQL
docker exec -it cima-postgres psql -U postgres -c "ALTER USER cima_app WITH PASSWORD 'NUEVA_PASSWORD';"

# 4. Actualizar User Secrets
cd src/cima.DbMigrator
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=cima;User Id=cima_app;Password=NUEVA_PASSWORD;Pooling=true;Maximum Pool Size=100;"
cd ../cima.Blazor
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=cima;User Id=cima_app;Password=NUEVA_PASSWORD;Pooling=true;Maximum Pool Size=100;"
```

---

**URGENCIA:** ALTA  
**IMPACTO:** CRÍTICO  
**ACCIÓN REQUERIDA:** Inmediata  
**TIEMPO ESTIMADO:** 15 minutos

