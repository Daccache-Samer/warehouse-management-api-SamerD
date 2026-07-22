namespace Warehouse.DomainWarehouse.Domain.Common;

public interface IFileStorage
{
    Task<FileStorageResult> UploadAsync(
        string category, string ownerId, Stream content, string fileName, string contentType,
        CancellationToken ct = default);

    Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey, CancellationToken ct = default);

    Task DeleteAsync(string objectKey, CancellationToken ct = default);
}

public record FileStorageResult(string ObjectKey, string FileName);