-- Verificar grant types del cliente Swagger
SELECT "ClientId", "Permissions"
FROM "OpenIddictApplications"
WHERE "ClientId" = 'cima_Swagger';
