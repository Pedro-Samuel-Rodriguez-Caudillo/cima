using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Public;
using Microsoft.Extensions.DependencyInjection;

namespace cima.Blazor.UITests.Components;

public class PropertySearchFiltersTests : Bunit.TestContext
{
    public PropertySearchFiltersTests()
    {
        Services.AddLogging();
    }

    [Fact]
    public void PropertySearchFilters_Should_Render()
    {
        // Act
        var cut = RenderComponent<PropertySearchFilters>();

        // Assert
        Assert.NotNull(cut);
        Assert.NotEmpty(cut.Markup);
    }

    [Fact]
    public void PropertySearchFilters_Should_ContainFilterInputs()
    {
        // Act
        var cut = RenderComponent<PropertySearchFilters>();

        // Assert
        var inputs = cut.FindAll("input, select");
        Assert.NotEmpty(inputs);
    }

    [Fact]
    public void PropertySearchFilters_Should_HaveSearchButton()
    {
        // Act
        var cut = RenderComponent<PropertySearchFilters>();

        // Assert
        var buttons = cut.FindAll("button");
        Assert.NotEmpty(buttons);
    }
}
