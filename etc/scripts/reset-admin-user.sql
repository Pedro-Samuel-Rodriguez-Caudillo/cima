-- Ver usuarios actuales
SELECT "UserName", "Email", "IsActive" FROM "AbpUsers";

-- Eliminar usuario admin@cima.com para que se recree
DELETE FROM "AbpUserRoles" WHERE "UserId" IN (SELECT "Id" FROM "AbpUsers" WHERE "UserName" = 'admin@cima.com');
DELETE FROM "AbpUsers" WHERE "UserName" = 'admin@cima.com';

-- Verificar
SELECT "UserName", "Email" FROM "AbpUsers";
