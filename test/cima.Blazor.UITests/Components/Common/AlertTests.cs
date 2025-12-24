using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Common;
using cima.Blazor.Client.Services.Notifications;
using Microsoft.AspNetCore.Components;

namespace cima.Blazor.UITests.Components.Common;

public class AlertTests : TestContext
{
    [Fact]
    public void Alert_Should_RenderTitleAndContent()
    {
        // Act
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(p => p.Title, "Warning Title")
            .Add(p => p.ChildContent, "This is a warning message."));

        // Assert
        Assert.Contains("Warning Title", cut.Markup);
        Assert.Contains("This is a warning message.", cut.Markup);
    }

    [Fact]
    public void Alert_Should_ApplyCorrectStyles_ForErrorLevel()
    {
        // Act
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(p => p.Level, ToastLevel.Error)
            .Add(p => p.Title, "Error"));

        // Assert
        Assert.Contains("bg-red-50", cut.Markup);
        Assert.Contains("text-red-800", cut.Markup);
    }

    [Fact]
    public void Alert_Should_RenderCloseButton_When_OnCloseProvided()
    {
        // Arrange
        var closed = false;

        // Act
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(p => p.Title, "Alert")
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        
        button.Click();
        Assert.True(closed);
    }

    [Fact]
    public void Alert_Should_NotRenderCloseButton_When_OnCloseMissing()
    {
        // Act
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(p => p.Title, "Alert"));

        // Assert
        var buttons = cut.FindAll("button");
        Assert.Empty(buttons);
    }
}
