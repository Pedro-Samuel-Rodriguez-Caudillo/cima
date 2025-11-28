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
    public void Should_Create_ContactRequest_With_Parameterless_Constructor()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest();

        // Assert
        contactRequest.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Set_Name()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var name = "María González López";

        // Act
        contactRequest.Name = name;

        // Assert
        contactRequest.Name.ShouldBe(name);
    }

    [Fact]
    public void Should_Set_Email()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var email = "maria.gonzalez@email.com";

        // Act
        contactRequest.Email = email;

        // Assert
        contactRequest.Email.ShouldBe(email);
    }

    [Fact]
    public void Should_Set_Phone()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var phone = "+52 33 1234 5678";

        // Act
        contactRequest.Phone = phone;

        // Assert
        contactRequest.Phone.ShouldBe(phone);
    }

    [Fact]
    public void Should_Set_Message()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var message = "Me interesa conocer más detalles";

        // Act
        contactRequest.Message = message;

        // Assert
        contactRequest.Message.ShouldBe(message);
    }

    [Fact]
    public void Should_Associate_With_Listing()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var listingId = Guid.NewGuid();

        // Act
        contactRequest.ListingId = listingId;

        // Assert
        contactRequest.ListingId.ShouldBe(listingId);
    }

    [Fact]
    public void Should_Associate_With_Architect()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var architectId = Guid.NewGuid();

        // Act
        contactRequest.ArchitectId = architectId;

        // Assert
        contactRequest.ArchitectId.ShouldBe(architectId);
    }

    [Theory]
    [InlineData(ContactRequestStatus.New)]
    [InlineData(ContactRequestStatus.Replied)]
    [InlineData(ContactRequestStatus.Closed)]
    public void Should_Set_Status(ContactRequestStatus status)
    {
        // Arrange
        var contactRequest = new ContactRequest();

        // Act
        contactRequest.Status = status;

        // Assert
        contactRequest.Status.ShouldBe(status);
    }

    [Fact]
    public void Should_Track_CreatedAt()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var createdAt = DateTime.UtcNow;

        // Act
        contactRequest.CreatedAt = createdAt;

        // Assert
        contactRequest.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Should_Track_RepliedAt()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var repliedAt = DateTime.UtcNow;

        // Act
        contactRequest.RepliedAt = repliedAt;

        // Assert
        contactRequest.RepliedAt.ShouldBe(repliedAt);
    }

    [Fact]
    public void Should_Set_ReplyNotes()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var replyNotes = "Contactado por email el 15/01/2024";

        // Act
        contactRequest.ReplyNotes = replyNotes;

        // Assert
        contactRequest.ReplyNotes.ShouldBe(replyNotes);
    }

    [Fact]
    public void Should_Allow_Null_RepliedAt()
    {
        // Arrange
        var contactRequest = new ContactRequest();

        // Act
        contactRequest.RepliedAt = null;

        // Assert
        contactRequest.RepliedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_All_Properties_Together()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var listingId = Guid.NewGuid();
        var architectId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        contactRequest.ListingId = listingId;
        contactRequest.ArchitectId = architectId;
        contactRequest.Name = "Juan Pérez";
        contactRequest.Email = "juan@email.com";
        contactRequest.Phone = "+52 33 1111 2222";
        contactRequest.Message = "Consulta sobre la propiedad";
        contactRequest.Status = ContactRequestStatus.New;
        contactRequest.CreatedAt = createdAt;

        // Assert
        contactRequest.ListingId.ShouldBe(listingId);
        contactRequest.ArchitectId.ShouldBe(architectId);
        contactRequest.Name.ShouldBe("Juan Pérez");
        contactRequest.Email.ShouldBe("juan@email.com");
        contactRequest.Phone.ShouldBe("+52 33 1111 2222");
        contactRequest.Message.ShouldBe("Consulta sobre la propiedad");
        contactRequest.Status.ShouldBe(ContactRequestStatus.New);
        contactRequest.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Should_Store_Long_Message()
    {
        // Arrange
        var contactRequest = new ContactRequest();
        var longMessage = "Hola, me interesa mucho esta propiedad. " +
                         "Me gustaría saber si es posible agendar una visita para este fin de semana. " +
                         "También quisiera información sobre el proceso de compra y financiamiento.";

        // Act
        contactRequest.Message = longMessage;

        // Assert
        contactRequest.Message.ShouldBe(longMessage);
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var contactRequest = new ContactRequest();

        // Assert
        contactRequest.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }

    [Theory]
    [InlineData("+52 33 1234 5678")]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("33-1234-5678")]
    public void Should_Handle_Different_Phone_Formats(string phone)
    {
        // Arrange
        var contactRequest = new ContactRequest();

        // Act
        contactRequest.Phone = phone;

        // Assert
        contactRequest.Phone.ShouldBe(phone);
    }

    [Theory]
    [InlineData("user@gmail.com")]
    [InlineData("contact@company.mx")]
    [InlineData("info@real-estate.com.mx")]
    public void Should_Handle_Different_Email_Formats(string email)
    {
        // Arrange
        var contactRequest = new ContactRequest();

        // Act
        contactRequest.Email = email;

        // Assert
        contactRequest.Email.ShouldBe(email);
    }
}
