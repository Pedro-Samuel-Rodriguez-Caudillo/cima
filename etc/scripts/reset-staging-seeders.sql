-- =====================================================
-- SCRIPT: reset-staging-seeders.sql
-- DESCRIPCION: Limpia los datos de seeding corruptos en staging
-- IMPORTANTE: Ejecutar este script contra la BD de staging
-- =====================================================

-- PASO 1: Deshabilitar restricciones de FK temporalmente
SET session_replication_role = 'replica';

-- =====================================================
-- LIMPIAR DATOS DE APLICACION (en orden de dependencias)
-- =====================================================

-- Limpiar Contact Requests
TRUNCATE TABLE "ContactRequests" CASCADE;
SELECT 'ContactRequests limpiado' AS status;

-- Limpiar Property Images
TRUNCATE TABLE "PropertyImages" CASCADE;
SELECT 'PropertyImages limpiado' AS status;

-- Limpiar Properties (Listings)
TRUNCATE TABLE "Properties" CASCADE;
SELECT 'Properties limpiado' AS status;

-- Limpiar Architects
TRUNCATE TABLE "Architects" CASCADE;
SELECT 'Architects limpiado' AS status;

-- Limpiar Featured Listings (si existe)
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'FeaturedListings') THEN
        TRUNCATE TABLE "FeaturedListings" CASCADE;
        RAISE NOTICE 'FeaturedListings limpiado';
    END IF;
END $$;

-- Limpiar Listings (si existe - nombre nuevo)
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Listings') THEN
        TRUNCATE TABLE "Listings" CASCADE;
        RAISE NOTICE 'Listings limpiado';
    END IF;
END $$;

-- =====================================================
-- LIMPIAR OPENIDDICT (tokens, autorizaciones, apps)
-- =====================================================

-- Limpiar tokens primero
TRUNCATE TABLE "OpenIddictTokens" CASCADE;
SELECT 'OpenIddictTokens limpiado' AS status;

-- Limpiar autorizaciones
TRUNCATE TABLE "OpenIddictAuthorizations" CASCADE;
SELECT 'OpenIddictAuthorizations limpiado' AS status;

-- Limpiar aplicaciones (clientes OAuth)
TRUNCATE TABLE "OpenIddictApplications" CASCADE;
SELECT 'OpenIddictApplications limpiado' AS status;

-- Limpiar scopes
TRUNCATE TABLE "OpenIddictScopes" CASCADE;
SELECT 'OpenIddictScopes limpiado' AS status;

-- =====================================================
-- LIMPIAR IDENTITY (usuarios, roles, permisos)
-- =====================================================

-- Limpiar relaciones de usuario
TRUNCATE TABLE "AbpUserTokens" CASCADE;
TRUNCATE TABLE "AbpUserRoles" CASCADE;
TRUNCATE TABLE "AbpUserOrganizationUnits" CASCADE;
TRUNCATE TABLE "AbpUserLogins" CASCADE;
TRUNCATE TABLE "AbpUserClaims" CASCADE;
SELECT 'Relaciones de usuario limpiadas' AS status;

-- Limpiar usuarios (excepto datos del sistema si los hay)
DELETE FROM "AbpUsers";
SELECT 'AbpUsers limpiado' AS status;

-- Limpiar relaciones de roles
TRUNCATE TABLE "AbpRoleClaims" CASCADE;
TRUNCATE TABLE "AbpOrganizationUnitRoles" CASCADE;
SELECT 'Relaciones de roles limpiadas' AS status;

-- Limpiar roles
DELETE FROM "AbpRoles";
SELECT 'AbpRoles limpiado' AS status;

-- Limpiar permisos
TRUNCATE TABLE "AbpPermissionGrants" CASCADE;
SELECT 'AbpPermissionGrants limpiado' AS status;

-- =====================================================
-- LIMPIAR SESIONES Y SEGURIDAD
-- =====================================================

TRUNCATE TABLE "AbpSessions" CASCADE;
TRUNCATE TABLE "AbpSecurityLogs" CASCADE;
SELECT 'Sesiones y logs de seguridad limpiados' AS status;

-- =====================================================
-- LIMPIAR CONFIGURACIONES (opcional - descomentar si necesario)
-- =====================================================

-- TRUNCATE TABLE "AbpSettings" CASCADE;
-- TRUNCATE TABLE "AbpSettingDefinitions" CASCADE;
-- SELECT 'Settings limpiados' AS status;

-- =====================================================
-- LIMPIAR AUDIT LOGS (opcional - puede ser mucho dato)
-- =====================================================

-- Limpiar audit logs si hay muchos datos corruptos
-- TRUNCATE TABLE "AbpAuditLogActions" CASCADE;
-- TRUNCATE TABLE "AbpEntityPropertyChanges" CASCADE;
-- TRUNCATE TABLE "AbpEntityChanges" CASCADE;
-- TRUNCATE TABLE "AbpAuditLogs" CASCADE;
-- SELECT 'Audit logs limpiados' AS status;

-- =====================================================
-- RESTAURAR RESTRICCIONES
-- =====================================================

SET session_replication_role = 'origin';

-- =====================================================
-- VERIFICACION FINAL
-- =====================================================

SELECT 'LIMPIEZA COMPLETADA' AS resultado;
SELECT '====================';
SELECT 'Ahora redeploya la aplicacion para que ejecute el seeding automatico' AS instruccion;

-- Mostrar conteo de registros
SELECT 'Verificacion de tablas vacias:' AS info;
SELECT 
    (SELECT COUNT(*) FROM "AbpUsers") AS usuarios,
    (SELECT COUNT(*) FROM "AbpRoles") AS roles,
    (SELECT COUNT(*) FROM "OpenIddictApplications") AS oauth_apps,
    (SELECT COUNT(*) FROM "Properties") AS propiedades,
    (SELECT COUNT(*) FROM "Architects") AS arquitectos;
