using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.ArchiveProduct;

public class ArchiveProductHandler(IProductRepository productRepository) : IRequestHandler<ArchiveProductCommand>
{
    public async Task Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.Archive();

        await productRepository.UpdateAsync(product, cancellationToken);
    }
}