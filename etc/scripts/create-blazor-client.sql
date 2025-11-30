-- Script SQL para crear cliente cima_BlazorWebApp en OpenIddict
-- Ejecutar en PostgreSQL

-- 1. Insertar la aplicación (cliente)
INSERT INTO "OpenIddictApplications" (
    "Id",
    "ApplicationType",
    "ClientId",
    "ClientSecret",
    "ClientType",
    "ConsentType",
    "DisplayName",
    "Permissions",
    "RedirectUris",
    "PostLogoutRedirectUris",
    "ClientUri",
    "LogoUri",
    "ExtraProperties",
    "ConcurrencyStamp",
    "CreationTime",
    "CreatorId",
    "LastModificationTime",
    "LastModifierId",
    "IsDeleted",
    "DeleterId",
    "DeletionTime"
)
VALUES (
    gen_random_uuid(),  -- Id
    'web',  -- ApplicationType
    'cima_BlazorWebApp',  -- ClientId
    'e4bcd65d-41a1-4e47-a5ea-ee1c3e5e5a97',  -- ClientSecret (hash de "1q2w3e*")
    'confidential',  -- ClientType
    'implicit',  -- ConsentType
    'Blazor WebApp Application',  -- DisplayName
    '[
        "ept:token",
        "ept:revocation",
        "ept:introspection",
        "ept:authorization",
        "ept:logout",
        "gt:authorization_code",
        "gt:refresh_token",
        "rst:code",
        "rst:code_id_token",
        "scp:address",
        "scp:email",
        "scp:phone",
        "scp:profile",
        "scp:roles",
        "scp:cima"
    ]'::text,  -- Permissions
    '["https://localhost:44350/signin-oidc"]'::text,  -- RedirectUris
    '["https://localhost:44350/signout-callback-oidc"]'::text,  -- PostLogoutRedirectUris
    'https://localhost:44350',  -- ClientUri
    '/images/clients/blazor.svg',  -- LogoUri
    '{}'::text,  -- ExtraProperties
    gen_random_uuid()::text,  -- ConcurrencyStamp
    NOW(),  -- CreationTime
    NULL,  -- CreatorId
    NULL,  -- LastModificationTime
    NULL,  -- LastModifierId
    FALSE,  -- IsDeleted
    NULL,  -- DeleterId
    NULL  -- DeletionTime
)
ON CONFLICT ("ClientId") DO UPDATE SET
    "Permissions" = EXCLUDED."Permissions",
    "RedirectUris" = EXCLUDED."RedirectUris",
    "PostLogoutRedirectUris" = EXCLUDED."PostLogoutRedirectUris",
    "LastModificationTime" = NOW();

-- 2. Verificar que se creó
SELECT "ClientId", "DisplayName", "ClientType", "Permissions"
FROM "OpenIddictApplications"
WHERE "ClientId" = 'cima_BlazorWebApp';
