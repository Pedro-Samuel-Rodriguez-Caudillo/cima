using System;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para la entidad Architect
/// Ahora con nueva estructura: sin Name/Bio, con estadísticas y metadata
/// </summary>
public sealed class ArchitectTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_Architect_With_Required_Properties()
    {
        // Arrange & Act
        var userId = Guid.NewGuid();
        var architect = new Architect 
        { 
            UserId = userId,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        architect.ShouldNotBeNull();
        architect.UserId.ShouldBe(userId);
        architect.TotalListingsPublished.ShouldBe(0);
        architect.ActiveListings.ShouldBe(0);
        architect.IsActive.ShouldBeTrue();
        architect.Listings.ShouldNotBeNull();
        architect.Listings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Set_UserId()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var architect = new Architect 
        { 
            UserId = userId1,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        architect.UserId = userId2;

        // Assert
        architect.UserId.ShouldBe(userId2);
    }

    [Fact]
    public void Should_Track_TotalListingsPublished()
    {
        // Arrange
        var architect = new Architect 
        { 
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        architect.TotalListingsPublished = 5;

        // Assert
        architect.TotalListingsPublished.ShouldBe(5);
    }

    [Fact]
    public void Should_Track_ActiveListings()
    {
        // Arrange
        var architect = new Architect 
        { 
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 10,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        architect.ActiveListings = 3;

        // Assert
        architect.ActiveListings.ShouldBe(3);
    }

    [Fact]
    public void Should_Set_RegistrationDate()
    {
        // Arrange
        var registrationDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var architect = new Architect
        {
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = registrationDate,
            IsActive = true
        };

        // Assert
        architect.RegistrationDate.ShouldBe(registrationDate);
    }

    [Fact]
    public void Should_Set_IsActive_Status()
    {
        // Arrange
        var architect = new Architect 
        { 
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        architect.IsActive = false;

        // Assert
        architect.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var architect = new Architect 
        { 
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        architect.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }

    [Fact]
    public void Should_Initialize_Empty_Listings_Collection()
    {
        // Arrange & Act
        var architect = new Architect 
        { 
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        architect.Listings.ShouldNotBeNull();
        architect.Listings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Update_Statistics()
    {
        // Arrange
        var architect = new Architect
        {
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 5,
            ActiveListings = 3,
            RegistrationDate = DateTime.UtcNow.AddMonths(-6),
            IsActive = true
        };

        // Act - Simular publicación de nuevas propiedades
        architect.TotalListingsPublished += 2;
        architect.ActiveListings += 2;

        // Assert
        architect.TotalListingsPublished.ShouldBe(7);
        architect.ActiveListings.ShouldBe(5);
    }

    [Fact]
    public void Should_Deactivate_Architect()
    {
        // Arrange
        var architect = new Architect
        {
            UserId = Guid.NewGuid(),
            TotalListingsPublished = 10,
            ActiveListings = 3,
            RegistrationDate = DateTime.UtcNow.AddYears(-1),
            IsActive = true
        };

        // Act
        architect.IsActive = false;

        // Assert
        architect.IsActive.ShouldBeFalse();
        architect.TotalListingsPublished.ShouldBe(10); // Stats no se pierden al desactivar
    }
}
