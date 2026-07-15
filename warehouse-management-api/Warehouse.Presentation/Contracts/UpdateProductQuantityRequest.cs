using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Contracts;

public class UpdateProductQuantityRequest
{
    [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock cannot be negative.")]
    public int QuantityInStock { get; set; }
}