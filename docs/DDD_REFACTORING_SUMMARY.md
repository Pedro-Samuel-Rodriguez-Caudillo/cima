# Refactorizacion DDD - Domain Services, Events y Specifications

## Resumen Ejecutivo

Se implementaron los patrones de DDD de prioridad alta para la capa de dominio de CIMA:

| Componente | Archivos Creados | Tests |
|------------|------------------|-------|
| Domain Service | ListingManager.cs | 23 tests |
| Domain Events | ListingCreatedEto.cs, ListingStatusChangedEto.cs | 6 tests |
| Event Handler | ArchitectStatisticsHandler.cs | 12 tests |
| Specifications | 5 specifications | 28 tests |

Total: 69 tests pasando

## Estructura de Archivos Creados

```
src/cima.Domain/
  Services/
    Listings/
      IListingManager.cs
      ListingManager.cs
  Events/
    Listings/
      ListingCreatedEto.cs
      ListingStatusChangedEto.cs
  EventHandlers/
    ArchitectStatisticsHandler.cs
  Specifications/
    Listings/
      PublishedListingSpecification.cs
      PortfolioListingSpecification.cs
      PublicVisibleListingSpecification.cs
      ListingByArchitectSpecification.cs
      ListingSearchSpecification.cs

src/cima.Domain.Shared/
  cimaDomainErrorCodes.cs

test/cima.Domain.Tests/
  Services/Listings/ListingManagerTests.cs
  EventHandlers/ArchitectStatisticsHandlerTests.cs
  Specifications/Listings/ListingSpecificationsTests.cs
```

## Maquina de Estados de Listing

```
Draft ------> Published ------> Portfolio
  ^              |                  |
  |              |                  |
  +--------------+------> Archived <+
                              |
                              +------> Published
```

## Ejecutar Tests

```powershell
dotnet test test\cima.Domain.Tests --filter "FullyQualifiedName~ListingManagerTests|FullyQualifiedName~ArchitectStatisticsHandlerTests|FullyQualifiedName~ListingSpecificationsTests"
```
