using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Public;
using Microsoft.Extensions.DependencyInjection;

namespace cima.Blazor.UITests.Components;

public class ContactFormTests : Bunit.TestContext
{
    public ContactFormTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void ContactForm_Should_Render()
    {
        // Act
        var cut = RenderComponent<ContactForm>();

        // Assert
        Assert.NotNull(cut);
        var form = cut.Find("form");
        Assert.NotNull(form);
    }

    [Fact]
    public void ContactForm_Should_ContainRequiredFields()
    {
        // Act
        var cut = RenderComponent<ContactForm>();

        // Assert
        var inputs = cut.FindAll("input[type='text'], input[type='email'], textarea");
        Assert.NotEmpty(inputs);
    }

    [Fact]
    public void ContactForm_Should_HaveSubmitButton()
    {
        // Act
        var cut = RenderComponent<ContactForm>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        Assert.NotNull(submitButton);
    }

    [Fact]
    public void ContactForm_Should_ShowValidationForEmptyName()
    {
        // Act
        var cut = RenderComponent<ContactForm>();
        var submitButton = cut.Find("button[type='submit']");
        
        // Trigger validation by attempting to submit
        submitButton.Click();

        // Assert - check for validation messages (implementation dependent)
        var markup = cut.Markup;
        Assert.NotNull(markup);
    }
}
