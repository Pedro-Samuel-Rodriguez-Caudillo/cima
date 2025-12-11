using System;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para el Value Object ListingImage con estructura de lista enlazada
/// </summary>
public sealed class ListingImageTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_ListingImage_With_Valid_Data()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var url = "https://storage.example.com/listings/image1.jpg";
        var altText = "Vista frontal de la propiedad";
        var fileSize = 2048000L; // 2MB
        var contentType = "image/jpeg";

        // Act
        var image = new ListingImage(
            imageId: imageId,
            url: url,
            altText: altText,
            fileSize: fileSize,
            contentType: contentType
        );

        // Assert
        image.ShouldNotBeNull();
        image.ImageId.ShouldBe(imageId);
        image.Url.ShouldBe(url);
        image.PreviousImageId.ShouldBeNull();
        image.NextImageId.ShouldBeNull();
        image.AltText.ShouldBe(altText);
        image.FileSize.ShouldBe(fileSize);
        image.ContentType.ShouldBe(contentType);
    }

    [Fact]
    public void Should_Create_ListingImage_With_Links()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var previousId = Guid.NewGuid();
        var nextId = Guid.NewGuid();

        // Act
        var image = new ListingImage(
            imageId: imageId,
            url: "https://example.com/image.jpg",
            altText: "Test image",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: previousId,
            nextImageId: nextId
        );

        // Assert
        image.PreviousImageId.ShouldBe(previousId);
        image.NextImageId.ShouldBe(nextId);
    }

    [Fact]
    public void Should_Create_First_Image_In_List()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var nextId = Guid.NewGuid();

        // Act
        var image = new ListingImage(
            imageId: imageId,
            url: "https://example.com/image.jpg",
            altText: "First image",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: null,
            nextImageId: nextId
        );

        // Assert
        image.PreviousImageId.ShouldBeNull(); // Es la primera
        image.NextImageId.ShouldBe(nextId);
    }

    [Fact]
    public void Should_Create_Last_Image_In_List()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var previousId = Guid.NewGuid();

        // Act
        var image = new ListingImage(
            imageId: imageId,
            url: "https://example.com/image.jpg",
            altText: "Last image",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: previousId,
            nextImageId: null
        );

        // Assert
        image.PreviousImageId.ShouldBe(previousId);
        image.NextImageId.ShouldBeNull(); // Es la última
    }

    [Fact]
    public void Should_Update_Links_Immutably()
    {
        // Arrange
        var originalImage = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg"
        );
        var newPrevId = Guid.NewGuid();
        var newNextId = Guid.NewGuid();

        // Act
        var updatedImage = originalImage.WithLinks(newPrevId, newNextId);

        // Assert
        updatedImage.ShouldNotBe(originalImage); // Inmutable - nuevo objeto
        updatedImage.PreviousImageId.ShouldBe(newPrevId);
        updatedImage.NextImageId.ShouldBe(newNextId);
        updatedImage.ImageId.ShouldBe(originalImage.ImageId);
        updatedImage.Url.ShouldBe(originalImage.Url);
        
        // Original no cambia
        originalImage.PreviousImageId.ShouldBeNull();
        originalImage.NextImageId.ShouldBeNull();
    }

    [Fact]
    public void Should_Update_Previous_Link_Only()
    {
        // Arrange
        var originalImage = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            nextImageId: Guid.NewGuid()
        );
        var newPrevId = Guid.NewGuid();

        // Act
        var updatedImage = originalImage.WithPreviousImage(newPrevId);

        // Assert
        updatedImage.PreviousImageId.ShouldBe(newPrevId);
        updatedImage.NextImageId.ShouldBe(originalImage.NextImageId); // Mantiene next
    }

    [Fact]
    public void Should_Update_Next_Link_Only()
    {
        // Arrange
        var originalImage = new ListingImage(
            imageId: Guid.NewGuid(),
            url: "https://example.com/image.jpg",
            altText: "Test",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: Guid.NewGuid()
        );
        var newNextId = Guid.NewGuid();

        // Act
        var updatedImage = originalImage.WithNextImage(newNextId);

        // Assert
        updatedImage.NextImageId.ShouldBe(newNextId);
        updatedImage.PreviousImageId.ShouldBe(originalImage.PreviousImageId); // Mantiene prev
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_Url_Is_Null()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => new ListingImage(
            imageId: Guid.NewGuid(),
            url: null!,
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
            altText: "Test",
            fileSize: 1024000L,
            contentType: null!
        );

        // Assert
        image.ContentType.ShouldBe("image/jpeg");
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
            altText: "Test",
            fileSize: 1024000L,
            contentType: contentType
        );

        // Assert
        image.ContentType.ShouldBe(contentType);
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
            altText: "Test",
            fileSize: fileSize,
            contentType: "image/jpeg"
        );

        // Assert
        image.FileSize.ShouldBe(fileSize);
    }

    [Fact]
    public void Should_Create_Linked_List_Chain()
    {
        // Arrange
        var img1Id = Guid.NewGuid();
        var img2Id = Guid.NewGuid();
        var img3Id = Guid.NewGuid();

        // Act - Create chain: img1 -> img2 -> img3
        var img1 = new ListingImage(
            imageId: img1Id,
            url: "https://example.com/1.jpg",
            thumbnailUrl: "",
            altText: "First",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: null,
            nextImageId: img2Id);
        
        var img2 = new ListingImage(
            imageId: img2Id,
            url: "https://example.com/2.jpg",
            thumbnailUrl: "",
            altText: "Middle",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: img1Id,
            nextImageId: img3Id);
        
        var img3 = new ListingImage(
            imageId: img3Id,
            url: "https://example.com/3.jpg",
            thumbnailUrl: "",
            altText: "Last",
            fileSize: 1024000L,
            contentType: "image/jpeg",
            previousImageId: img2Id,
            nextImageId: null);

        // Assert
        // img1 is first
        img1.PreviousImageId.ShouldBeNull();
        img1.NextImageId.ShouldBe(img2Id);
        
        // img2 is middle
        img2.PreviousImageId.ShouldBe(img1Id);
        img2.NextImageId.ShouldBe(img3Id);
        
        // img3 is last
        img3.PreviousImageId.ShouldBe(img2Id);
        img3.NextImageId.ShouldBeNull();
    }
}
