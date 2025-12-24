using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Common;
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Bunit.TestDoubles;

namespace cima.Blazor.UITests.Components.Common;

public class RetryBoundaryTests : TestContext
{
    public RetryBoundaryTests()
    {
        this.AddTestAuthorization();
    }

    [Fact]
    public void RetryBoundary_Should_RenderChildContent_When_NoException()
    {
        // Act
        var cut = RenderComponent<RetryBoundary>(parameters => parameters
            .AddChildContent("<div>Safe Content</div>"));

        // Assert
        cut.MarkupMatches("<div>Safe Content</div>");
    }

    [Fact]
    public void RetryBoundary_Should_ShowErrorDetail_When_ExceptionOccurs()
    {
        // Act
        var cut = RenderComponent<RetryBoundary>(parameters => parameters
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<ThrowingComponent>(0);
                builder.CloseComponent();
            })));

        // Assert
        Assert.Contains("Something went wrong", cut.Markup);
    }

    [Fact]
    public void RetryBoundary_Should_Recover_When_RetryClicked()
    {
        // Arrange
        var throwError = true;
        
        var cut = RenderComponent<RetryBoundary>(parameters => parameters
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                if (throwError)
                {
                    builder.OpenComponent<ThrowingComponent>(0);
                    builder.CloseComponent();
                }
                else
                {
                    builder.AddMarkupContent(0, "<div>Recovered Content</div>");
                }
            })));

        // Verify error state
        Assert.Contains("Something went wrong", cut.Markup);

        // Act
        throwError = false; // Stop throwing
        var button = cut.Find("button"); // Find the reload button in ErrorDetail
        button.Click();

        // Assert
        Assert.Contains("Recovered Content", cut.Markup);
    }

    class ThrowingComponent : ComponentBase
    {
        protected override void OnInitialized()
        {
            throw new Exception("Test Exception");
        }
    }
}
