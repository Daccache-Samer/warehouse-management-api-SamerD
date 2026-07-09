using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _uploadsFolder;

    public LocalFileStorage(string webRootPath)
    {
        _uploadsFolder = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(_uploadsFolder);
    }

    public async Task<string> SaveAsync(string productId, Stream content, string fileName, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_uploadsFolder, uniqueName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await content.CopyToAsync(fileStream, ct);

        return $"/uploads/{uniqueName}";
    }
}