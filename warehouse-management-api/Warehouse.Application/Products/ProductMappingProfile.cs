using AutoMapper;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products;

public class ProductMappingProfile : Profile
{ 
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductViewModel>();
        CreateMap<Product, ExpiringProductViewModel>()
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.DaysUntilExpiry,
                opt => opt.MapFrom(src => (src.ExpiryDate.Date - DateTime.UtcNow.Date).Days));
    }
}