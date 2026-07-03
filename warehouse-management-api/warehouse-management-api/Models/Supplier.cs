namespace warehouse_management_api.Models;

public class Supplier
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Country { get; set; }
    public required string ContactEmail { get; set; }
    public required string PhoneNumber { get; set; }
    public required bool IsActive { get; set; }
}