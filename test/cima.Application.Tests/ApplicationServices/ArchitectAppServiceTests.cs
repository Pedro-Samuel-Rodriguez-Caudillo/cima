using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;
using cima.Domain.Entities;
using cima.Architects;
using Volo.Abp.Domain.Repositories;

namespace cima.ApplicationServices;

/// <summary>
/// Tests básicos de integração para ArchitectAppService
/// </summary>
public sealed class ArchitectAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IArchitectAppService _architectAppService;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ArchitectAppServiceTests()
    {
        _architectAppService = GetRequiredService<IArchitectAppService>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
    }

    #region GetListAsync Tests

    [Fact]
    public async Task GetListAsync_Should_Return_Paginated_Results()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto
        {
            SkipCount = 0,
            MaxResultCount = 10
        };

        // Act
        var result = await _architectAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_Should_Return_Architect_When_Exists()
    {
        // Arrange
        var architect = await CreateTestArchitectAsync();

        // Act
        var result = await _architectAppService.GetAsync(architect.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(architect.Id);
        result.TotalListingsPublished.ShouldBe(architect.TotalListingsPublished);
        result.IsActive.ShouldBe(architect.IsActive);
    }

    #endregion

    #region Helper Methods

    private async Task<Architect> CreateTestArchitectAsync()
    {
        var architect = new Architect
        {
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _architectRepository.InsertAsync(architect, autoSave: true);
        });

        return architect;
    }

    #endregion
}
