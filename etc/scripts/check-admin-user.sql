-- Script para verificar usuario admin y estado de autenticación

-- 1. Verificar si existe el usuario admin
SELECT 'VERIFICANDO USUARIO ADMIN' as step;

SELECT 
    "UserName",
    "Email",
    "IsActive",
    "LockoutEnabled",
    "LockoutEnd",
    "EmailConfirmed",
    "AccessFailedCount",
    "PasswordHash" IS NOT NULL as "HasPassword"
FROM "AbpUsers"
WHERE "UserName" = 'admin' OR "NormalizedUserName" = 'ADMIN';

-- 2. Verificar rol admin
SELECT 'VERIFICANDO ROL ADMIN' as step;

SELECT "Name", "NormalizedName", "IsStatic"
FROM "AbpRoles"
WHERE "NormalizedName" = 'ADMIN';

-- 3. Verificar asignación de rol al usuario admin
SELECT 'VERIFICANDO ASIGNACION DE ROL' as step;

SELECT u."UserName", r."Name" as "RoleName"
FROM "AbpUsers" u
JOIN "AbpUserRoles" ur ON u."Id" = ur."UserId"
JOIN "AbpRoles" r ON ur."RoleId" = r."Id"
WHERE u."UserName" = 'admin';

-- 4. Verificar clientes OpenIddict
SELECT 'VERIFICANDO CLIENTES OPENIDDICT' as step;

SELECT "ClientId", "ClientType", "ConsentType"
FROM "OpenIddictApplications";

-- 5. Verificar permisos del rol admin
SELECT 'VERIFICANDO PERMISOS ADMIN' as step;

SELECT COUNT(*) as "TotalPermisos"
FROM "AbpPermissionGrants"
WHERE "ProviderName" = 'R' AND "ProviderKey" = 'admin';

-- 6. Verificar si hay intentos fallidos de login
SELECT 'VERIFICANDO SECURITY LOGS RECIENTES' as step;

SELECT "Action", "UserName", "CreationTime", "ClientIpAddress"
FROM "AbpSecurityLogs"
ORDER BY "CreationTime" DESC
LIMIT 10;
