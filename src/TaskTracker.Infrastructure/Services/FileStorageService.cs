using Microsoft.Extensions.Configuration;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/files";
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueFileName);
        
        await using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream, cancellationToken);
        
        return uniqueFileName;
    }

    public async Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storagePath);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {storagePath}");

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storagePath);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(string storagePath, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        // For local file storage, we return a simple URL
        // In production with S3/Azure Blob, this would generate a presigned URL
        var url = $"{_baseUrl}/{storagePath}";
        return Task.FromResult(url);
    }
}
