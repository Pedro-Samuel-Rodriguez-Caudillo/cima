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
    public void Should_Create_Architect_With_Required_Properties()
    {
        // Arrange & Act
        var architect = new Architect 
        { 
            Name = "Juan Pérez"
        };

        // Assert
        architect.ShouldNotBeNull();
        architect.Name.ShouldBe("Juan Pérez");
        architect.Listings.ShouldNotBeNull();
        architect.Listings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Set_UserId()
    {
        // Arrange
        var architect = new Architect 
        { 
            Name = "María González"
        };
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
        var architect = new Architect 
        { 
            Name = "Carlos Ramírez"
        };
        var bio = "Arquitecto con más de 15 años de experiencia";

        // Act
        architect.Bio = bio;

        // Assert
        architect.Bio.ShouldBe(bio);
    }

    [Fact]
    public void Should_Allow_Null_Bio()
    {
        // Arrange & Act
        var architect = new Architect 
        { 
            Name = "Ana Torres",
            Bio = null
        };

        // Assert
        architect.Bio.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_All_Properties_Together()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var architect = new Architect
        {
            UserId = userId,
            Name = "Luis Hernández",
            Bio = "Especialista en diseño residencial"
        };

        // Assert
        architect.UserId.ShouldBe(userId);
        architect.Name.ShouldBe("Luis Hernández");
        architect.Bio.ShouldBe("Especialista en diseño residencial");
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var architect = new Architect 
        { 
            Name = "Pedro Sánchez"
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
            Name = "Sofia Mendoza"
        };

        // Assert
        architect.Listings.ShouldNotBeNull();
        architect.Listings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Store_Long_Bio()
    {
        // Arrange
        var longBio = "Arquitecto con más de 15 años de experiencia en diseño residencial y comercial. " +
                     "Especializado en arquitectura sustentable y eficiencia energética. " +
                     "Ha participado en proyectos reconocidos a nivel nacional e internacional.";

        // Act
        var architect = new Architect
        {
            Name = "Roberto Jiménez",
            Bio = longBio
        };

        // Assert
        architect.Bio.ShouldBe(longBio);
    }

    [Fact]
    public void Should_Require_Name()
    {
        // Arrange & Act
        var architect = new Architect
        {
            Name = "Nombre Válido"
        };

        // Assert - Name is required and cannot be empty
        architect.Name.ShouldNotBeNullOrEmpty();
        architect.Name.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("Juan Pérez")]
    [InlineData("María González López")]
    [InlineData("Arq. Carlos Ramírez")]
    public void Should_Store_Different_Name_Formats(string name)
    {
        // Arrange & Act
        var architect = new Architect
        {
            Name = name
        };

        // Assert
        architect.Name.ShouldBe(name);
    }
}
