using AutoMapper;
using cima.Domain.Entities;
using cima.Domain.Shared.Dtos;

namespace cima;

public class cimaApplicationAutoMapperProfile : Profile
{
    public cimaApplicationAutoMapperProfile()
    {
        // Listings
        CreateMap<Listing, ListingDto>();
        CreateMap<Listing, ListingListDto>();
        CreateMap<CreateUpdateListingDto, Listing>();
        CreateMap<ListingImage, ListingImageDto>();

        // Contact Requests
        CreateMap<ContactRequest, ContactRequestDto>();
        CreateMap<CreateContactRequestDto, ContactRequest>();

        // Architects
        CreateMap<Architect, ArchitectDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore()); // Se carga manualmente
        CreateMap<CreateArchitectDto, Architect>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
        CreateMap<UpdateArchitectDto, Architect>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
    }
}
