using System;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para la entidad ContactRequest
/// </summary>
public sealed class ContactRequestTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_ContactRequest_With_Required_Properties()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest
        {
            Name = "Juan Pérez",
            Email = "juan@example.com",
            Message = "Estoy interesado en esta propiedad"
        };

        // Assert
        contactRequest.ShouldNotBeNull();
        contactRequest.Name.ShouldBe("Juan Pérez");
        contactRequest.Email.ShouldBe("juan@example.com");
        contactRequest.Message.ShouldBe("Estoy interesado en esta propiedad");
    }

    [Fact]
    public void Should_Allow_Null_Phone()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest
        {
            Name = "María González",
            Email = "maria@example.com",
            Message = "Consulta",
            Phone = null
        };

        // Assert
        contactRequest.Phone.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Phone_When_Provided()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Carlos López",
            Email = "carlos@example.com",
            Message = "Pregunta"
        };
        var phone = "+52 33 1234 5678";

        // Act
        contactRequest.Phone = phone;

        // Assert
        contactRequest.Phone.ShouldBe(phone);
    }

    [Fact]
    public void Should_Associate_With_Listing()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Ana Torres",
            Email = "ana@example.com",
            Message = "Interesada"
        };
        var listingId = Guid.NewGuid();

        // Act
        contactRequest.ListingId = listingId;

        // Assert
        contactRequest.ListingId.ShouldBe(listingId);
    }

    [Theory]
    [InlineData(ContactRequestStatus.New)]
    [InlineData(ContactRequestStatus.Replied)]
    [InlineData(ContactRequestStatus.Closed)]
    public void Should_Set_Status(ContactRequestStatus status)
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            Name = "Luis Hernández",
            Email = "luis@example.com",
            Message = "Consulta general"
        };

        // Act
        contactRequest.Status = status;

        // Assert
        contactRequest.Status.ShouldBe(status);
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest
        {
            Name = "Sofia Mendoza",
            Email = "sofia@example.com",
            Message = "Información"
        };

        // Assert
        contactRequest.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }

    [Theory]
    [InlineData("+52 33 1234 5678")]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("33-1234-5678")]
    [InlineData(null)]
    public void Should_Handle_Different_Phone_Formats(string? phone)
    {
        // Arrange & Act
        var contactRequest = new ContactRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Message = "Test message",
            Phone = phone
        };

        // Assert
        contactRequest.Phone.ShouldBe(phone);
    }
}
