using Microsoft.AspNetCore.Hosting;
using RestaurantMS.Application.Common.Interfaces;

namespace RestaurantMS.Infrastructure.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _environment;

    private static readonly Dictionary<string, byte[][]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", [ [ 0xFF, 0xD8, 0xFF ] ] },
        { ".jpeg", [ [ 0xFF, 0xD8, 0xFF ] ] },
        { ".png", [ [ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A ] ] },
        { ".webp", [ [ 0x52, 0x49, 0x46, 0x46 ] ] } // RIFF header
    };

    public LocalFileStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!IsValidImageSignature(fileStream, extension))
        {
            throw new InvalidOperationException("File content signature does not match allowed image types (JPG, PNG, WEBP).");
        }

        if (fileStream.Length > 2 * 1024 * 1024)
        {
            throw new InvalidOperationException("File size exceeds the 2 MB limit.");
        }

        fileStream.Position = 0;

        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var targetFolder = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(targetFolder);

        var newFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetFolder, newFileName);

        using (var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        return $"/uploads/{folder}/{newFileName}";
    }

    public async Task<string> SaveDocumentAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!IsValidDocumentSignature(fileStream, extension))
        {
            throw new InvalidOperationException("File content signature does not match allowed invoice types (JPG, PNG, WEBP, PDF).");
        }

        if (fileStream.Length > 5 * 1024 * 1024)
        {
            throw new InvalidOperationException("Invoice attachment size exceeds the 5 MB limit.");
        }

        fileStream.Position = 0;

        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var targetFolder = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(targetFolder);

        var newFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetFolder, newFileName);

        using (var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        return $"/uploads/{folder}/{newFileName}";
    }

    public static bool IsValidDocumentSignature(Stream stream, string extension)
    {
        if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var header = new byte[4];
            stream.Position = 0;
            var read = stream.Read(header, 0, header.Length);
            stream.Position = 0;
            return read == 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46; // %PDF
        }

        return IsValidImageSignature(stream, extension);
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var trimmed = relativePath.TrimStart('/', '\\');
        var fullPath = Path.Combine(webRoot, trimmed);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult(false);

        var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var trimmed = relativePath.TrimStart('/', '\\');
        var fullPath = Path.Combine(webRoot, trimmed);

        return Task.FromResult(File.Exists(fullPath));
    }

    public static bool IsValidImageSignature(Stream stream, string extension)
    {
        if (!FileSignatures.TryGetValue(extension, out var signatures))
        {
            return false;
        }

        var maxSignatureLength = signatures.Max(s => s.Length);
        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            maxSignatureLength = 12; // WebP requires checking bytes 0-3 (RIFF) and 8-11 (WEBP)
        }

        var header = new byte[maxSignatureLength];
        stream.Position = 0;
        var bytesRead = stream.Read(header, 0, header.Length);
        stream.Position = 0;

        if (bytesRead < maxSignatureLength && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            // Bytes 0-3 must be RIFF (0x52, 0x49, 0x46, 0x46)
            // Bytes 8-11 must be WEBP (0x57, 0x45, 0x42, 0x50)
            if (bytesRead < 12) return false;
            var isRiff = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46;
            var isWebp = header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
            return isRiff && isWebp;
        }

        return signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));
    }
}
