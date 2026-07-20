using Warehouse.Application.Products.Queries.GetProductById;
using Warehouse.Application.Products.Queries.ListProducts;

namespace Warehouse.Application.Products;

public static class ProductCacheKeys
{
    public static string ById(string id) => $"{nameof(GetProductByIdQuery)}";
    public static string List(bool onlyAvailable) => $"{nameof(ListProductsHandler)}";
    public static readonly string[] AllListVariants = [List(true), List(false)];
}