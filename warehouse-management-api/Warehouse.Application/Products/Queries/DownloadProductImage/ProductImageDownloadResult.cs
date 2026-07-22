namespace Warehouse.Application.Products.Queries.DownloadProductImage;

public record ProductImageDownloadResult(Stream Content, string ContentType, string FileName);