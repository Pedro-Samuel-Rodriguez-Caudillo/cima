using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Bunit.TestDoubles;

namespace cima.Blazor.UITests.Components.Common;

public class ErrorDetailTests : TestContext
{
    public ErrorDetailTests()
    {
        // No manual service registration needed for Auth if using AddAuthorization()
    }

    [Fact]
    public void ErrorDetail_Should_ShowFriendlyMessage_ToPublicUser()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("TestUser", AuthorizationState.Unauthorized); // Or just don't set authorized

        var ex = new Exception("Critical Database Failure");

        // Act
        var cut = RenderComponent<ErrorDetail>(parameters => parameters
            .Add(p => p.Exception, ex));

        // Assert
        Assert.Contains("Something went wrong", cut.Markup);
        Assert.DoesNotContain("Critical Database Failure", cut.Markup); 
    }

    [Fact]
    public void ErrorDetail_Should_ShowStackTrace_ToAdminUser()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("AdminUser");
        authContext.SetRoles("admin");

        var ex = new Exception("Critical Database Failure");

        // Act
        var cut = RenderComponent<ErrorDetail>(parameters => parameters
            .Add(p => p.Exception, ex));

        // Assert
        Assert.Contains("Something went wrong", cut.Markup);
        Assert.Contains("Critical Database Failure", cut.Markup); 
        Assert.Contains("details", cut.Markup); 
    }

    [Fact]
    public void ErrorDetail_Should_HaveRecoverAction()
    {
        // Arrange
        var authContext = this.AddTestAuthorization(); // Auth required for rendering AuthorizeView inside
        authContext.SetAuthorized("User"); 

        var ex = new Exception("Test");
        var recovered = false;

        // Act
        var cut = RenderComponent<ErrorDetail>(parameters => parameters
            .Add(p => p.Exception, ex)
            .Add(p => p.OnRecover, () => recovered = true));

        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.True(recovered);
    }
}
