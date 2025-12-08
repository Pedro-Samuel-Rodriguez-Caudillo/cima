using AutoMapper;
using cima.Architects;
using cima.ContactRequests;
using cima.Domain.Entities;
using cima.Listings;
using System.Linq;
using System.Text.Json;

namespace cima;

public class cimaApplicationAutoMapperProfile : Profile
{
    public cimaApplicationAutoMapperProfile()
    {
        // Listings con relaciones completas
        CreateMap<Listing, ListingDto>()
            .ForMember(dest => dest.Architect, opt => opt.MapFrom(src => src.Architect))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.LandArea)) // Map Area
            .ForMember(dest => dest.CoverImage, opt => opt.MapFrom(src => 
                 src.Images != null && src.Images.Any() 
                    ? src.Images.FirstOrDefault(i => i.PreviousImageId == null)
                    : null)) // Map CoverImage
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.Location) ? null : JsonSerializer.Deserialize<LocationDto>(src.Location, (JsonSerializerOptions?)null)));
            
        CreateMap<Listing, ListingListDto>()
            .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src => 
                src.Images != null && src.Images.Any() 
                    ? src.Images.FirstOrDefault(i => i.PreviousImageId == null)
                    : null))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                string.IsNullOrEmpty(src.Location) ? null : JsonSerializer.Deserialize<LocationDto>(src.Location, (JsonSerializerOptions?)null)));
                    
        CreateMap<CreateUpdateListingDto, Listing>()
            .ForMember(dest => dest.Images, opt => opt.Ignore());
            
        // ListingImage - mapeo con lista enlazada
        CreateMap<ListingImage, ListingImageDto>()
            .ForMember(dest => dest.PreviousImageId, opt => opt.MapFrom(src => src.PreviousImageId))
            .ForMember(dest => dest.NextImageId, opt => opt.MapFrom(src => src.NextImageId))
            .ForMember(dest => dest.ImageId, opt => opt.MapFrom(src => src.ImageId));

        // Featured Listings
        CreateMap<FeaturedListing, FeaturedListingDto>()
            .ForMember(dest => dest.Listing, opt => opt.MapFrom(src => src.Listing));

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
