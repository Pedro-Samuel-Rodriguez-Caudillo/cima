using AutoMapper;
using cima.Architects;
using cima.ContactRequests;
using cima.Domain.Entities;
using cima.Listings;
using System.Linq;
using System.Text.Json;
using Volo.Abp;

using cima.Listings.Inputs;
using cima.Listings.Outputs;
using cima.Domain.Entities.Portfolio;
using cima.Portfolio;

namespace cima;

public class cimaApplicationAutoMapperProfile : Profile
{
    private static string SerializeLocation(AddressDto input)
    {
        // Simple serialization to JSON string for now, matching how it's stored
        var locDto = new LocationDto { Address = input.Value };
        return JsonSerializer.Serialize(locDto);
    }

    private static decimal ResolvePrice(bool isPriceOnRequest, decimal? price)
    {
        if (isPriceOnRequest)
        {
            return -1;
        }

        if (!price.HasValue)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingInvalidPrice);
        }

        return price.Value;
    }
    public cimaApplicationAutoMapperProfile()
    {
        // Listings con relaciones completas
        // Listings - Output Maps
        CreateMap<Listing, ListingDetailDto>()
            .ForMember(dest => dest.Architect, opt => opt.MapFrom(src => src.Architect))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.SortOrder)))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.LandArea)) // Map Area
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => DeserializeLocation(src.Location != null ? src.Location.Value : null)))
            .ForMember(dest => dest.IsPriceOnRequest, opt => opt.MapFrom(src => src.Price == -1))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price == -1 ? (decimal?)null : src.Price));

        CreateMap<Listing, ListingSummaryDto>()
            .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src =>
                src.Images != null && src.Images.Any()
                    ? src.Images.OrderBy(i => i.SortOrder).FirstOrDefault()
                    : null))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => DeserializeLocation(src.Location != null ? src.Location.Value : null)))
            .ForMember(dest => dest.ImageCount, opt => opt.MapFrom(src => src.Images != null ? src.Images.Count : 0))
            .ForMember(dest => dest.IsPriceOnRequest, opt => opt.MapFrom(src => src.Price == -1))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price == -1 ? (decimal?)null : src.Price));

        // Listings - Input Maps
        CreateMap<CreateListingDto, Listing>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Address != null ? new Domain.Entities.Listings.Address(SerializeLocation(src.Address)) : null))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => ResolvePrice(src.IsPriceOnRequest, src.Price)));

        CreateMap<UpdateListingDto, Listing>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Address != null ? new Domain.Entities.Listings.Address(SerializeLocation(src.Address)) : null))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => ResolvePrice(src.IsPriceOnRequest, src.Price)));
            
        // ListingImage
        CreateMap<ListingImage, ListingImageDto>()
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.ImageId, opt => opt.MapFrom(src => src.ImageId))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl));

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

        // Portfolio
        CreateMap<PortfolioProject, PortfolioProjectDto>()
            .ForMember(dest => dest.Gallery, opt => opt.MapFrom(src => src.Gallery.OrderBy(i => i.SortOrder)));

        CreateMap<PortfolioImage, PortfolioImageDto>();
    }

    private static LocationDto? DeserializeLocation(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<LocationDto>(json);
        }
        catch
        {
            // Si no es JSON válido, devolver el string como Address para no perder datos
            return new LocationDto { Address = json };
        }
    }
}
