using System.ComponentModel.DataAnnotations;
using warehouse_management_api.Resources;
using warehouse_management_api.Validation;
namespace warehouse_management_api.Contracts;

public class CreateProductRequest
{
    ///<example>laptop</example> 
    [Required(ErrorMessage = SharedResources.NameRequired)]
    [StringLength(200, MinimumLength = 2, ErrorMessage = SharedResources.NameLength)]
    public string Name { get; set; } = string.Empty;
    ///<example>22</example>
    [Required(ErrorMessage = SharedResources.SkuRequired)]
    [StringLength(50, MinimumLength = 2, ErrorMessage = SharedResources.SkuLength)]
    public string Sku { get; set; } = string.Empty;
    ///<example>A gaming laptop</example>
    [StringLength(1000, ErrorMessage = SharedResources.DescriptionLength)]
    public string Description { get; set; } = string.Empty;
    ///<example>500</example>
    [Range(0.01, double.MaxValue, ErrorMessage = SharedResources.PriceRange)]
    public decimal Price { get; set; }
    ///<example>100</example>
    [Range(0, int.MaxValue, ErrorMessage = SharedResources.QuantityRange)]
    public int QuantityInStock { get; set; }
    ///<example>Acer</example>
    public string SupplierName { get; set; } = string.Empty;
    /// <example>2027-01-01T00:00:00Z</example>
    [FutureDate(ErrorMessage = SharedResources.ExpiryDateFuture)]
    public DateTime ExpiryDate { get; set; }
}