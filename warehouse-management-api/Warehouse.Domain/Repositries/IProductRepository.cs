using Warehouse.DomainWarehouse.Domain.Models;
namespace Warehouse.DomainWarehouse.Domain.Repositries;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(string id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}