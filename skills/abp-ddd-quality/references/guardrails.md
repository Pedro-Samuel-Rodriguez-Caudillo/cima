# ABP + DDD Guardrails (CIMA)

## Table of contents
- Layer boundaries
- Common violations
- Quality hardening checklist
- Review prompts

## Layer boundaries

### Domain
- Keep only domain logic and invariants.
- Do not depend on Application, HttpApi, Blazor, EF Core, or DbContext.
- Use aggregate roots, value objects, and domain services.
- Do not expose or depend on DTOs.

### Application
- Orchestrate use cases, mapping, validation, and authorization.
- Use repositories and domain entities.
- Return DTOs, not entities.
- Prefer ApplicationService or ICrudAppService patterns when suitable.
- Use unit of work boundaries and CancellationToken for async operations.

### HttpApi
- Keep controllers thin and delegate to application services.
- Avoid business rules or direct repository access.

### Presentation (Blazor/MudBlazor)
- Keep UI state and view models only.
- Call application services; consume DTOs.
- Do not reference domain entities, DbContext, or repositories.
- Validate with DTO annotations and UI validation.
- Handle errors at the boundary and show user friendly feedback.

## Common violations
- UI uses domain entity -> return DTOs from app services and map in the UI.
- App service returns entity -> convert to DTO using ObjectMapper.
- Domain references infrastructure types -> move infra code to EF Core or a domain service abstraction.
- UI duplicates business rules -> move rule to domain or application.

## Quality hardening checklist
- Validate inputs at the application boundary (DTO validation).
- Guard invariants in domain and raise BusinessException when violated.
- Use CancellationToken for async calls in services and repositories.
- Avoid async void; use Task or Task<T>.
- Centralize mapping with ObjectMapper.
- Avoid N+1 queries; include related data in repositories.
- Use PagedResultDto for paged lists.

## Review prompts
- Would this code work with a different persistence layer?
- Is any business rule implemented in the UI?
- Do dependencies flow inward (Domain <- Application <- Presentation)?
