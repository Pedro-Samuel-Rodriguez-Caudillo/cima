using AutoMapper;
using cima.Domain.Entities;
using cima.Domain.Shared.Dtos;
using System.Linq;

namespace cima;

public class cimaApplicationAutoMapperProfile : Profile
{
    public cimaApplicationAutoMapperProfile()
    {
        // Listings con relaciones completas
        CreateMap<Listing, ListingDto>()
            .ForMember(dest => dest.Architect, opt => opt.MapFrom(src => src.Architect))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
            
        CreateMap<Listing, ListingListDto>()
            .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src => 
                src.Images != null && src.Images.Any() 
                    ? src.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault() 
                    : null));
                    
        CreateMap<CreateUpdateListingDto, Listing>()
            .ForMember(dest => dest.Images, opt => opt.Ignore());
            
        CreateMap<ListingImage, ListingImageDto>();

        // Contact Requests
        CreateMap<ContactRequest, ContactRequestDto>();
        CreateMap<CreateContactRequestDto, ContactRequest>();

        // Architects
        CreateMap<Architect, ArchitectDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore());
        CreateMap<CreateArchitectDto, Architect>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
        CreateMap<UpdateArchitectDto, Architect>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
    }
}
