using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using cima.Listings;

namespace cima.Blazor.Client.Services;

public class SeoJsonLdService
{
    public string GenerateListingJson(ListingDto listing, string baseUrl)
    {
        var schema = new
        {
            @context = "https://schema.org",
            @type = "Product",
            name = listing.Title,
            description = listing.Description,
            image = listing.Images?.Select(i => i.Url).ToArray(),
            sku = listing.Id.ToString(),
            brand = new
            {
                @type = "Brand",
                name = "CIMA Inmobiliaria"
            },
            offers = new
            {
                @type = "Offer",
                url = $"{baseUrl}/properties/{listing.Id}",
                priceCurrency = "MXN",
                price = listing.Price,
                itemCondition = "https://schema.org/NewCondition",
                availability = "https://schema.org/InStock",
                areaServed = listing.Location?.ToString() ?? "México"
            },
            numberOfRooms = listing.Bedrooms,
            floorSize = new 
            {
                @type = "QuantitativeValue",
                value = listing.ConstructionArea,
                unitCode = "MTK"
            },
            address = listing.Location?.ToString() ?? "México"
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public string GeneratePropertyJsonLd(ListingDto listing, string baseUrl)
    {
        return GenerateListingJson(listing, baseUrl);
    }
}
