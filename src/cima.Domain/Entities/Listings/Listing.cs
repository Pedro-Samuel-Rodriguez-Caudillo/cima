using System;
using System.Collections.Generic;
using System.Linq;
using cima.Domain.Entities.Listings;
using cima.Domain.Shared;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace cima.Domain.Entities;

/// <summary>
/// Agregado raíz que representa una propiedad inmobiliaria.
/// Bounded Context: Listings (Gestión de Propiedades)
/// Implementa ISoftDelete para eliminación lógica.
/// </summary>
public class Listing : AggregateRoot<Guid>, ISoftDelete
{
    // Constantes de negocio
    public const int MaxImages = 12;

    #region Propiedades
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    // Value Object Address
    public Address? Location { get; private set; }
    
    /// <summary>
    /// Precio de la propiedad. Use -1 para indicar "precio a consultar".
    /// </summary>
    public decimal Price { get; private set; }
    
    /// <summary>
    /// Área total del terreno en m²
    /// </summary>
    public decimal LandArea { get; private set; }
    
    /// <summary>
    /// Área construida en m²
    /// </summary>
    public decimal ConstructionArea { get; private set; }
    
    public int Bedrooms { get; private set; }
    public int Bathrooms { get; private set; }
    public ListingStatus Status { get; private set; }
    
    public PropertyCategory Category { get; private set; }
    public PropertyType Type { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public Guid ArchitectId { get; private set; }

    // Auditoría
    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public Guid? LastModifiedBy { get; private set; }
    
    /// <summary>
    /// Fecha de la primera publicación. Null si nunca se ha publicado.
    /// </summary>
    public DateTime? FirstPublishedAt { get; private set; }
    
    /// <summary>
    /// Indica si el registro está eliminado lógicamente (Soft Delete).
    /// ABP filtra automáticamente los registros eliminados.
    /// </summary>
    public bool IsDeleted { get; set; }

    // Relaciones
    public ICollection<ListingImage> Images { get; private set; } = new List<ListingImage>();
    public virtual Architect? Architect { get; private set; }
    #endregion

    #region Constructores
    // Constructor para EF Core
    private Listing() 
    {
    }

    /// <summary>
    /// Constructor Factory interno.
    /// Se accede via ListingManager para garantizar orquestación si es necesaria,
    /// o estáticamente via Listing.Create().
    /// </summary>
    internal Listing(
        Guid id,
        string title,
        string description,
        Address? location,
        decimal price,
        decimal landArea,
        decimal constructionArea,
        int bedrooms,
        int bathrooms,
        PropertyCategory category,
        PropertyType type,
        TransactionType transactionType,
        Guid architectId,
        Guid? createdBy)
        : base(id)
    {
        ValidateInvariants(price, landArea, constructionArea);

        Title = title;
        Description = description;
        Location = location;
        Price = price;
        LandArea = landArea;
        ConstructionArea = constructionArea;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        Category = category;
        Type = type;
        TransactionType = transactionType;
        ArchitectId = architectId;
        
        Status = ListingStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }
    #endregion

    #region Metodos de Negocio

    public void UpdateInfo(
        string title,
        string description,
        Address? location,
        decimal price,
        decimal landArea,
        decimal constructionArea,
        int bedrooms,
        int bathrooms,
        PropertyCategory category,
        PropertyType type,
        TransactionType transactionType,
        Guid? modifiedBy)
    {
        // Validar invariantes
        ValidateInvariants(price, landArea, constructionArea);

        Title = title;
        Description = description;
        Location = location;
        Price = price;
        LandArea = landArea;
        ConstructionArea = constructionArea;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        Category = category;
        Type = type;
        TransactionType = transactionType;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Publish(Guid? publishedBy)
    {
        if (Status == ListingStatus.Published) return;

        // Reglas para publicar (Price = -1 significa "precio consultable", es válido)
        if (Price < -1)
        {
            throw new BusinessException("Listing:InvalidPrice").WithData("Price", Price);
        }
        if (Images.Count == 0)
        {
             // Regla de negocio: Debe haber al menos una imagen
             throw new BusinessException("Listing:NoImages");
        }

        Status = ListingStatus.Published;
        if (!FirstPublishedAt.HasValue)
        {
            FirstPublishedAt = DateTime.UtcNow;
        }
        
        UpdateAudit(publishedBy);
        
        // Aquí podríamos disparar un evento de dominio ListingPublishedEvent
    }

    public void Unpublish(Guid? unpublishedBy)
    {
        Status = ListingStatus.Draft;
        UpdateAudit(unpublishedBy);
    }

    public void Archive(Guid? archivedBy)
    {
        Status = ListingStatus.Archived;
        UpdateAudit(archivedBy);
    }

    public void Unarchive(Guid? unarchivedBy)
    {
        // Al desarchivar, regresa a publicado si cumple reglas, o a draft?
        // Asumimos Draft para seguridad, o Published si estaba ok.
        Status = ListingStatus.Draft; 
        UpdateAudit(unarchivedBy);
    }
    
    public void MoveToPortfolio(Guid? movedBy)
    {
        Status = ListingStatus.Portfolio; 
        UpdateAudit(movedBy);
    }

    #region Gestion de Imagenes

    public void AddImage(Guid imageId, string url, string thumbnailUrl, string altText, long fileSize, string contentType)
    {
        if (Images.Count >= MaxImages)
        {
            throw new BusinessException("Listing:MaxImagesExceeded").WithData("Max", MaxImages);
        }

        var nextOrder = Images.Any() ? Images.Max(i => i.SortOrder) + 1 : 0;
        
        var newImage = new ListingImage(
            imageId,
            url,
            nextOrder,
            thumbnailUrl,
            altText,
            fileSize,
            contentType
        );

        Images.Add(newImage);
    }

    public void RemoveImage(Guid imageId)
    {
        var image = Images.FirstOrDefault(x => x.ImageId == imageId);
        if (image == null) return;

        Images.Remove(image);
        ReindexImages();
    }

    public void ReorderImages(List<Guid> orderedIds)
    {
        // Validar que sean los mismos IDs
        if (orderedIds.Count != Images.Count || !Images.All(i => orderedIds.Contains(i.ImageId)))
        {
            // Opcional: Lanzar error o ignorar
            return;
        }

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var img = Images.First(x => x.ImageId == orderedIds[i]);
            img.SortOrder = i;
        }
    }

    private void ReindexImages()
    {
        var sorted = Images.OrderBy(x => x.SortOrder).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SortOrder = i;
        }
    }

    #endregion

    private void ValidateInvariants(decimal price, decimal landArea, decimal constructionArea)
    {
        // -1 significa "precio a consultar", es válido
        if (price < -1)
        {
            throw new BusinessException("Listing:InvalidPrice");
        }
        if (landArea <= 0)
        {
            throw new BusinessException("Listing:LandAreaMustBePositive");
        }
        if (constructionArea < 0)
        {
            // Construccion puede ser 0 (terreno baldío), pero no negativa
            throw new BusinessException("Listing:ConstructionAreaCannotBeNegative");
        }
    }

    private void UpdateAudit(Guid? userId)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;
    }

    #endregion
}