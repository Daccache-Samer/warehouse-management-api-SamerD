using System.ComponentModel.DataAnnotations;
namespace warehouse_management_api.Contracts;

public class CreateSupplierRequest
{
    /// <example>Acer</example>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
    public string Name { get; set; } = string.Empty;
    /// <example>Lebanon</example>
    [Required(ErrorMessage = "Country is required.")]
    [StringLength(100, ErrorMessage = "Country must not exceed 100 characters.")]
    public string Country { get; set; } = string.Empty;
    /// <example>Acer@outlook.com</example>
    [Required(ErrorMessage = "Contact email is required.")]
    [EmailAddress(ErrorMessage = "Contact email must be a valid email address.")]
    public string ContactEmail { get; set; } = string.Empty;
    /// <example>70123456</example>
    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Phone number must be a valid phone number.")]
    public string PhoneNumber { get; set; } = string.Empty;
}