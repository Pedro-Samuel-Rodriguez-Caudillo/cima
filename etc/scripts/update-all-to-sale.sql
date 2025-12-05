-- Script para actualizar todas las propiedades a tipo VENTA
-- TransactionType: 0 = Sale, 1 = Rent, 2 = Lease

-- Verificar estado actual
SELECT "TransactionType", COUNT(*) as cantidad 
FROM "Listings" 
GROUP BY "TransactionType";

-- Actualizar todas a Venta (0)
UPDATE "Listings" 
SET "TransactionType" = 0 
WHERE "TransactionType" != 0;

-- Verificar resultado
SELECT "TransactionType", COUNT(*) as cantidad 
FROM "Listings" 
GROUP BY "TransactionType";

-- Mostrar resumen
SELECT 'Todas las propiedades ahora son de VENTA' as resultado;
