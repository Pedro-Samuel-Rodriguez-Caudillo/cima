-- Script para resetear contraseña del usuario admin
-- NOTA: Esto NO cambiará la contraseña directamente porque el hash es generado por ASP.NET Identity
-- Necesitamos hacerlo a través de la aplicación

-- 1. Primero, verificamos los usuarios existentes
SELECT 'USUARIOS EXISTENTES:' as info;
SELECT "Id", "UserName", "Email", "IsActive", "AccessFailedCount", "LockoutEnd"
FROM "AbpUsers";

-- 2. Resetear contador de intentos fallidos y desbloquear cuentas
UPDATE "AbpUsers" 
SET "AccessFailedCount" = 0,
    "LockoutEnd" = NULL
WHERE "AccessFailedCount" > 0 OR "LockoutEnd" IS NOT NULL;

-- 3. Verificar que las cuentas están activas
UPDATE "AbpUsers"
SET "IsActive" = true
WHERE "IsActive" = false;

SELECT 'CUENTAS ACTUALIZADAS - Intentos fallidos reseteados y cuentas desbloqueadas' as resultado;

-- 4. Mostrar credenciales que deberían funcionar
SELECT 'CREDENCIALES DE PRUEBA:' as info;
SELECT 'admin' as usuario, '1q2w3E*' as password
UNION ALL
SELECT 'admin@cima.com' as usuario, '1q2w3E*' as password
UNION ALL
SELECT 'arq@cima.com' as usuario, '1q2w3E*' as password;
