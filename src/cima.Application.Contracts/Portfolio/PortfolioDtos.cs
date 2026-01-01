using System;
using System.Collections.Generic;
using cima.Domain.Shared;
using Volo.Abp.Application.Dtos;

namespace cima.Portfolio;

public class PortfolioProjectDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime? CompletionDate { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? ListingId { get; set; }
    public string Testimonial { get; set; } = string.Empty;
    public string TestimonialAuthor { get; set; } = string.Empty;
    public string CoverImage { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public ICollection<PortfolioImageDto> Gallery { get; set; } = new List<PortfolioImageDto>();
}

public class PortfolioImageDto : EntityDto<Guid>
{
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string AltText { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
}

public class CreateUpdatePortfolioProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime? CompletionDate { get; set; }
    public Guid CategoryId { get; set; }
    public string Testimonial { get; set; } = string.Empty;
    public string TestimonialAuthor { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
}

public class CreatePortfolioImageDto
{
    public string Url { get; set; }
    public string AltText { get; set; }
    public string Caption { get; set; }
    public string Tags { get; set; }
}

public class GetPortfolioListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CategoryId { get; set; }
}
