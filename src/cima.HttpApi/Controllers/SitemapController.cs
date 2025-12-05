using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using cima.Listings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cima.Controllers;

/// <summary>
/// Controlador para generar sitemap.xml dinámico
/// Mejora SEO al listar todas las URLs públicas del sitio
/// </summary>
[ApiController]
[Route("sitemap.xml")]
public class SitemapController : ControllerBase
{
    private readonly IListingAppService _listingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SitemapController(
        IListingAppService listingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _listingService = listingService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [Produces("application/xml")]
    public async Task<IActionResult> Get()
    {
        var baseUrl = GetBaseUrl();
        var urls = new List<SitemapUrl>();

        // Páginas estáticas
        urls.Add(new SitemapUrl(baseUrl, DateTime.UtcNow, ChangeFrequency.Daily, 1.0));
        urls.Add(new SitemapUrl($"{baseUrl}/properties", DateTime.UtcNow, ChangeFrequency.Daily, 0.9));
        urls.Add(new SitemapUrl($"{baseUrl}/portfolio", DateTime.UtcNow, ChangeFrequency.Weekly, 0.8));
        urls.Add(new SitemapUrl($"{baseUrl}/propiedades", DateTime.UtcNow, ChangeFrequency.Daily, 0.9));

        // Obtener propiedades publicadas mediante el servicio
        try
        {
            var publishedListings = await _listingService.GetListAsync(new GetListingsInput
            {
                MaxResultCount = 1000,
                SkipCount = 0
            });

            foreach (var listing in publishedListings.Items)
            {
                // Usar fecha actual si no hay información de modificación
                var lastMod = DateTime.UtcNow;
                urls.Add(new SitemapUrl(
                    $"{baseUrl}/properties/{listing.Id}",
                    lastMod,
                    ChangeFrequency.Weekly,
                    0.7
                ));
                // URL alternativa en español
                urls.Add(new SitemapUrl(
                    $"{baseUrl}/propiedades/{listing.Id}",
                    lastMod,
                    ChangeFrequency.Weekly,
                    0.7
                ));
            }
        }
        catch
        {
            // Si falla, continuar con las páginas estáticas
        }

        var sitemap = GenerateSitemapXml(urls);
        return Content(sitemap, "application/xml", Encoding.UTF8);
    }

    private string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return "https://4cima.com";
        
        return $"{request.Scheme}://{request.Host}";
    }

    private static string GenerateSitemapXml(List<SitemapUrl> urls)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        
        var sitemap = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "urlset",
                urls.Select(url => new XElement(ns + "url",
                    new XElement(ns + "loc", url.Location),
                    new XElement(ns + "lastmod", url.LastModified.ToString("yyyy-MM-dd")),
                    new XElement(ns + "changefreq", url.ChangeFrequency.ToString().ToLower()),
                    new XElement(ns + "priority", url.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture))
                ))
            )
        );

        return sitemap.Declaration + Environment.NewLine + sitemap.ToString();
    }

    private record SitemapUrl(string Location, DateTime LastModified, ChangeFrequency ChangeFrequency, double Priority);

    private enum ChangeFrequency
    {
        Always,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Never
    }
}
