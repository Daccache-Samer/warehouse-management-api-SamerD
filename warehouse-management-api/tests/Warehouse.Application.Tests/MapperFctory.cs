using AutoMapper;
using Warehouse.Application.Products;
using Warehouse.Application.Suppliers;

namespace Warehouse.Application.Tests;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<SupplierMappingProfile>();
        });
        return configuration.CreateMapper();
    }
}