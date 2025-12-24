using System.Collections.Generic;
using Volo.Abp.Validation;
using Xunit;
using cima.Blazor.Client.Services;
using Moq;
using Microsoft.Extensions.Localization;
using cima.Localization;
using System.ComponentModel.DataAnnotations;

namespace cima.Blazor.UITests.Services.ExceptionParsing;

public class ExceptionMessageParserTests
{
    private readonly Mock<IStringLocalizer<cimaResource>> _localizerMock;
    private readonly ExceptionMessageParser _parser;

    public ExceptionMessageParserTests()
    {
        _localizerMock = new Mock<IStringLocalizer<cimaResource>>();
        _localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        _parser = new ExceptionMessageParser(_localizerMock.Object);
    }

    [Fact]
    public void Parse_Should_Return_Friendly_Message_For_ValidationException()
    {
        // Arrange
        var validationErrors = new List<ValidationResult>
        {
            new ValidationResult("Field is required", new[] { "Field" })
        };
        var ex = new AbpValidationException("Validation failed", validationErrors);

        // Act
        var message = _parser.Parse(ex);

        // Assert
        Assert.Contains("Field is required", message);
    }

    [Fact]
    public void Parse_Should_Return_Default_Message_For_GenericException()
    {
        // Arrange
        var ex = new System.Exception("System Error");

        // Act
        var message = _parser.Parse(ex);

        // Assert
        Assert.Equal("Common:Error", message);
    }
}
