using System;
using Shouldly;
using Xunit;
using cima.Domain.Entities;

namespace cima.Entities;

/// <summary>
/// Tests para el Value Object ListingImage
/// </summary>
public sealed class ListingImageTests
{
    [Fact]
    public void Should_Create_ListingImage_With_Correct_Properties()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var url = "https://example.com/image.jpg";
        var thumbnailUrl = "https://example.com/thumb.jpg";
        var sortOrder = 1;
        var altText = "Test image";
        var fileSize = 1024000L;
        var contentType = "image/jpeg";

        // Act
        var image = new ListingImage(
            imageId: imageId,
            url: url,
            sortOrder: sortOrder,
            thumbnailUrl: thumbnailUrl,
            altText: altText,
            fileSize: fileSize,
            contentType: contentType
        );

        // Assert
        image.ImageId.ShouldBe(imageId);
        image.Url.ShouldBe(url);
        image.ThumbnailUrl.ShouldBe(thumbnailUrl);
        image.SortOrder.ShouldBe(sortOrder);
        image.AltText.ShouldBe(altText);
        image.FileSize.ShouldBe(fileSize);
        image.ContentType.ShouldBe(contentType);
    }

    [Fact]
    public void Should_Default_ThumbnailUrl_To_Url_When_Null_Or_Empty()
    {
        // Arrange
        var url = "https://example.com/image.jpg";

        // Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: url,
            sortOrder: 0,
            thumbnailUrl: null!,
            altText: "Test",
            fileSize: 1000,
            contentType: "image/jpeg"
        );

        // Assert
        image.ThumbnailUrl.ShouldBe(url);
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_Url_Is_Null()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => new ListingImage(
            imageId: Guid.NewGuid(),
            url: null!,
            sortOrder: 0
        ));
    }

    [Fact]
    public void Should_Have_Same_Values_When_Created_Identically()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var url = "https://example.com/image.jpg";
        
        var img1 = new ListingImage(imageId, url, 1, "thumb", "alt", 100, "image/jpeg");
        var img2 = new ListingImage(imageId, url, 1, "thumb", "alt", 100, "image/jpeg");

        // Assert
        img1.ImageId.ShouldBe(img2.ImageId);
        img1.Url.ShouldBe(img2.Url);
        img1.ThumbnailUrl.ShouldBe(img2.ThumbnailUrl);
        img1.SortOrder.ShouldBe(img2.SortOrder);
        img1.AltText.ShouldBe(img2.AltText);
        img1.FileSize.ShouldBe(img2.FileSize);
        img1.ContentType.ShouldBe(img2.ContentType);
    }

    [Fact]
    public void Should_Not_Have_Same_SortOrder()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var url = "https://example.com/image.jpg";
        
        var img1 = new ListingImage(imageId, url, 1, "thumb", "alt", 100, "image/jpeg");
        var img2 = new ListingImage(imageId, url, 2, "thumb", "alt", 100, "image/jpeg");

        // Assert
        img1.SortOrder.ShouldNotBe(img2.SortOrder);
    }

    [Fact]
    public void Should_Create_New_Instance_With_Updated_SortOrder()
    {
        // Arrange
        var original = new ListingImage(Guid.NewGuid(), "url", 1, "thumb", "alt", 100, "image/jpeg");
        var newSortOrder = 5;

        // Act
        var updated = original.WithSortOrder(newSortOrder);

        // Assert
        updated.ShouldNotBe(original); // Should be different instance
        updated.SortOrder.ShouldBe(newSortOrder);
        updated.ImageId.ShouldBe(original.ImageId);
        updated.Url.ShouldBe(original.Url);
        updated.ThumbnailUrl.ShouldBe(original.ThumbnailUrl);
        updated.AltText.ShouldBe(original.AltText);
        updated.FileSize.ShouldBe(original.FileSize);
        updated.ContentType.ShouldBe(original.ContentType);
    }
}
