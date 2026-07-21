using System.ComponentModel.DataAnnotations;

namespace Warehouse.Infrastructure.Firebase;

public class UserDto
{
    [Required, EmailAddress]
    public required string? Email { get; init; }

    [Required]
    public required string? Password { get; init; }
}