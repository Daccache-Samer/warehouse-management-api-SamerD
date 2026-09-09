using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Contracts;

public class GetExpiringProductsRequest
{
    [Range(1, 365, ErrorMessage = "WithinDays must be between 1 and 365.")]
    public int WithinDays { get; set; } = 30;
}