using warehouse_management_api.Models;

namespace warehouse_management_api;

public static class FakeWarehouseStore
{
    public static List<Product> Products { set; get; } =
        new()
        {
            new Product
            {
                Id = "1ea627ad-cd70-4de2-9d59-c65a5442b9e2",
                Name = "laptop",
                SKU = "AA",
                Description = "Laptop",
                Price = 100,
                QuantityInStock = 10,
                SupplierName = "Lenovo",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "cfbddbc1-c0a0-4414-8c66-678923118cf0",
                Name = "GTA6",
                SKU = "BB",
                Description = "GTA6",
                Price = 80,
                QuantityInStock = 100,
                SupplierName = "Rockstar",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "98bc6c67-7e70-4f3d-91c1-b5e79afad9bb",
                Name = "PS5",
                SKU = "CC",
                Description = "PS5",
                Price = 400,
                QuantityInStock = 20,
                SupplierName = "Sony",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "fdb8659d-28c9-4190-98d3-d5549476ba7a",
                Name = "RAM",
                SKU = "DD",
                Description = "RAM",
                Price = 10000,
                QuantityInStock = 100,
                SupplierName = "Nvidia",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "a365845c-9c26-4cb3-aa67-4cdd59dbbe4a",
                Name = "pen",
                SKU = "EE",
                Description = "pen",
                Price = 10,
                QuantityInStock = 1000,
                SupplierName = "Bic",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "1192c0d3-e542-46a7-bdff-d7baa8adf6bd",
                Name = "Gaming Chairs",
                SKU = "FF",
                Description = "Gaming Chairs",
                Price = 200,
                QuantityInStock = 100,
                SupplierName = "Alienware",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "e31f293c-31ad-4421-85e0-5fd24cf1ee53",
                Name = "Bottle of water",
                SKU = "GG",
                Description = "Bottle of water",
                Price = 1,
                QuantityInStock = 1000,
                SupplierName = "Tannourine",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "5e2db995-1f14-4fb4-abae-49198637ccc1",
                Name = "textbook",
                SKU = "HH",
                Description = "textbook",
                Price = 10,
                QuantityInStock = 1000,
                SupplierName = "Bic",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "ff17297a-b575-44b4-b394-593cc31211d7",
                Name = "Phones",
                SKU = "II",
                Description = "Phones",
                Price = 400,
                QuantityInStock = 500,
                SupplierName = "Xiaomi",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            },
            new Product
            {
                Id = "76c46cf4-e8ba-4906-8cbe-5e5abcd1bfb4",
                Name = "sneakers",
                SKU = "JJ",
                Description = "sneakers",
                Price = 300,
                QuantityInStock = 1000,
                SupplierName = "Nike",
                ExpiryDate = DateTime.Now,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            }
        };
}