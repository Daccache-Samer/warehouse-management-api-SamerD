using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Validation;

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext context)
    {
        if (value is DateTime date && date <= DateTime.Now)
        {
            return new ValidationResult("Date must be in the future");
        }

        return ValidationResult.Success;
    }
}