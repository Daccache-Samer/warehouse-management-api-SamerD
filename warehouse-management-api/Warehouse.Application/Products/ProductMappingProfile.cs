using AutoMapper;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products;

public class ProductMappingProfile : Profile
{ 
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductViewModel>();
    }
}