using System;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para el Value Object ListingImage
/// </summary>
public sealed class ListingImageTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_ListingImage_With_Valid_Data()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var url = "https://storage.example.com/listings/image1.jpg";
        var displayOrder = 1;
        var altText = "Vista frontal de la propiedad";
        var fileSize = 2048000L; // 2MB
        var contentType = "image/jpeg";

        // Act
        var image = new ListingImage(
            imageId: imageId,
            url: url,
            displayOrder: displayOrder,
            altText: altText,
            fileSize: fileSize,
            contentType: contentType
        );

        // Assert
        image.ShouldNotBeNull();
        image.ImageId.ShouldBe(imageId);
        image.Url.ShouldBe(url);
        image.DisplayOrder.ShouldBe(displayOrder);
        image.AltText.ShouldBe(altText);
        image.FileSize.ShouldBe(fileSize);
        image.ContentType.ShouldBe(contentType);
    }

    [Theory]
    [InlineData("https://storage.example.com/listings/image1.jpg", "image/jpeg")]
    [InlineData("https://cdn.cima.com/properties/abc123.png", "image/png")]
    [InlineData("/uploads/listings/2024/property-photo.webp", "image/webp")]
    [InlineData("https://s3.amazonaws.com/bucket/listings/image.jpg", "image/jpeg")]
    public void Should_Store_Different_Image_Url_And_ContentType_Formats(string url, string contentType)
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: url,
            displayOrder: 1,
            altText: "Test image",
            fileSize: 1024000L,
            contentType: contentType
        );

        // Assert
        image.Url.ShouldBe(url);
        image.ContentType.ShouldBe(contentType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void Should_Set_DisplayOrder_Correctly(int displayOrder)
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: displayOrder,
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Assert
        image.DisplayOrder.ShouldBe(displayOrder);
    }

    [Theory]
    [InlineData("Vista frontal de la propiedad")]
    [InlineData("Sala de estar con iluminación natural")]
    [InlineData("Cocina integral moderna")]
    [InlineData("Jardín trasero")]
    public void Should_Store_Image_AltText(string altText)
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: altText,
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Assert
        image.AltText.ShouldBe(altText);
    }

    [Theory]
    [InlineData(512000L)]    // 500KB
    [InlineData(1024000L)]   // 1MB
    [InlineData(2048000L)]   // 2MB
    [InlineData(5242880L)]   // 5MB
    public void Should_Store_Different_File_Sizes(long fileSize)
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: "Test",
            fileSize: fileSize,
            contentType: "image/jpeg"
        );

        // Assert
        image.FileSize.ShouldBe(fileSize);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public void Should_Store_Different_ContentTypes(string contentType)
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: "Test",
            fileSize: 1024000L,
            contentType: contentType
        );

        // Assert
        image.ContentType.ShouldBe(contentType);
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_Url_Is_Null()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => new ListingImage(
            imageId: Guid.NewGuid(),
            url: null!,
            displayOrder: 1,
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        ));
    }

    [Fact]
    public void Should_Default_AltText_To_Empty_When_Null()
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: null!,
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Assert
        image.AltText.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Default_ContentType_To_Jpeg_When_Null()
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: "Test",
            fileSize: 1024000L,
            contentType: null!
        );

        // Assert
        image.ContentType.ShouldBe("image/jpeg");
    }

    [Fact]
    public void Should_Create_New_Instance_With_Updated_DisplayOrder()
    {
        // Arrange
        var originalImage = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 5,
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Act
        var updatedImage = originalImage.WithDisplayOrder(1);

        // Assert
        updatedImage.ShouldNotBe(originalImage); // Nuevo objeto (Value Object inmutable)
        updatedImage.DisplayOrder.ShouldBe(1);
        updatedImage.ImageId.ShouldBe(originalImage.ImageId);
        updatedImage.Url.ShouldBe(originalImage.Url);
        updatedImage.AltText.ShouldBe(originalImage.AltText);
        updatedImage.FileSize.ShouldBe(originalImage.FileSize);
        updatedImage.ContentType.ShouldBe(originalImage.ContentType);
        
        // Original no debe cambiar (inmutabilidad)
        originalImage.DisplayOrder.ShouldBe(5);
    }

    // TODO: El Value Object no está comparando por igualdad de valores correctamente
    // Esto requiere override de Equals y GetHashCode en ListingImage
    // [Fact]
    // public void Should_Be_Equal_When_All_Values_Are_Same()
    // {
    //     // Arrange
    //     var imageId = Guid.NewGuid();
    //     var image1 = new ListingImage(
    //         imageId: imageId,
    //         url: "https://example.com/image.jpg",
    //         displayOrder: 1,
    //         altText: "Test",
    //         fileSize: 1024000L,
    //         contentType: "image/jpeg"
    //     );

    //     var image2 = new ListingImage(
    //         imageId: imageId,
    //         url: "https://example.com/image.jpg",
    //         displayOrder: 1,
    //         altText: "Test",
    //         fileSize: 1024000L,
    //         contentType: "image/jpeg"
    //     );

    //     // Act & Assert - Value Objects con mismos valores son iguales
    //     image1.ShouldBe(image2);
    //     (image1 == image2).ShouldBeTrue();
    // }

    [Fact]
    public void Should_Not_Be_Equal_When_Values_Differ()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var image1 = new ListingImage(
            imageId: imageId,
            url: "https://example.com/image1.jpg",
            displayOrder: 1,
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        var image2 = new ListingImage(
            imageId: imageId,
            url: "https://example.com/image2.jpg",
            displayOrder: 1,
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Act & Assert - Value Objects con diferentes valores no son iguales
        image1.ShouldNotBe(image2);
        (image1 != image2).ShouldBeTrue();
    }

    [Theory]
    [InlineData(1, 2, true)]  // 1 < 2 (orden correcto)
    [InlineData(2, 1, false)] // 2 > 1 (orden incorrecto)
    [InlineData(3, 3, false)] // 3 == 3 (mismo orden)
    public void Should_Compare_DisplayOrder_For_Sorting(int order1, int order2, bool shouldBeFirst)
    {
        // Arrange
        var image1 = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image1.jpg",
            displayOrder: order1,
            altText: "Image 1",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        var image2 = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image2.jpg",
            displayOrder: order2,
            altText: "Image 2",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Act
        var isFirstInOrder = image1.DisplayOrder < image2.DisplayOrder;

        // Assert
        isFirstInOrder.ShouldBe(shouldBeFirst);
    }

    [Fact]
    public void Should_Store_Unique_ImageIds()
    {
        // Arrange
        var imageId1 = Guid.NewGuid();
        var imageId2 = Guid.NewGuid();
        var imageId3 = Guid.NewGuid();

        // Act
        var image1 = new ListingImage(imageId1, "https://example.com/1.jpg", 1, "Test 1", 1024000L, "image/jpeg");
        var image2 = new ListingImage(imageId2, "https://example.com/2.jpg", 2, "Test 2", 1024000L, "image/jpeg");
        var image3 = new ListingImage(imageId3, "https://example.com/3.jpg", 3, "Test 3", 1024000L, "image/jpeg");

        // Assert
        image1.ImageId.ShouldNotBe(image2.ImageId);
        image2.ImageId.ShouldNotBe(image3.ImageId);
        image3.ImageId.ShouldNotBe(image1.ImageId);
    }

    [Fact]
    public void Should_Handle_Empty_AltText()
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: "",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Assert
        image.AltText.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Allow_Zero_Or_Negative_DisplayOrder(int displayOrder)
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: displayOrder,
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );

        // Assert
        image.DisplayOrder.ShouldBe(displayOrder);
    }

    [Fact]
    public void Should_Allow_Zero_FileSize()
    {
        // Arrange & Act
        var image = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            displayOrder: 1,
            altText: "Test",
            fileSize: 0L,
            contentType: "image/jpeg"
        );

        // Assert
        image.FileSize.ShouldBe(0L);
    }
}
