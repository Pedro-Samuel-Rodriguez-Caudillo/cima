using System;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para la entidad Architect
/// </summary>
public sealed class ArchitectTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_Architect_With_Parameterless_Constructor()
    {
        // Arrange & Act
        var architect = new Architect();

        // Assert
        architect.ShouldNotBeNull();
        architect.Listings.ShouldNotBeNull();
        architect.Listings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Set_UserId()
    {
        // Arrange
        var architect = new Architect();
        var userId = Guid.NewGuid();

        // Act
        architect.UserId = userId;

        // Assert
        architect.UserId.ShouldBe(userId);
    }

    [Fact]
    public void Should_Set_Bio()
    {
        // Arrange
        var architect = new Architect();
        var bio = "Arquitecto con más de 15 años de experiencia";

        // Act
        architect.Bio = bio;

        // Assert
        architect.Bio.ShouldBe(bio);
    }

    [Fact]
    public void Should_Set_PortfolioUrl()
    {
        // Arrange
        var architect = new Architect();
        var portfolioUrl = "https://portfolio.example.com";

        // Act
        architect.PortfolioUrl = portfolioUrl;

        // Assert
        architect.PortfolioUrl.ShouldBe(portfolioUrl);
    }

    [Fact]
    public void Should_Set_All_Properties_Together()
    {
        // Arrange
        var architect = new Architect();
        var userId = Guid.NewGuid();

        // Act
        architect.UserId = userId;
        architect.Bio = "Especialista en diseño residencial";
        architect.PortfolioUrl = "https://myportfolio.com";

        // Assert
        architect.UserId.ShouldBe(userId);
        architect.Bio.ShouldBe("Especialista en diseño residencial");
        architect.PortfolioUrl.ShouldBe("https://myportfolio.com");
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var architect = new Architect();

        // Assert
        architect.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }

    [Fact]
    public void Should_Initialize_Empty_Listings_Collection()
    {
        // Arrange & Act
        var architect = new Architect();

        // Assert
        architect.Listings.ShouldNotBeNull();
        architect.Listings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Store_Long_Bio()
    {
        // Arrange
        var architect = new Architect();
        var longBio = "Arquitecto con más de 15 años de experiencia en diseño residencial y comercial. " +
                     "Especializado en arquitectura sustentable y eficiencia energética. " +
                     "Ha participado en proyectos reconocidos a nivel nacional e internacional.";

        // Act
        architect.Bio = longBio;

        // Assert
        architect.Bio.ShouldBe(longBio);
    }

    [Theory]
    [InlineData("https://www.architect-portfolio.com")]
    [InlineData("https://arquitectura.mx/juan-perez")]
    [InlineData("www.myarchitecture.com")]
    public void Should_Store_Portfolio_Url(string url)
    {
        // Arrange
        var architect = new Architect();

        // Act
        architect.PortfolioUrl = url;

        // Assert
        architect.PortfolioUrl.ShouldBe(url);
    }
}
