using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Contracts;

public class SignUpUserDto
{
    [Required, EmailAddress]
    public required string? Email { get; init; }

    [Required]
    public required string? Password { get; init; }

    [Required, Compare(nameof(Password), ErrorMessage = "The passwords didn't match.")]
    public required string? ConfirmPassword { get; init; }
}