using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Products.Queries.SearchProducts;

public class SearchProductsHandler(
    IProductRepository productRepository,
    ISupplierRepository supplierRepository)
    : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.Supplier))
        {
            throw new ValidationException("Product name and Supplier name cannot both be empty.");
        }

        var products = (await productRepository.GetAllAsync(cancellationToken)).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            products = products.Where(p =>
                p.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Supplier))
        {
            var suppliers = await supplierRepository.GetAllAsync(cancellationToken);
            var matchingSupplierIds = suppliers
                .Where(s => s.Name.Contains(request.Supplier, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id)
                .ToHashSet();

            products = products.Where(p => p.SupplierId is not null && matchingSupplierIds.Contains(p.SupplierId));
        }

        return products.Select(ProductDto.FromDomain).ToList();
    }
}