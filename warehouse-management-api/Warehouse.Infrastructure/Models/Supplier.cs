using System;
using System.Collections.Generic;

namespace Warehouse.Infrastructure.Models;

public partial class Supplier
{
    public string SupplierId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
