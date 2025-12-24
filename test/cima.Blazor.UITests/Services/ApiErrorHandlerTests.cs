using System;
using System.Threading.Tasks;
using Xunit;
using cima.Blazor.Client.Services;
using MudBlazor;
using Moq;
using Microsoft.Extensions.Localization;
using cima.Localization;

namespace cima.Blazor.UITests.Services;

public class ApiErrorHandlerTests
{
    private readonly Mock<ISnackbar> _snackbarMock;
    private readonly Mock<IStringLocalizer<cimaResource>> _localizerMock;
    private readonly ApiErrorHandler _handler;

    public ApiErrorHandlerTests()
    {
        _snackbarMock = new Mock<ISnackbar>();
        _localizerMock = new Mock<IStringLocalizer<cimaResource>>();
        
        // Setup default localizer behavior
        _localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _handler = new ApiErrorHandler(_snackbarMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_ExecuteAction_Successfully()
    {
        // Arrange
        var executed = false;
        Task Action() { executed = true; return Task.CompletedTask; }

        // Act
        var result = await _handler.HandleAsync(Action);

        // Assert
        Assert.True(result);
        Assert.True(executed);
        _snackbarMock.Verify(s => s.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_HandleException_AndReturnFalse()
    {
        // Arrange
        Task Action() => throw new Exception("API Error");

        // Act
        var result = await _handler.HandleAsync(Action);

        // Assert
        Assert.False(result);
        _snackbarMock.Verify(s => s.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
    }
}
