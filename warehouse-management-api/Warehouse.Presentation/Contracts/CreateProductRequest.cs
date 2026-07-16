using System.ComponentModel.DataAnnotations;
using warehouse_management_api.Validation;
namespace warehouse_management_api.Contracts;

public class CreateProductRequest
{
    ///<example>laptop</example> 
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
    public string Name { get; set; } = string.Empty;
    ///<example>22</example>
    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "SKU must be between 2 and 50 characters.")]
    public string Sku { get; set; } = string.Empty;
    ///<example>A gaming laptop</example>
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;
    ///<example>500</example>
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
    ///<example>100</example>
    [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock cannot be negative.")]
    public int QuantityInStock { get; set; }
    ///<example>Acer</example>
    public string SupplierName { get; set; } = string.Empty;
    /// <example>2027-01-01T00:00:00Z</example>
    [FutureDate(ErrorMessage = "Expiry date must be in the future.")]
    public DateTime ExpiryDate { get; set; }
}