using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace cima.Categories;

[Authorize(Roles = "admin")]
public class CategoryAppService : ApplicationService, ICategoryAppService
{
    private readonly IRepository<PropertyCategoryEntity, Guid> _categoryRepository;
    private readonly IRepository<PropertyTypeEntity, Guid> _typeRepository;

    public CategoryAppService(
        IRepository<PropertyCategoryEntity, Guid> categoryRepository,
        IRepository<PropertyTypeEntity, Guid> typeRepository)
    {
        _categoryRepository = categoryRepository;
        _typeRepository = typeRepository;
    }

    #region Categories

    public async Task<List<PropertyCategoryDto>> GetCategoriesAsync(bool includeInactive = false)
    {
        var categories = await _categoryRepository.GetListAsync();
        
        var result = categories
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PropertyCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                Icon = x.Icon
            })
            .ToList();

        return result;
    }

    public async Task<PropertyCategoryDto> GetCategoryAsync(Guid id)
    {
        var entity = await _categoryRepository.GetAsync(id);
        return new PropertyCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            Icon = entity.Icon
        };
    }

    public async Task<PropertyCategoryDto> CreateCategoryAsync(CreateUpdateCategoryDto input)
    {
        var entity = new PropertyCategoryEntity(
            GuidGenerator.Create(),
            input.Name,
            input.Description,
            input.SortOrder,
            input.IsActive,
            input.Icon
        );

        await _categoryRepository.InsertAsync(entity);
        
        return new PropertyCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            Icon = entity.Icon
        };
    }

    public async Task<PropertyCategoryDto> UpdateCategoryAsync(Guid id, CreateUpdateCategoryDto input)
    {
        var entity = await _categoryRepository.GetAsync(id);
        entity.Update(input.Name, input.Description, input.SortOrder, input.IsActive, input.Icon);
        await _categoryRepository.UpdateAsync(entity);
        
        return new PropertyCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive,
            Icon = entity.Icon
        };
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        await _categoryRepository.DeleteAsync(id);
    }

    #endregion

    #region Types

    public async Task<List<PropertyTypeDto>> GetTypesAsync(Guid? categoryId = null, bool includeInactive = false)
    {
        var types = await _typeRepository.GetListAsync();
        var categories = await _categoryRepository.GetListAsync();
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);
        
        var result = types
            .Where(x => (!categoryId.HasValue || x.CategoryId == categoryId.Value))
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new PropertyTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                CategoryName = categoryDict.GetValueOrDefault(x.CategoryId),
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToList();

        return result;
    }

    public async Task<PropertyTypeDto> GetTypeAsync(Guid id)
    {
        var entity = await _typeRepository.GetAsync(id);
        var category = await _categoryRepository.FindAsync(entity.CategoryId);

        return new PropertyTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            CategoryName = category?.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        };
    }

    public async Task<PropertyTypeDto> CreateTypeAsync(CreateUpdateTypeDto input)
    {
        // Validate category exists
        var category = await _categoryRepository.GetAsync(input.CategoryId);
        
        var entity = new PropertyTypeEntity(
            GuidGenerator.Create(),
            input.Name,
            input.CategoryId,
            input.Description,
            input.SortOrder,
            input.IsActive
        );

        await _typeRepository.InsertAsync(entity);
        
        return new PropertyTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        };
    }

    public async Task<PropertyTypeDto> UpdateTypeAsync(Guid id, CreateUpdateTypeDto input)
    {
        var entity = await _typeRepository.GetAsync(id);
        var category = await _categoryRepository.GetAsync(input.CategoryId);
        
        entity.Update(input.Name, input.CategoryId, input.Description, input.SortOrder, input.IsActive);
        await _typeRepository.UpdateAsync(entity);
        
        return new PropertyTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        };
    }

    public async Task DeleteTypeAsync(Guid id)
    {
        await _typeRepository.DeleteAsync(id);
    }

    #endregion
}
