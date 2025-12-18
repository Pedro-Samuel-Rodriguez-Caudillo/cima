using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Categories;

public interface ICategoryAppService : IApplicationService
{
    // Categories
    Task<List<PropertyCategoryDto>> GetCategoriesAsync(bool includeInactive = false);
    Task<PropertyCategoryDto> GetCategoryAsync(Guid id);
    Task<PropertyCategoryDto> CreateCategoryAsync(CreateUpdateCategoryDto input);
    Task<PropertyCategoryDto> UpdateCategoryAsync(Guid id, CreateUpdateCategoryDto input);
    Task DeleteCategoryAsync(Guid id);
    
    // Types
    Task<List<PropertyTypeDto>> GetTypesAsync(Guid? categoryId = null, bool includeInactive = false);
    Task<PropertyTypeDto> GetTypeAsync(Guid id);
    Task<PropertyTypeDto> CreateTypeAsync(CreateUpdateTypeDto input);
    Task<PropertyTypeDto> UpdateTypeAsync(Guid id, CreateUpdateTypeDto input);
    Task DeleteTypeAsync(Guid id);
}
