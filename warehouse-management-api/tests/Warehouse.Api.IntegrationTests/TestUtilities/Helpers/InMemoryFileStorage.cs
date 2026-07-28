using System.Collections.Concurrent;
using Warehouse.DomainWarehouse.Domain.Common;

namespace Warehouse.Api.IntegrationTests.TestUtilities.Helpers;

public sealed class InMemoryFileStorage : IFileStorage
{
    private readonly ConcurrentDictionary<string, (byte[] Content, string ContentType)> _store = new();

    public async Task<FileStorageResult> UploadAsync(
        string category, string ownerId, Stream content, string fileName, string contentType,
        CancellationToken ct = default)
    {
        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, ct);
        var objectKey = $"{category}/{ownerId}/{Guid.NewGuid()}-{fileName}";
        _store[objectKey] = (memoryStream.ToArray(), contentType);
        return new FileStorageResult(objectKey, fileName);
    }

    public Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        var (bytes, contentType) = _store[objectKey];
        return Task.FromResult<(Stream, string)>((new MemoryStream(bytes), contentType));
    }

    public Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        _store.TryRemove(objectKey, out _);
        return Task.CompletedTask;
    }
}