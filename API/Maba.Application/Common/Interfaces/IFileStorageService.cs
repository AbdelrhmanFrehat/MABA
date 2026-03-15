namespace Maba.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileAsync(string filePath);
    Task<string> GetFileUrlAsync(string filePath);
    Task<string> GenerateThumbnailAsync(string filePath, int width, int height);
    Task<bool> FileExistsAsync(string filePath);
}

