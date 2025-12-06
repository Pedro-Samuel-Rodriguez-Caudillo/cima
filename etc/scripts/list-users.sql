SELECT "UserName", "Email", "IsActive", "PasswordHash" IS NOT NULL as "HasPassword" 
FROM "AbpUsers" 
ORDER BY "UserName";
