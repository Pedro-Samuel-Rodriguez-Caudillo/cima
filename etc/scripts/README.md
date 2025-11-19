# Scripts PowerShell - CIMA

Este directorio contiene scripts de automatización para el proyecto CIMA.

## Scripts Disponibles

### ??? Base de Datos

| Script | Propósito | Uso |
|--------|-----------|-----|
| `setup-postgres-docker.ps1` | Configura contenedor PostgreSQL en Docker | `.\etc\scripts\setup-postgres-docker.ps1` |
| `reset-database.ps1` | Elimina y recrea la BD con migraciones | `.\etc\scripts\reset-database.ps1` |
| `actualizar-migraciones.ps1` | Crea/aplica migraciones EF Core | `.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "MiCambio"` |

### ?? Configuración

| Script | Propósito | Uso |
|--------|-----------|-----|
| `setup-user-secrets.ps1` | Configura User Secrets en proyectos | `.\etc\scripts\setup-user-secrets.ps1` |

### ?? Pruebas

| Script | Propósito | Uso |
|--------|-----------|-----|
| `test-api.ps1` | Prueba todos los endpoints de la API | `.\etc\scripts\test-api.ps1` |
| `diagnostico-api.ps1` | Diagnóstico rápido de la API | `.\etc\scripts\diagnostico-api.ps1` |
| `diagnostico-detallado.ps1` | Diagnóstico detallado del sistema | `.\etc\scripts\diagnostico-detallado.ps1` |

### ?? Logs

| Script | Propósito | Uso |
|--------|-----------|-----|
| `ver-logs.ps1` | Visualiza logs del sistema | `.\etc\scripts\ver-logs.ps1` |
| `limpiar-logs.ps1` | Limpia archivos de log | `.\etc\scripts\limpiar-logs.ps1` |

### ?? Frontend (Tailwind CSS)

| Script | Propósito | Uso |
|--------|-----------|-----|
| `build-tailwind.ps1` | Compila CSS para producción (minificado) | `.\etc\scripts\build-tailwind.ps1` |
| `start-tailwind-watch.ps1` | Inicia Tailwind en modo watch (desarrollo) | `.\etc\scripts\start-tailwind-watch.ps1` |

## Guías de Uso

### Setup Inicial (Primera Vez)

```powershell
# 1. Configurar PostgreSQL en Docker
.\etc\scripts\setup-postgres-docker.ps1

# 2. Configurar User Secrets
.\etc\scripts\setup-user-secrets.ps1

# 3. Crear y aplicar migraciones
.\etc\scripts\reset-database.ps1

# 4. Compilar Tailwind CSS
.\etc\scripts\build-tailwind.ps1
```

### Desarrollo Diario

```powershell
# Iniciar Tailwind en modo watch (en una terminal)
.\etc\scripts\start-tailwind-watch.ps1

# Ver logs de la aplicación (en otra terminal)
.\etc\scripts\ver-logs.ps1

# Probar API después de cambios
.\etc\scripts\test-api.ps1

# Verificar estado del sistema
.\etc\scripts\diagnostico-api.ps1
```

### Actualizar Modelo de Datos

```powershell
# Después de modificar entidades en Domain
# 1. Crear migración
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarCampoX"

# 2. Aplicar a BD
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarCampoX" -Aplicar

# O en un solo paso
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarCampoX" -Aplicar
```

### Resetear Todo (Desarrollo)

```powershell
# Elimina BD, crea nueva, aplica migraciones y seeders
.\etc\scripts\reset-database.ps1
```

## Detalles de Scripts

### setup-postgres-docker.ps1

Configura un contenedor PostgreSQL 16 con:
- Base de datos: `cima_db`
- Usuario: `cima_user` / `cima_pass`
- Puerto: `5432`
- Volumen persistente

**Parámetros:**
- `-ContainerName`: Nombre del contenedor (default: `cima-postgres`)
- `-Port`: Puerto host (default: `5432`)
- `-DbName`: Nombre de BD (default: `cima_db`)

### reset-database.ps1

Proceso completo:
1. Verifica Docker
2. Elimina BD existente
3. Crea BD nueva
4. Ejecuta DbMigrator (migraciones + seeders)

**Sin parámetros necesarios**

### actualizar-migraciones.ps1

Gestión completa de migraciones EF Core:
1. Detecta cambios en el modelo
2. Crea nueva migración
3. Opcionalmente aplica a BD

**Parámetros:**
- `-NombreMigracion`: Nombre de la migración (ej: "AgregarCampoX")
- `-Aplicar`: Aplica la migración inmediatamente
- `-Limpiar`: Elimina todas las migraciones existentes

