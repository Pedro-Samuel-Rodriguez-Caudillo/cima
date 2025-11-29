using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Public;
using System.Collections.Generic;

namespace cima.Blazor.UITests.Components;

public class ImageGalleryTests : Bunit.TestContext
{
    [Fact]
    public void ImageGallery_Should_RenderWithImages()
    {
        // Arrange
        var images = new List<string>
        {
            "/images/test1.jpg",
            "/images/test2.jpg",
            "/images/test3.jpg"
        };

        // Act
        var cut = RenderComponent<ImageGallery>(parameters => parameters
            .Add(p => p.Images, images));

        // Assert
        var imgElements = cut.FindAll("img");
        Assert.True(imgElements.Count >= 3);
    }

    [Fact]
    public void ImageGallery_Should_DisplayImagesInOrder()
    {
        // Arrange
        var images = new List<string>
        {
            "/images/first.jpg",
            "/images/second.jpg"
        };

        // Act
        var cut = RenderComponent<ImageGallery>(parameters => parameters
            .Add(p => p.Images, images));

        // Assert
        var imgElements = cut.FindAll("img");
        Assert.True(imgElements.Count > 0);
        Assert.Contains("/images/first.jpg", imgElements[0].GetAttribute("src") ?? "");
    }

    [Fact]
    public void ImageGallery_Should_HandleEmptyImageList()
    {
        // Arrange
        var images = new List<string>();

        // Act
        var cut = RenderComponent<ImageGallery>(parameters => parameters
            .Add(p => p.Images, images));

        // Assert - component adds placeholder if empty
        var imgElements = cut.FindAll("img");
        Assert.NotEmpty(imgElements);
    }

    [Fact]
    public void ImageGallery_Should_ShowNavigationButtons_WithMultipleImages()
    {
        // Arrange
        var images = new List<string>
        {
            "/images/test1.jpg",
            "/images/test2.jpg"
        };

        // Act
        var cut = RenderComponent<ImageGallery>(parameters => parameters
            .Add(p => p.Images, images));

        // Assert - should have navigation buttons
        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 2);
    }
}
