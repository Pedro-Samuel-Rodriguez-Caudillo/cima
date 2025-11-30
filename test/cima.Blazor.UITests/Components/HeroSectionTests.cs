using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Public;
using Microsoft.Extensions.DependencyInjection;

namespace cima.Blazor.UITests.Components;

public class HeroSectionTests : Bunit.TestContext
{
    public HeroSectionTests()
    {
        // Setup any required services
        Services.AddLogging();
    }

    [Fact]
    public void HeroSection_Should_Render()
    {
        // Act
        var cut = RenderComponent<HeroSection>();

        // Assert
        Assert.NotNull(cut);
        Assert.NotEmpty(cut.Markup);
    }

    [Fact]
    public void HeroSection_Should_ContainCallToAction()
    {
        // Act
        var cut = RenderComponent<HeroSection>();

        // Assert - should have some interactive elements
        var buttons = cut.FindAll("button, a[class*='btn']");
        Assert.NotEmpty(buttons);
    }

    [Fact]
    public void HeroSection_Should_DisplayMainHeading()
    {
        // Act
        var cut = RenderComponent<HeroSection>();

        // Assert
        var headings = cut.FindAll("h1, h2");
        Assert.NotEmpty(headings);
    }
}
