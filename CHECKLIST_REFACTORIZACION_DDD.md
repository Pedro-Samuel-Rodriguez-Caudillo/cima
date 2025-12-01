# ?? CHECKLIST: Refactorización DDD

## ? Completado por GitHub Copilot

- [x] **Bounded Contexts:** Entidades organizadas en carpetas DDD
- [x] **Architect refactorizado:** Sin Name/Bio, con estadísticas
- [x] **Listing actualizado:** LandArea + ConstructionArea
- [x] **ABP Settings:** Configuración admin implementada
- [x] **Seeder híbrido:** Bogus + usuarios fijos
- [x] **Tests actualizados:** Todos compilando y pasando
- [x] **Migración creada:** RefactorDDD_BoundedContexts_AreasAndStats
- [x] **Compilación exitosa:** Sin errores
- [x] **Documentación completa:** 4 archivos MD creados

---

## ?? Tu Turno: Aplicar Cambios

### Paso 1: Revisar Migración
- [ ] Abrir `src\cima.EntityFrameworkCore\Migrations\*_RefactorDDD_BoundedContexts_AreasAndStats.cs`
- [ ] Verificar que incluye drop/add columns correcto
- [ ] Verificar rename de `Area` a `LandArea`

### Paso 2: Aplicar Migración
```bash
cd src\cima.DbMigrator
dotnet run
```
- [ ] Migración aplicada sin errores
- [ ] Seeder ejecutado correctamente
- [ ] Ver logs de ejecución

### Paso 3: Verificar Base de Datos
```sql
-- Conectar a PostgreSQL
psql -U postgres -d cima

-- Verificar Architects
\d "Architects"
-- Debe tener: UserId, TotalListingsPublished, ActiveListings, RegistrationDate, IsActive
-- NO debe tener: Name, Bio

-- Verificar Listings
\d "Listings"
-- Debe tener: LandArea, ConstructionArea
-- NO debe tener: Area

-- Ver datos
SELECT * FROM "Architects";
SELECT "Title", "LandArea", "ConstructionArea" FROM "Listings";
```
- [ ] Tabla Architects correcta
- [ ] Tabla Listings correcta
- [ ] 1 arquitecto creado
- [ ] 12 listings creados

### Paso 4: Probar Aplicación
```bash
# Terminal 1
cd src\cima.Blazor
dotnet watch run
```
- [ ] Aplicación inicia sin errores
- [ ] Navegar a https://localhost:44382

### Paso 5: Login y Verificación
**Login Admin:**
- [ ] Email: admin@cima.com
- [ ] Password: 1q2w3E*
- [ ] Acceso al panel admin

**Login Arquitecto:**
- [ ] Email: arq@cima.com
- [ ] Password: 1q2w3E*
- [ ] Dashboard de arquitecto visible

**Verificar Propiedades:**
- [ ] Ver listado de propiedades (8 publicadas)
- [ ] Abrir detalle de una propiedad
- [ ] Verificar que muestra LandArea y ConstructionArea
- [ ] Verificar imágenes genéricas de ABP

### Paso 6: Ejecutar Tests
```bash
dotnet test
```
- [ ] Todos los tests pasan
- [ ] Sin warnings de compilación

### Paso 7: Hacer Commit
```bash
git status
# Debería mostrar archivos modificados

git add .
git commit -F .git_commit_msg_refactor_ddd.txt

git push origin develop
```
- [ ] Commit realizado
- [ ] Push exitoso

---

## ?? Archivos de Referencia

1. **`RESUMEN_REFACTORIZACION_DDD.md`** - Resumen ejecutivo (lee esto primero)
2. **`REFACTORIZACION_DDD_COMPLETADA.md`** - Documentación técnica completa
3. **`APLICAR_MIGRACION_DDD.md`** - Instrucciones paso a paso
4. **Este archivo** - Checklist visual

---

## ?? Si Algo Falla

### Error en Migración:
```bash
# Ver migración creada
cd src\cima.EntityFrameworkCore
dotnet ef migrations list

# Remover última migración si es necesario
dotnet ef migrations remove --startup-project ..\cima.DbMigrator\cima.DbMigrator.csproj
```

### Error en Seeder:
- Verificar connection string en `src\cima.DbMigrator\appsettings.json`
- Verificar que PostgreSQL está corriendo
- Limpiar base de datos y re-ejecutar migrator

### Error de Compilación:
- Revisar que todos los archivos están guardados
- Hacer `dotnet clean` y `dotnet build` en solución

---

## ?? Métricas de Refactorización

| Métrica | Valor |
|---------|-------|
| Archivos modificados | ~25 |
| Archivos nuevos | ~7 |
| Tests actualizados | 5 |
| Líneas de código agregadas | ~400 |
| Líneas de código eliminadas | ~150 |
| Breaking changes | 2 (Architect, Listing) |
| Migración EF Core | 1 |
| Bounded contexts | 3 |
| ABP Settings | 2 |
| Usuarios de prueba | 2 |
| Listings de prueba | 12 |

---

## ?? Estado Final Esperado

```
? Compilación sin errores
? Tests pasando (100%)
? Migración aplicada
? Seeder ejecutado
? App corriendo
? Login funcionando
? 12 propiedades visibles
? Panel admin accesible
? Commit realizado
```

---

**?? Siguiente paso después de completar este checklist:**

Lee `docs/FEATURES_PLANIFICADAS.md` para ver las próximas features a implementar.
