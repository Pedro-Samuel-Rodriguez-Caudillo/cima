-- Script para agregar índices de búsqueda optimizada
-- Ejecutar en PostgreSQL después de las migraciones

-- Índice para búsqueda por estado (muy usado en filtros)
CREATE INDEX IF NOT EXISTS IX_Listings_Status ON public."CmsListings" ("Status");

-- Índice para búsqueda por ubicación (usado en autocompletado)
CREATE INDEX IF NOT EXISTS IX_Listings_Location ON public."CmsListings" ("Location");

-- Índice compuesto para búsqueda pública (Status + Location)
CREATE INDEX IF NOT EXISTS IX_Listings_Status_Location ON public."CmsListings" ("Status", "Location")
WHERE "Status" IN (1, 3); -- Published=1, Portfolio=3

-- Índice para ordenamiento por fecha (muy usado)
CREATE INDEX IF NOT EXISTS IX_Listings_CreatedAt ON public."CmsListings" ("CreatedAt" DESC);

-- Índice para ordenamiento por precio
CREATE INDEX IF NOT EXISTS IX_Listings_Price ON public."CmsListings" ("Price");

-- Índice para filtro por arquitecto
CREATE INDEX IF NOT EXISTS IX_Listings_ArchitectId ON public."CmsListings" ("ArchitectId");

-- Índice compuesto para búsqueda con filtros comunes
CREATE INDEX IF NOT EXISTS IX_Listings_Status_Type_TransactionType ON public."CmsListings" ("Status", "Type", "TransactionType")
WHERE "Status" IN (1, 3);

-- Índice para propiedades destacadas
CREATE INDEX IF NOT EXISTS IX_FeaturedListings_ListingId ON public."CmsFeaturedListings" ("ListingId");

-- Índice de texto completo para búsqueda (PostgreSQL específico)
-- Requiere extensión pg_trgm para búsqueda fuzzy
-- CREATE EXTENSION IF NOT EXISTS pg_trgm;
-- CREATE INDEX IF NOT EXISTS IX_Listings_Title_Trgm ON public."CmsListings" USING gin ("Title" gin_trgm_ops);
-- CREATE INDEX IF NOT EXISTS IX_Listings_Description_Trgm ON public."CmsListings" USING gin ("Description" gin_trgm_ops);

-- Verificar índices creados
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes 
WHERE schemaname = 'public' 
  AND tablename IN ('CmsListings', 'CmsFeaturedListings')
ORDER BY tablename, indexname;
