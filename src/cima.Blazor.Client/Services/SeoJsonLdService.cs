using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using cima.Listings;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Servicio para generar datos estructurados JSON-LD para SEO
/// </summary>
public class SeoJsonLdService
{
    /// <summary>
    /// Genera JSON-LD para una propiedad inmobiliaria
    /// </summary>
    public string GeneratePropertyJsonLd(ListingDto listing, string baseUrl)
    {
        var jsonLd = new
        {
            @context = "https://schema.org",
            @type = "RealEstateListing",
            name = listing.Title,
            description = listing.Description ?? "",
            url = $"{baseUrl}/properties/{listing.Id}",
            image = listing.Images?.FirstOrDefault()?.Url ?? "",
            address = new
            {
                @type = "PostalAddress",
                addressLocality = listing.Location ?? "",
                addressCountry = "MX"
            },
            offers = new
            {
                @type = "Offer",
                price = listing.Price,
                priceCurrency = "MXN",
                availability = "https://schema.org/InStock"
            },
            numberOfRooms = listing.Bedrooms,
            numberOfBathroomsTotal = listing.Bathrooms,
            floorSize = listing.ConstructionArea > 0 ? new
            {
                @type = "QuantitativeValue",
                value = listing.ConstructionArea,
                unitCode = "MTK"
            } : null,
            lotSize = listing.LandArea > 0 ? new
            {
                @type = "QuantitativeValue",
                value = listing.LandArea,
                unitCode = "MTK"
            } : null
        };

        return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions 
        { 
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Genera JSON-LD para la organización (página principal)
    /// </summary>
    public string GenerateOrganizationJsonLd(string baseUrl)
    {
        var jsonLd = new
        {
            @context = "https://schema.org",
            @type = "RealEstateAgent",
            name = "4cima",
            description = "Propiedades exclusivas con arquitectura de vanguardia en México",
            url = baseUrl,
            logo = $"{baseUrl}/images/logo/4cima - B&W.png",
            sameAs = new[]
            {
                "https://www.facebook.com/4cima",
                "https://www.instagram.com/4cima"
            },
            address = new
            {
                @type = "PostalAddress",
                addressCountry = "MX"
            },
            areaServed = new
            {
                @type = "Country",
                name = "México"
            }
        };

        return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions { WriteIndented = false });
    }

    /// <summary>
    /// Genera JSON-LD para lista de propiedades (búsqueda)
    /// </summary>
    public string GeneratePropertyListJsonLd(IEnumerable<ListingDto> listings, string baseUrl, int totalCount)
    {
        var listingsList = listings.ToList();
        var jsonLd = new
        {
            @context = "https://schema.org",
            @type = "ItemList",
            name = "Propiedades disponibles en 4cima",
            numberOfItems = totalCount,
            itemListElement = listingsList.Select((l, i) => new
            {
                @type = "ListItem",
                position = i + 1,
                url = $"{baseUrl}/properties/{l.Id}",
                name = l.Title
            }).ToArray()
        };

        return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions { WriteIndented = false });
    }

    /// <summary>
    /// Genera JSON-LD para breadcrumbs
    /// </summary>
    public string GenerateBreadcrumbJsonLd(params (string name, string url)[] items)
    {
        var itemsList = items.ToList();
        var jsonLd = new
        {
            @context = "https://schema.org",
            @type = "BreadcrumbList",
            itemListElement = itemsList.Select((item, i) => new
            {
                @type = "ListItem",
                position = i + 1,
                name = item.name,
                item = item.url
            }).ToArray()
        };

        return JsonSerializer.Serialize(jsonLd, new JsonSerializerOptions { WriteIndented = false });
    }
}
