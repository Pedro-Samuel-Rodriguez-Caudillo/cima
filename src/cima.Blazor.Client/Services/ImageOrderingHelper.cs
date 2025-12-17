using System;
using System.Collections.Generic;
using System.Linq;
using cima.Listings;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Utilidades para ordenar y obtener portada de galerías respetando la lista enlazada.
/// Centraliza la lógica para evitar inconsistencias entre vistas.
/// </summary>
public static class ImageOrderingHelper
{
    /// <summary>
    /// Devuelve las imágenes en orden según Previous/Next; agrega huérfanas al final.
    /// </summary>
    public static List<ListingImageDto> OrderImages(IEnumerable<ListingImageDto>? images)
    {
        return images?.OrderBy(i => i.SortOrder).ToList() ?? new List<ListingImageDto>();
    }

    public static ListingImageDto? GetCover(IEnumerable<ListingImageDto>? images)
        => OrderImages(images).FirstOrDefault();

    public static string? GetCoverUrl(IEnumerable<ListingImageDto>? images)
        => GetCover(images)?.Url;
}

