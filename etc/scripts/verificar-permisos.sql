-- Verificar permisos del rol admin
SELECT COUNT(*) as total_permisos 
FROM "AbpPermissionGrants" 
WHERE "ProviderName" = 'R' AND "ProviderKey" = 'admin';

-- Listar permisos de CIMA asignados
SELECT "Name" 
FROM "AbpPermissionGrants" 
WHERE "ProviderName" = 'R' 
  AND "ProviderKey" = 'admin'
  AND "Name" LIKE 'cima%'
ORDER BY "Name";
