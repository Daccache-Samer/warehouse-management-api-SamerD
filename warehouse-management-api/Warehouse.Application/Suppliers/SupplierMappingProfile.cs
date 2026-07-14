using AutoMapper;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers;

public class SupplierMappingProfile : Profile
{
    public SupplierMappingProfile()
    {
        CreateMap<Supplier, SupplierViewModel>();
    }
}