**Ejemplos:**
```powershell
# Solo crear migración
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarDescripcion"

# Crear y aplicar
.\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "AgregarDescripcion" -Aplicar

# Limpiar todo y recrear
.\etc\scripts\actualizar-migraciones.ps1 -Limpiar -NombreMigracion "InitialCreate" -Aplicar
```

### setup-user-secrets.ps1

Configura connection strings y secretos en:
- `src/cima.Blazor`
- `src/cima.DbMigrator`

**Sin parámetros necesarios**

### test-api.ps1

Ejecuta 20 pruebas cubriendo:
- Autenticación (token)
- CRUD de Architects
- CRUD de Listings
- CRUD de ContactRequests

**Parámetros:**
- `-BaseUrl`: URL de la API (default: `https://localhost:44350`)
- `-Username`: Usuario (default: `admin`)
- `-Password`: Contraseña (default: `1q2w3E*`)

### ver-logs.ps1

Muestra logs con colores:
- Rojo: Errores
- Amarillo: Warnings
- Verde: Info
- Gris: Debug

**Parámetros:**
- `-Proyecto`: `blazor`, `migrator`, `all` (default: `all`)
- `-Lineas`: Número de líneas a mostrar (default: `50`)

**Ejemplos:**
```powershell
# Ver todos los logs
.\etc\scripts\ver-logs.ps1

# Solo Blazor, 100 líneas
.\etc\scripts\ver-logs.ps1 -Proyecto blazor -Lineas 100
```

### limpiar-logs.ps1

Limpia archivos de log y crea backups automáticos si > 100KB.

**Sin parámetros necesarios**

### build-tailwind.ps1

Compila el CSS de Tailwind para producción. Asegúrate de tener configurado el archivo `tailwind.config.js` correctamente.

**Sin parámetros necesarios**

### start-tailwind-watch.ps1

Inicia el proceso de Tailwind en modo desarrollo. Observa cambios en los archivos y recompila automáticamente.

**Sin parámetros necesarios**

## Archivos SQL

### create-db-user.sql

Script SQL para crear manualmente el usuario de BD si es necesario.

**Uso:**
```powershell
# Desde PostgreSQL
docker exec -i cima-postgres psql -U postgres < etc\scripts\create-db-user.sql
```

## Orden de Ejecución Recomendado

### Primera Vez (Setup Completo)

```powershell
1. .\etc\scripts\setup-postgres-docker.ps1
2. .\etc\scripts\setup-user-secrets.ps1
3. .\etc\scripts\reset-database.ps1
4. .\etc\scripts\test-api.ps1
```

### Después de Cambios en Entidades

```powershell
1. Modificar entidades en src/cima.Domain/Entities/
2. .\etc\scripts\actualizar-migraciones.ps1 -NombreMigracion "MiCambio" -Aplicar
3. .\etc\scripts\test-api.ps1
```

### Debugging

```powershell
1. .\etc\scripts\ver-logs.ps1
2. .\etc\scripts\diagnostico-detallado.ps1
3. Si hay problemas: .\etc\scripts\reset-database.ps1
```

## Troubleshooting

### Docker no está corriendo

```powershell
# Iniciar Docker Desktop manualmente
# Luego ejecutar el script nuevamente
```

### Contenedor PostgreSQL no existe

```powershell
.\etc\scripts\setup-postgres-docker.ps1
```

### Errores de migración

```powershell
# Ver logs detallados
.\etc\scripts\ver-logs.ps1 -Proyecto migrator

# Resetear todo
.\etc\scripts\reset-database.ps1
```

### API no responde

```powershell
# Diagnóstico
.\etc\scripts\diagnostico-api.ps1

# Ver logs
.\etc\scripts\ver-logs.ps1 -Proyecto blazor
```

## Documentación Relacionada

- **Migraciones EF Core**: `docs/GUIA_MIGRACIONES_EF.md`
- **Sistema de Logging**: `docs/SISTEMA_LOGGING.md`
- **Solución de Permisos**: `docs/SOLUCION_PERMISOS_API.md`
- **Configuración Secrets**: `docs/CONFIGURACION_USER_SECRETS_COMPLETADA.md`

## Notas Importantes

1. **Permisos**: Todos los scripts requieren PowerShell con permisos de ejecución
2. **Docker**: Debe estar instalado y corriendo para scripts de BD
3. **.NET SDK**: Debe estar instalado para comandos `dotnet`
4. **Ruta**: Ejecutar siempre desde la raíz del proyecto

## Convenciones

- Scripts en español
- PascalCase para nombres de archivo
- Parámetros con `-NombreParametro`
- Logs con colores (Verde=OK, Amarillo=Warning, Rojo=Error)
