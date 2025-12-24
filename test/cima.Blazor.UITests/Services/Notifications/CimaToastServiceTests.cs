using System.Linq;
using Xunit;
using cima.Blazor.Client.Services.Notifications;
using System.Threading.Tasks;

namespace cima.Blazor.UITests.Services.Notifications;

public class CimaToastServiceTests
{
    [Fact]
    public void ShowToast_AddsMessageToList()
    {
        // Arrange
        var service = new CimaToastService();

        // Act
        service.ShowInfo("Test Message");

        // Assert
        Assert.Single(service.Messages);
        Assert.Equal("Test Message", service.Messages.First().Message);
        Assert.Equal(ToastLevel.Info, service.Messages.First().Level);
    }

    [Fact]
    public void RemoveToast_RemovesMessageFromList()
    {
        // Arrange
        var service = new CimaToastService();
        service.ShowInfo("Test Message");
        var id = service.Messages.First().Id;

        // Act
        service.RemoveToast(id);

        // Assert
        Assert.Empty(service.Messages);
    }

    [Fact]
    public void OnChange_IsInvoked_WhenToastAdded()
    {
        // Arrange
        var service = new CimaToastService();
        var invoked = false;
        service.OnChange += () => invoked = true;

        // Act
        service.ShowSuccess("Success");

        // Assert
        Assert.True(invoked);
    }
}
