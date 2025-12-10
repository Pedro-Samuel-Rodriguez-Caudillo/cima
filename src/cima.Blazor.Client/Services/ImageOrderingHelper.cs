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
        var list = images?.ToList() ?? new List<ListingImageDto>();
        if (list.Count <= 1) return list;

        var map = list
            .Where(i => i.ImageId != Guid.Empty)
            .ToDictionary(i => i.ImageId, i => i);

        // Inicio: imagen sin Previous, si no existe usamos la primera.
        var start = list.FirstOrDefault(i => i.PreviousImageId == null) ?? list[0];

        var ordered = new List<ListingImageDto>();
        var visited = new HashSet<Guid>();
        var current = start;

        while (current != null && ordered.Count < list.Count)
        {
            if (!visited.Add(current.ImageId))
            {
                // Bucle defensivo: rompe para evitar ciclos corruptos.
                break;
            }

            ordered.Add(current);

            if (current.NextImageId is Guid nextId && map.TryGetValue(nextId, out var next))
            {
                current = next;
            }
            else
            {
                current = null;
            }
        }

        // Añadir cualquier imagen no visitada (huérfanas) para no perderlas.
        ordered.AddRange(list.Where(i => !visited.Contains(i.ImageId)));
        return ordered;
    }

    public static ListingImageDto? GetCover(IEnumerable<ListingImageDto>? images)
        => OrderImages(images).FirstOrDefault();

    public static string? GetCoverUrl(IEnumerable<ListingImageDto>? images)
        => GetCover(images)?.Url;
}

