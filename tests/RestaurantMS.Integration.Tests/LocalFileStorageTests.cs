using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using RestaurantMS.Infrastructure.Services;
using Xunit;

namespace RestaurantMS.Integration.Tests;

public class LocalFileStorageTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();

    public LocalFileStorageTests()
    {
        _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
    }

    [Fact]
    public void IsValidImageSignature_ValidJpg_ReturnsTrue()
    {
        // JPG magic bytes: FF D8 FF
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        using var stream = new MemoryStream(bytes);

        var result = LocalFileStorage.IsValidImageSignature(stream, ".jpg");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageSignature_ValidPng_ReturnsTrue()
    {
        // PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = new MemoryStream(bytes);

        var result = LocalFileStorage.IsValidImageSignature(stream, ".png");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageSignature_ValidWebp_ReturnsTrue()
    {
        // WEBP magic bytes: RIFF (0x52, 0x49, 0x46, 0x46) + 4 bytes size + WEBP (0x57, 0x42, 0x50)
        var bytes = new byte[] {
            0x52, 0x49, 0x46, 0x46, // RIFF
            0x24, 0x00, 0x00, 0x00, // Size
            0x57, 0x45, 0x42, 0x50  // WEBP
        };
        using var stream = new MemoryStream(bytes);

        var result = LocalFileStorage.IsValidImageSignature(stream, ".webp");

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(".exe")]
    [InlineData(".pdf")]
    [InlineData(".svg")]
    [InlineData(".gif")]
    public void IsValidImageSignature_UnsupportedExtension_ReturnsFalse(string extension)
    {
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
        using var stream = new MemoryStream(bytes);

        var result = LocalFileStorage.IsValidImageSignature(stream, extension);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageSignature_CorruptedOrWrongSignature_ReturnsFalse()
    {
        // Extension says .png but header is plain text
        var bytes = "NOT_A_PNG_FILE"u8.ToArray();
        using var stream = new MemoryStream(bytes);

        var result = LocalFileStorage.IsValidImageSignature(stream, ".png");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveFileAsync_FileSizeGreaterThan2MB_ThrowsInvalidOperationException()
    {
        var storage = new LocalFileStorage(_envMock.Object);

        // Valid JPG header followed by > 2MB data
        var largeSize = 2 * 1024 * 1024 + 1024; // 2 MB + 1 KB
        var largeData = new byte[largeSize];
        largeData[0] = 0xFF;
        largeData[1] = 0xD8;
        largeData[2] = 0xFF;

        using var stream = new MemoryStream(largeData);

        var act = async () => await storage.SaveFileAsync(stream, "large_logo.jpg", "logos");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*2 MB*");
    }

    [Fact]
    public async Task SaveFileAsync_InvalidSignature_ThrowsInvalidOperationException()
    {
        var storage = new LocalFileStorage(_envMock.Object);

        var bytes = "InvalidContent"u8.ToArray();
        using var stream = new MemoryStream(bytes);

        var act = async () => await storage.SaveFileAsync(stream, "fake.jpg", "logos");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*signature*");
    }
}
