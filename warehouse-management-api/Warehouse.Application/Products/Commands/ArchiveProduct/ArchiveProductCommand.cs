using MediatR;

namespace Warehouse.Application.Products.Commands.ArchiveProduct;

public record ArchiveProductCommand(string ProductId) : IRequest;