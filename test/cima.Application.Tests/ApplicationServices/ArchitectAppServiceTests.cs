using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared.Dtos;
using cima.Architects;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace cima.ApplicationServices;

/// <summary>
/// Tests básicos de integración para ArchitectAppService
/// </summary>
public sealed class ArchitectAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IArchitectAppService _architectAppService;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IIdentityUserRepository _userRepository;

    public ArchitectAppServiceTests()
    {
        _architectAppService = GetRequiredService<IArchitectAppService>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
        _userRepository = GetRequiredService<IIdentityUserRepository>();
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
        PagedResultDto<ArchitectDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _architectAppService.GetListAsync(input);
        });

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
        ArchitectDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _architectAppService.GetAsync(architect.Id);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(architect.Id);
        result.Bio.ShouldBe(architect.Bio);
    }

    #endregion

    #region Helper Methods

    private async Task<Architect> CreateTestArchitectAsync()
    {
        var architect = new Architect
        {
            UserId = Guid.NewGuid(),
            Name = "Test Architect", // ? Name es required
            Bio = "Experienced architect with over 10 years in residential design"
            // PortfolioUrl eliminado - el portafolio es interno
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _architectRepository.InsertAsync(architect, autoSave: true);
        });

        return architect;
    }

    #endregion
}
