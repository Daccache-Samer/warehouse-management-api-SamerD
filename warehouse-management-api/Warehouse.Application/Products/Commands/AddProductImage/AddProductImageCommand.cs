using MediatR;

namespace Warehouse.Application.Products.Commands.AddProductImage;

public record AddProductImageCommand(
    string ProductId,
    Stream Content,
    string FileName,
    long Length) : IRequest<ProductDto>;