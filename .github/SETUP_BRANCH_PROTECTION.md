# Configurar Protección de Branches en GitHub

Este documento te guía paso a paso para configurar las protecciones de branches según la estrategia definida.

## Acceder a la Configuración

1. Ve a tu repositorio en GitHub: https://github.com/Pedro-Samuel-Rodriguez-Caudillo/cima
2. Haz clic en **Settings** (arriba a la derecha)
3. En el menú lateral, haz clic en **Branches** (bajo "Code and automation")
4. Busca la sección **Branch protection rules**

## Configurar Branch: `main`

1. Haz clic en **Add branch protection rule**
2. En **Branch name pattern** escribe: `main`
3. Activa las siguientes opciones:

### ? Require a pull request before merging
- Marcar checkbox
- Bajo esta opción, activa:
  - **Require approvals**: Establecer en `1`
  - **Dismiss stale pull request approvals when new commits are pushed**: Marcar
  - **Require review from Code Owners**: (Opcional, si tienes CODEOWNERS)

### ? Require status checks to pass before merging
- Marcar checkbox
- Bajo esta opción, activa:
  - **Require branches to be up to date before merging**: Marcar
- En el campo de búsqueda, agregar estos checks (cuando estén disponibles):
  - `build-and-test` (del workflow CI)
  - `Build and Test` (nombre del job)

### ? Require conversation resolution before merging
- Marcar checkbox

### ? Do not allow bypassing the above settings
- Marcar checkbox
- Esto evita que incluso los admins puedan saltarse las reglas

### ? Restrict who can push to matching branches
- Marcar checkbox
- Agregar solo usuarios administradores (opcional pero recomendado)

4. Haz clic en **Create** (abajo)

## Configurar Branch: `staging`

1. Haz clic en **Add branch protection rule** nuevamente
2. En **Branch name pattern** escribe: `staging`
3. Activa las siguientes opciones:

### ? Require a pull request before merging
- Marcar checkbox
- Bajo esta opción, activa:
  - **Require approvals**: Establecer en `1`

### ? Require status checks to pass before merging
- Marcar checkbox
- Bajo esta opción, activa:
  - **Require branches to be up to date before merging**: Marcar
- Agregar checks:
  - `build-and-test`

### ? Require conversation resolution before merging
- Marcar checkbox

### ?? Allow force pushes
- **NO marcar** (solo en casos extremos con mucho cuidado)

4. Haz clic en **Create**

## Configurar Branch: `develop`

1. Haz clic en **Add branch protection rule** nuevamente
2. En **Branch name pattern** escribe: `develop`
3. Activa las siguientes opciones:

### ? Require a pull request before merging
- Marcar checkbox
- Bajo esta opción:
  - **Require approvals**: Establecer en `0` (para agilidad)
    - Esto permite merges sin aprobación, pero igual requiere PR

### ? Require status checks to pass before merging
- Marcar checkbox
- Bajo esta opción, activa:
  - **Require branches to be up to date before merging**: Marcar
- Agregar checks:
  - `build-and-test`

### ?? Allow specified actors to bypass pull request requirements
- Marcar checkbox (solo para admins en urgencias)
- Agregar tu usuario como administrador

4. Haz clic en **Create**

## Configurar Environments (Para Producción)

Si quieres agregar protecciones extra para producción:

1. Ve a **Settings** > **Environments**
2. Haz clic en **New environment**
3. Nombre: `production`
4. Haz clic en **Configure environment**

### Configuración del Environment

#### Protection rules:
- **Required reviewers**: Agregar al menos 1 reviewer
- **Wait timer**: (Opcional) Esperar X minutos antes de deploy
- **Deployment branches**: Seleccionar "Selected branches"
  - Agregar pattern: `main`

#### Environment secrets:
Agregar aquí los secretos específicos de producción:
- `PRODUCTION_HOST`
- `PRODUCTION_USER`
- `PRODUCTION_SSH_KEY`
- `PRODUCTION_SSH_PORT`

5. Haz clic en **Save protection rules**

## Verificar Configuración

Para verificar que todo está bien:

1. Intenta hacer push directo a `main`:
   ```bash
   git checkout main
   echo "test" > test.txt
   git add test.txt
   git commit -m "test"
   git push origin main
   ```
   **Resultado esperado:** Rechazado ?

2. Intenta crear PR sin que pase CI:
   - Crear PR con código que no compile
   - **Resultado esperado:** No se puede mergear ?

3. Crea PR desde `feature/test` a `develop`:
   - Debe permitir merge sin aprobación
   - Pero debe esperar a que CI pase ?

## Resumen Visual

```
???????????????????????????????????????????????????
?                    MAIN                         ?
?  ? Require PR                                   ?
?  ? Require 1 approval                           ?
?  ? Require status checks                        ?
?  ? No bypass                                    ?
?  ? Restricted push                              ?
???????????????????????????????????????????????????
                     ?
                     ? PR (con review)
                     ?
???????????????????????????????????????????????????
?                  STAGING                        ?
?  ? Require PR                                   ?
?  ? Require 1 approval                           ?
?  ? Require status checks                        ?
???????????????????????????????????????????????????
                     ?
                     ? PR
                     ?
???????????????????????????????????????????????????
?                  DEVELOP                        ?
?  ? Require PR                                   ?
?  ? Require 0 approvals (fast)                   ?
?  ? Require status checks                        ?
?  ? Admin can bypass (emergencies)               ?
???????????????????????????????????????????????????
                     ?
                     ? PR (auto-merge si CI pasa)
                     ?
        ???????????????????????????
        ?                         ?
  ???????????????         ???????????????
  ?  feature/*  ?         ?   bugfix/*  ?
  ???????????????         ???????????????
```

## Comandos Útiles

### Ver reglas de protección desde CLI
```bash
gh api repos/Pedro-Samuel-Rodriguez-Caudillo/cima/branches/main/protection
```

### Listar todos los branches protegidos
```bash
gh api repos/Pedro-Samuel-Rodriguez-Caudillo/cima/branches --paginate | jq '.[] | select(.protected == true) | .name'
```

## Solución de Problemas

### "Required status check is not in the list"
- El check debe haber corrido al menos una vez antes de poder agregarlo
- Haz push a un branch y espera a que corra el workflow
- Luego podrás seleccionar el check

### "I can't push to main"
- Correcto! Debes usar PRs
- Si realmente necesitas push directo (emergencia):
  1. Ve a Settings > Branches
  2. Edita la regla de main
  3. Temporalmente deshabilita "Do not allow bypassing"
  4. Haz tu push
  5. RE-HABILITA inmediatamente la protección

### "CI no está corriendo"
- Verifica que los workflows estén en `.github/workflows/`
- Verifica que el branch trigger esté configurado correctamente
- Revisa Actions tab para ver errores

## Referencias

- [GitHub Branch Protection Docs](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [GitHub Environments Docs](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- Estrategia de Branching: `.github/BRANCHING_STRATEGY.md`

---

**Nota:** Estas configuraciones son críticas para mantener la calidad del código. NO las deshabilites a menos que sea absolutamente necesario.
