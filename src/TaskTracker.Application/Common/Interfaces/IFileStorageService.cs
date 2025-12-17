namespace TaskTracker.Application.Common.Interfaces;

/// <summary>
/// Interface for file storage service
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string storagePath, TimeSpan expiry, CancellationToken cancellationToken = default);
}
