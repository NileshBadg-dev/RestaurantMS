namespace RestaurantMS.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<string> SaveDocumentAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}
