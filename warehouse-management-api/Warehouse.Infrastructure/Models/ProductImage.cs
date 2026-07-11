using System;
using System.Collections.Generic;

namespace Warehouse.Infrastructure.Models;

public partial class ProductImage
{
    public string Id { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
