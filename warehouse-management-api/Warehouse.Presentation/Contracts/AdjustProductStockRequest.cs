using System.ComponentModel.DataAnnotations;
using Warehouse.Application.Products.Commands.AdjustProductStock;

namespace warehouse_management_api.Contracts;

public class AdjustProductStockRequest : IValidatableObject
{
    /// <example>7b18b8d7-1db7-4a8b-b049-be7341162d15</example>
    [Required(ErrorMessage = "Product id is required.")]
    public string ProductId { get; set; } = string.Empty;
    /// <example>Increase</example>
    [Required(ErrorMessage = "Adjustment type is required.")]
    public StockAdjustmentType AdjustmentType { get; set; }
    /// <example>100</example>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
    /// <example>A shipment came in</example>
    [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters.")]
    public string? Reason { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AdjustmentType == StockAdjustmentType.Decrease && string.IsNullOrWhiteSpace(Reason))
        {
            yield return new ValidationResult(
                "Reason is required when decreasing stock.",
                [nameof(Reason)]);
        }
    }
}