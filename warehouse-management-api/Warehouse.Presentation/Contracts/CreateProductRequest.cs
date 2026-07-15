using System.ComponentModel.DataAnnotations;
using warehouse_management_api.Validation;
namespace warehouse_management_api.Contracts;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "SKU must be between 2 and 50 characters.")]
    public string Sku { get; set; } = string.Empty;
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock cannot be negative.")]
    public int QuantityInStock { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    [FutureDate(ErrorMessage = "Expiry date must be in the future.")]
    public DateTime ExpiryDate { get; set; }
}