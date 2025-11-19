# CIMA Script Runner

Aplicacion de consola interactiva para ejecutar scripts de automatizacion de forma amigable.

## Uso

```powershell
# Desde la raiz del proyecto
cd tools\cima.ScriptRunner
dotnet run
```

O crea un alias global:

```powershell
# Agregar al perfil de PowerShell
function cima-scripts { 
    dotnet run --project C:\Users\rodri\Documents\Inmobiliaria\cima\tools\cima.ScriptRunner 
}
```

## Caracteristicas

### Menu Interactivo
- Navegacion facil por categorias
- Ejecucion de scripts con un numero
- Parametros interactivos cuando son necesarios

### Categorias

**Base de Datos**
- Configurar PostgreSQL
- Resetear BD completa
- Actualizar migraciones

**Pruebas y Diagnostico**
- Pruebas API completas
- Diagnostico rapido
- Diagnostico detallado

**Logs**
- Ver logs (todos/blazor/migrator)
- Limpiar logs

**Verificacion**
- Verificar permisos
- Verificar cliente Swagger

## Ejemplo

```
?????????????????????????????????????????????????????
?         CIMA - SCRIPT RUNNER                      ?
?????????????????????????????????????????????????????

=== BASE DE DATOS ===
1. Configurar PostgreSQL en Docker
2. Resetear base de datos completa
3. Actualizar migraciones

=== PRUEBAS Y DIAGNOSTICO ===
4. Ejecutar pruebas API completas
5. Diagnostico rapido API
6. Diagnostico detallado

Selecciona una opcion: 4
```

## Ventajas

vs Scripts PowerShell directos:
- No necesitas recordar nombres de scripts
- No necesitas recordar parametros
- Validacion de entrada
- Feedback visual mejorado
- Cross-platform (puede adaptarse para Linux/Mac)

## Requisitos

- .NET 9 SDK
- PowerShell (para ejecutar los scripts subyacentes)
- Docker (para scripts de BD)
