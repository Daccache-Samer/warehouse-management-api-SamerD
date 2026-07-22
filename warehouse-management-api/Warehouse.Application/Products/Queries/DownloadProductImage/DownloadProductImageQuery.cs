using MediatR;

namespace Warehouse.Application.Products.Queries.DownloadProductImage;

public record DownloadProductImageQuery(string ProductId, string FileName) : IRequest<ProductImageDownloadResult>;