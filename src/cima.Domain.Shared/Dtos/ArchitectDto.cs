using System;
using System.Collections.Generic;

namespace cima.Domain.Shared.Dtos
{
    public class ArchitectDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }
        public string UserName { get; set; }
    }

    public class CreateUpdateArchitectDto
    {
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }
    }

    public class ArchitectDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }
        public string UserName { get; set; }
        public List<PropertyListDto> Properties { get; set; } = new();
    }

    public class ArchitectListDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Bio { get; set; }
        public string PortfolioUrl { get; set; }
        public int PropertiesCount { get; set; }
    }
}
