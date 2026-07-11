using System;
using System.Collections.Generic;

namespace Warehouse.Infrastructure.Models;

public partial class Product
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int QuantityInStock { get; set; }

    public string? SupplierId { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual Supplier? Supplier { get; set; }
}
