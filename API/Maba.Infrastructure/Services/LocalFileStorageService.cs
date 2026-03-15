using Maba.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Maba.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:Local:BasePath"] ?? "wwwroot/uploads";
        _baseUrl = configuration["FileStorage:Local:BaseUrl"] ?? "/uploads";
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        var directory = string.IsNullOrEmpty(folder) ? _basePath : Path.Combine(_basePath, folder);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(directory, uniqueFileName);
        var relativePath = string.IsNullOrEmpty(folder) 
            ? uniqueFileName 
            : Path.Combine(folder, uniqueFileName).Replace("\\", "/");

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter);
        }

        return relativePath;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, filePath);
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream?> GetFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, filePath);
            if (File.Exists(fullPath))
            {
                return await Task.FromResult<Stream>(File.OpenRead(fullPath));
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public Task<string> GetFileUrlAsync(string filePath)
    {
        var url = $"{_baseUrl}/{filePath}".Replace("\\", "/");
        return Task.FromResult(url);
    }

    public Task<string> GenerateThumbnailAsync(string filePath, int width, int height)
    {
        // TODO: Implement thumbnail generation using ImageSharp or similar
        // For now, return the original file path
        return Task.FromResult(filePath);
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }
}

