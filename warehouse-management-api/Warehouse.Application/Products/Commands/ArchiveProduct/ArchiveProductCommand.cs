using MediatR;

namespace Warehouse.Application.Products.Commands.ArchiveProduct;

public abstract record ArchiveProductCommand(string ProductId) : IRequest;