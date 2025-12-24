using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace cima.Blazor.UITests.Components.Forms;

public class LoadingButtonTests : TestContext
{
    public LoadingButtonTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void LoadingButton_Should_RenderChildContent()
    {
        // Act
        var cut = RenderComponent<LoadingButton>(parameters => parameters
            .Add(p => p.ChildContent, "<span>Click Me</span>"));

        // Assert
        cut.MarkupMatches("<button class=\"cima-btn cima-btn-primary disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2\"  ><span>Click Me</span></button>");
    }

    [Fact]
    public void LoadingButton_Should_ShowSpinner_When_IsLoading()
    {
        // Act
        var cut = RenderComponent<LoadingButton>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.ChildContent, "Save"));

        // Assert
        Assert.Contains("animate-spin", cut.Markup); // Check for spinner class
        Assert.Contains("disabled", cut.Markup); // Button should be disabled
    }

    [Fact]
    public void LoadingButton_Should_ExecuteOnClick_When_NotLoading()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<LoadingButton>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true)));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void LoadingButton_Should_NotExecuteOnClick_When_IsLoading()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<LoadingButton>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true)));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.False(clicked);
    }
}
