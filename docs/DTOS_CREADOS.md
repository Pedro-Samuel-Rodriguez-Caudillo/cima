DTOs CREADOS - PROYECTO CIMA

Creación exitosa de todos los DTOs necesarios para el proyecto CIMA.

ARCHIVOS CREADOS EN src/cima.Domain.Shared/Dtos/

1. PropertyDto.cs
   - PropertyDto: DTO de lectura completo para Property
   - CreateUpdatePropertyDto: DTO para crear/actualizar Property
   - PropertyImageDto: DTO para imagenes de Property

2. ArchitectDto.cs
   - ArchitectDto: DTO basico para Architect
   - CreateUpdateArchitectDto: DTO para crear/actualizar Architect
   - ArchitectDetailDto: DTO con detalles completos incluyendo propiedades
   - ArchitectListDto: DTO optimizado para listados

3. ContactRequestDto.cs
   - ContactRequestDto: DTO de lectura para ContactRequest
   - CreateContactRequestDto: DTO para crear ContactRequest
   - UpdateContactRequestDto: DTO para actualizar estado de ContactRequest

4. PropertyFiltersDto.cs
   - PropertyFilterDto: DTO para filtrados y busquedas de Property
   - PropertyDetailDto: DTO con todos los detalles de una Property
   - PropertyListDto: DTO optimizado para listados de Property

5. ApiResponseDto.cs
   - ApiResponseDto<T>: Respuesta genérica para API
   - PagedResponseDto<T>: Respuesta paginada
   - CreateResultDto<T>: Resultado de creación
   - UpdateResultDto<T>: Resultado de actualización
   - DeleteResultDto: Resultado de eliminación

6. ValidationDtos.cs
   - CreatePropertyDtoValidation: Validaciones para Property
   - CreateContactRequestDtoValidation: Validaciones para ContactRequest
   - CreateArchitectDtoValidation: Validaciones para Architect

7. Enums.cs (en cima.Domain.Shared)
   - PropertyStatus: Draft, Published, Archived
   - ContactRequestStatus: New, Replied, Closed

CARACTERISTICAS:

- Todos los DTOs compilados sin errores
- Namespace correcto: cima.Domain.Shared.Dtos
- Enums duplicados en Domain.Shared para accesibilidad
- DTOs de lectura (Dto)
- DTOs de creación (CreateDto)
- DTOs de actualización (UpdateDto)
- DTOs de detalle (DetailDto)
- DTOs para listados (ListDto)
- DTOs con filtros (FilterDto)
- DTOs de respuesta API (ResponseDto)
- DTOs con validación (ValidationDto)

PROXIMOS PASOS:

1. Crear Repositories en cima.EntityFrameworkCore
2. Crear Application Services en cima.Application
3. Crear AutoMapper Profiles para mapeos
4. Crear Controllers en cima.HttpApi
5. Crear validadores con FluentValidation

NOTA: Sin emojis en código, comentarios ni logs (seguir regla REGLA_NO_EMOJIS.md)
