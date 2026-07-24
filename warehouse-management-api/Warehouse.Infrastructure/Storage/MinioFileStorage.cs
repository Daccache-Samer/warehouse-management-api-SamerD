using Minio;
using Minio.DataModel.Args;
using Warehouse.DomainWarehouse.Domain.Common;

namespace Warehouse.Infrastructure.Storage;

public class MinioFileStorage(IMinioClient client, string bucketName) : IFileStorage
{
    public async Task<FileStorageResult> UploadAsync(
        string category, string ownerId, Stream content, string fileName, string contentType,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var objectKey = $"{category}/{ownerId}/{Guid.NewGuid()}{extension}";

        await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType), ct);

        return new FileStorageResult(objectKey, fileName);
    }

    public async Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        var stat = await client.StatObjectAsync(new StatObjectArgs()
            .WithBucket(bucketName).WithObject(objectKey), ct);

        var memoryStream = new MemoryStream();
        await client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectKey)
            .WithCallbackStream(s => s.CopyTo(memoryStream)), ct);

        memoryStream.Position = 0;
        return (memoryStream, stat.ContentType);
    }
    public async Task DeleteAsync(string objectKey, CancellationToken ct = default) =>
        await client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName).WithObject(objectKey), ct);
}
