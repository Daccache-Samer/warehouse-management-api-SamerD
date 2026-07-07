using warehouse_management_api.Models;

namespace warehouse_management_api.Data;

public static class FakeSuppliers
{
    public static List<Supplier> Suppliers { get; set; } = new()
    {
        new Supplier
        {
            Id = "ae7a00f9-8908-40fd-aa56-3fa39ca3adee", Name = "TechSupply Co",
            Country = "USA", ContactEmail = "contact@techsupply.com",
            PhoneNumber = "+1-555-0101", IsActive = true
        },
        new Supplier
        {
            Id = "18bd6614-06c3-4f88-ae4d-7f313136e8d9", Name = "OfficeGear Ltd",
            Country = "UK", ContactEmail = "sales@officegear.co.uk",
            PhoneNumber = "+44-20-7946-0102", IsActive = true
        },
        new Supplier
        {
            Id = "77af2912-6606-4b84-b878-e89da888f7a7", Name = "DisplayTech",
            Country = "South Korea", ContactEmail = "info@displaytech.kr",
            PhoneNumber = "+82-2-555-0103", IsActive = true
        },
    };
}