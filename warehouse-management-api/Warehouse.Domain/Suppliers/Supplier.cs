using Warehouse.DomainWarehouse.Domain.Exceptions;
namespace Warehouse.DomainWarehouse.Domain.Suppliers;

public class Supplier
{
    public string SupplierId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Supplier() { }

    public static Supplier Create(string name, string country, string contactEmail, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        return new Supplier
        {
            SupplierId = Guid.NewGuid().ToString(),
            Name = name,
            Country = country,
            ContactEmail = contactEmail,
            PhoneNumber = phoneNumber,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}