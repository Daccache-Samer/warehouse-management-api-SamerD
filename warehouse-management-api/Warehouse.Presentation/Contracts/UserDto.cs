using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Contracts;

public class UserDto
{
    [Required, EmailAddress] public required string? Email { get; init; } = "";

    [Required] public required string? Password { get; init; } = "";
}