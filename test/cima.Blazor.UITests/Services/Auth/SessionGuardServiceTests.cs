using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using cima.Blazor.Client.Services.Auth;
using cima.Blazor.Client.Components.Common;
using Microsoft.Extensions.Localization;
using cima.Localization;
using System.Security.Claims;

namespace cima.Blazor.UITests.Services.Auth;

public class SessionGuardServiceTests
{
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<NavigationManager> _navigationManagerMock;
    private readonly Mock<AuthenticationStateProvider> _authProviderMock;
    private readonly Mock<IStringLocalizer<cimaResource>> _localizerMock;
    private readonly SessionGuardService _service;

    public SessionGuardServiceTests()
    {
        _dialogServiceMock = new Mock<IDialogService>();
        _navigationManagerMock = new Mock<NavigationManager>();
        _authProviderMock = new Mock<AuthenticationStateProvider>();
        _localizerMock = new Mock<IStringLocalizer<cimaResource>>();

        _localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

        _service = new SessionGuardService(
            _dialogServiceMock.Object,
            _navigationManagerMock.Object,
            _authProviderMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task ValidateSessionAsync_ReturnsTrue_WhenAuthenticated()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "User") }, "Test"));
        _authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(user));

        // Act
        var result = await _service.ValidateSessionAsync();

        // Assert
        Assert.True(result);
        _dialogServiceMock.Verify(d => d.ShowAsync<SessionExpiredDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()), Times.Never);
    }

    [Fact]
    public async Task ValidateSessionAsync_ReturnsFalse_AndShowsDialog_WhenNotAuthenticated()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated
        _authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(user));

        var dialogReferenceMock = new Mock<IDialogReference>();
        dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(DialogResult.Ok(true));

        _dialogServiceMock.Setup(d => d.ShowAsync<SessionExpiredDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(dialogReferenceMock.Object);

        // Act
        var result = await _service.ValidateSessionAsync();

        // Assert
        Assert.False(result);
        _dialogServiceMock.Verify(d => d.ShowAsync<SessionExpiredDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()), Times.Once);
        // Note: Navigation verification is tricky with Mock<NavigationManager> due to extension methods, skipping direct navigation verify for this unit test scope.
    }
}
