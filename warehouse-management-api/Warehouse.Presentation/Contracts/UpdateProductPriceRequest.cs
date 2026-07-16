using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Contracts;

public class UpdateProductPriceRequest
{
    /// <example>50</example>
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}