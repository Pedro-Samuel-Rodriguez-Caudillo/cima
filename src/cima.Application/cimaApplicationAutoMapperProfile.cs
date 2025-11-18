using AutoMapper;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Shared.Dtos;

namespace cima
{
    /// <summary>
    /// Defines AutoMapper profile mappings for application domain entities and their corresponding data transfer
    /// objects (DTOs).
    /// </summary>
    /// <remarks>This profile configures object-object mappings for entities such as Property, Architect,
    /// PropertyImage, and ContactRequest, enabling seamless conversion between domain models and DTOs for create,
    /// update, and retrieval operations. The mappings specify which properties are mapped, ignored, or set to default
    /// values, ensuring that only relevant data is transferred between layers. This profile should be registered with
    /// AutoMapper during application startup to enable automatic mapping throughout the application.</remarks>
    /// 
    /// Resumen: Mapea Entidades <--> DTOs
    public class cimaApplicationAutoMapperProfile : Profile
    {
        public cimaApplicationAutoMapperProfile()
        {
            // ==================== PROPERTY MAPPINGS ====================
            
            // Entidad -> DTO (para devolver al cliente)
            CreateMap<Property, PropertyDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
            
            // DTO -> Entidad (para crear/actualizar)
            CreateMap<CreateUpdatePropertyDto, Property>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())           // BD genera el Id
                .ForMember(dest => dest.Images, opt => opt.Ignore())       // Se manejan aparte
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())    // Se asigna manual
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Architect, opt => opt.Ignore());   // Relacion navegacion

            // ==================== PROPERTY IMAGE MAPPINGS ====================
            
            CreateMap<PropertyImage, PropertyImageDto>();
            CreateMap<PropertyImageDto, PropertyImage>();

            // ==================== ARCHITECT MAPPINGS ====================
            
            // Entidad -> DTO
            CreateMap<Architect, ArchitectDto>();
            
            // DTO -> Entidad (Create)
            CreateMap<CreateArchitectDto, Architect>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())       // Se asigna manual
                .ForMember(dest => dest.Properties, opt => opt.Ignore());
            
            // DTO -> Entidad (Update)
            CreateMap<UpdateArchitectDto, Architect>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            // ==================== CONTACT REQUEST MAPPINGS ====================
            
            // Entidad -> DTO
            CreateMap<ContactRequest, ContactRequestDto>();
            
            // DTO -> Entidad
            CreateMap<CreateContactRequestDto, ContactRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ContactRequestStatus.New)) // Siempre New al crear
                .ForMember(dest => dest.ArchitectId, opt => opt.Ignore())  // Se asigna manual
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ReplyNotes, opt => opt.Ignore())
                .ForMember(dest => dest.Property, opt => opt.Ignore())
                .ForMember(dest => dest.Architect, opt => opt.Ignore());
        }
    }
}
