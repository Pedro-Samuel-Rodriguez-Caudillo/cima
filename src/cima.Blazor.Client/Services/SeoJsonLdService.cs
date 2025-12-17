using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using cima.Listings;
using cima.Listings.Inputs;
using cima.Listings.Outputs;

namespace cima.Blazor.Client.Services;

public class SeoJsonLdService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string GenerateHomeJsonLd(string baseUrl)
    {
        var cleanBaseUrl = TrimTrailingSlash(baseUrl);

        var organization = new
        {
            @context = "https://schema.org",
            @type = "Organization",
            name = "4cima",
            url = cleanBaseUrl,
            logo = $"{cleanBaseUrl}/images/logo/4cima.svg"
        };

        var website = new
        {
            @context = "https://schema.org",
            @type = "WebSite",
            name = "4cima",
            url = cleanBaseUrl,
            potentialAction = new
            {
                @type = "SearchAction",
                target = $"{cleanBaseUrl}/propiedades?query={{search_term_string}}",
                query_input = "required name=search_term_string"
            }
        };

        return JsonSerializer.Serialize(new object[] { organization, website }, SerializerOptions);
    }

    public string GenerateFeaturedListJsonLd(IReadOnlyList<ListingSummaryDto> listings, string baseUrl)
    {
        if (listings is not { Count: > 0 })
        {
            return GenerateHomeJsonLd(baseUrl);
        }

        var cleanBaseUrl = TrimTrailingSlash(baseUrl);

        var schema = new
        {
            @context = "https://schema.org",
            @type = "ItemList",
            name = "Featured properties",
            itemListElement = listings
                .Select((listing, index) => new
                {
                    @type = "ListItem",
                    position = index + 1,
                    url = $"{cleanBaseUrl}/properties/{listing.Id}",
                    name = listing.Title,
                    description = listing.Title, // Summary doesn't have description
                    image = listing.MainImage != null ? new[] { listing.MainImage.Url } : Array.Empty<string>(),
                    offers = new
                    {
                        @type = "Offer",
                        priceCurrency = "MXN",
                        price = listing.Price
                    }
                })
                .ToArray()
        };

        return JsonSerializer.Serialize(schema, SerializerOptions);
    }

    public string GenerateListingJson(ListingDetailDto listing, string baseUrl)
    {
        var cleanBaseUrl = TrimTrailingSlash(baseUrl);

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
                url = $"{cleanBaseUrl}/properties/{listing.Id}",
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

        return JsonSerializer.Serialize(schema, SerializerOptions);
    }

    public string GeneratePropertyJsonLd(ListingDetailDto listing, string baseUrl)
    {
        return GenerateListingJson(listing, baseUrl);
    }

    private static string TrimTrailingSlash(string url) => url.TrimEnd('/');
}
