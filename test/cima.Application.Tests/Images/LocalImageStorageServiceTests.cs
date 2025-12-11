using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Volo.Abp.DependencyInjection;
using cima;
using Xunit;
using cima.Images;

namespace cima.ApplicationTests.Images;

/// <summary>
/// Pruebas de integración para la implementación local de almacenamiento de imágenes.
/// Usa ImageSharp para generar una imagen en memoria y verifica URLs y archivos generados.
/// </summary>
public class LocalImageStorageServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IImageStorageService _storageService;
    private readonly IHostEnvironment _env;

    public LocalImageStorageServiceTests()
    {
        _storageService = GetRequiredService<IImageStorageService>();
        _env = GetRequiredService<IHostEnvironment>();
    }

    [Fact]
    public async Task UploadImageAsync_Should_Return_Urls_And_Create_Files()
    {
        // Arrange: crear imagen pequeña en memoria
        using var image = new Image<Rgba32>(width: 50, height: 50);

        await using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        ms.Position = 0;

        // Act
        UploadImageResult result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _storageService.UploadImageAsync(ms, "test.png", "listings");
        });

        // Assert: URLs no vacías
        result.Url.Should().NotBeNullOrWhiteSpace();
        result.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        // Assert: archivos creados en disco
        var originalPath = Path.Combine(_env.ContentRootPath, "wwwroot", result.Url.TrimStart('/'));
        var thumbPath = Path.Combine(_env.ContentRootPath, "wwwroot", result.ThumbnailUrl.TrimStart('/'));
        File.Exists(originalPath).Should().BeTrue();
        File.Exists(thumbPath).Should().BeTrue();

        // Cleanup
        TryDelete(originalPath);
        TryDelete(thumbPath);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignorar en tests
        }
    }
}
