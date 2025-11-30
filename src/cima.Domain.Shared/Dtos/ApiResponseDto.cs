using System;
using System.Collections.Generic;

namespace cima.Domain.Shared.Dtos
{
    public class ApiResponseDto<T>
    {
        public required T Data { get; set; }  // ? required
        public bool Success { get; set; }
        public required string Message { get; set; }  // ? required
    }

    public class PagedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int SkipCount { get; set; }
        public int MaxResultCount { get; set; }

        public int TotalPages => MaxResultCount > 0 ? (TotalCount + MaxResultCount - 1) / MaxResultCount : 0;
        public int CurrentPage => SkipCount / MaxResultCount + 1;
    }

    public class CreateResultDto<T>
    {
        public required T Data { get; set; }  // ? required
        public required string Id { get; set; }  // ? required
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateResultDto<T>
    {
        public required T Data { get; set; }  // ? required
        public DateTime UpdatedAt { get; set; }
    }

    public class DeleteResultDto
    {
        public Guid Id { get; set; }
        public DateTime DeletedAt { get; set; }
        public bool Success { get; set; }
    }
}
