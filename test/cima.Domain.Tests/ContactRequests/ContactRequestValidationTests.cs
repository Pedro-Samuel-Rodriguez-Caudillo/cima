using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Shouldly;
using Xunit;

namespace cima.ContactRequests;

public class ContactRequestValidationTests
{
    [Fact]
    public void Should_Pass_Validation_With_Correct_Data()
    {
        // Arrange
        var dto = new CreateContactRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            Message = "I am interested in this property.",
            ListingId = System.Guid.NewGuid()
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Fail_Validation_When_Name_Is_Missing()
    {
        // Arrange
        var dto = new CreateContactRequestDto
        {
            Name = null!,
            Email = "john@example.com",
            Message = "Message",
            ListingId = System.Guid.NewGuid()
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.ShouldContain(v => v.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Should_Fail_Validation_When_Email_Is_Invalid()
    {
        // Arrange
        var dto = new CreateContactRequestDto
        {
            Name = "John Doe",
            Email = "invalid-email",
            Message = "Message",
            ListingId = System.Guid.NewGuid()
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.ShouldContain(v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Should_Fail_Validation_When_Message_Is_Too_Long()
    {
        // Arrange
        var dto = new CreateContactRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            Message = new string('a', 2001),
            ListingId = System.Guid.NewGuid()
        };

        // Act
        var results = ValidateModel(dto);

        // Assert
        results.ShouldContain(v => v.MemberNames.Contains("Message"));
    }

    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}
