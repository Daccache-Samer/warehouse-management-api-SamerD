using Microsoft.AspNetCore.Mvc;
using warehouse_management_api.Models;
using warehouse_management_api.Contracts;
using warehouse_management_api.Data;

namespace warehouse_management_api.Controllers;


[ApiController]
[Route("api/products" )]
public class ProductsController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public ProductsController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }
    //1. Get all products 
    [HttpGet()]
    public IActionResult GetAllProducts([FromQuery] bool onlyAvailable = false)
    {
        var products = FakeWarehouseStore.Products.AsEnumerable();
        if (onlyAvailable)
        {
            products = products.Where(p => !p.IsArchived && p.QuantityInStock > 0);
        }

        var result = products.OrderByDescending(p => p.CreatedAt).ToList();
        return Ok(result);
    }
    //2. Get product by id
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProductById([FromRoute] string id)
    {
        var product = FakeWarehouseStore.Products.AsEnumerable().FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound("Product not found");
        }
        return Ok(product);
    }
    //3. Search products
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string? name, [FromQuery] string? supplier)
    {
        var products = FakeWarehouseStore.Products.AsEnumerable();
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(supplier))
        {
            return BadRequest("Product name and Supplier name cannot be empty");
        }
        if (!string.IsNullOrEmpty(supplier))
        {
            products = products.Where(p => p.SupplierName.Contains(supplier, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrEmpty(name))
        { 
            products = products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
        return Ok(products.ToList());
    }
    //4. Create product 
    [HttpPost()]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        bool duplicateSku =
            FakeWarehouseStore.Products.Any(p => p.SKU.Equals(request.SKU, StringComparison.OrdinalIgnoreCase));
        if (duplicateSku)
        {
            return Conflict("SKU already exists");
        }

        var product = new Product
        {
            Id = Guid.NewGuid()
                .ToString(),
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            Price = request.Price,
            QuantityInStock = request.QuantityInStock,
            SupplierName = request.SupplierName,
            ExpiryDate = request.ExpiryDate,
            IsArchived = false,
            CreatedAt =  DateTime.UtcNow,
            LastUpdatedAt =  DateTime.UtcNow
        };
        FakeWarehouseStore.Products.Add(product);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }
    //5. Update quantity
    [HttpPost("{id}/quantity")]
    public IActionResult UpdateQuantity([FromRoute] string? id, [FromBody] UpdateProductQuantityRequest request)
    {
        var product = FakeWarehouseStore.Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            return NotFound($"Product with id '{id}' was not found.");
        }

        if (request.QuantityInStock < 0)
        {
            return BadRequest("Quantity cannot be negative.");
        }

        product.QuantityInStock = request.QuantityInStock;
        product.LastUpdatedAt = DateTime.UtcNow;

        return Ok(product);
    }
    //6. Update price
    [HttpPost("{id}/price")]
    public IActionResult UpdatePrice([FromRoute] string? id, [FromBody] UpdateProductPriceRequest request)
    {
        var product = FakeWarehouseStore.Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            return NotFound($"Product with id '{id}' was not found.");
        }

        if (request.Price <= 0)
        {
            return BadRequest("Price cannot be equal  or less than 0.");
        }
        Console.WriteLine($"Price updated from {product.Price} to {request.Price}");
        product.Price = request.Price;
        product.LastUpdatedAt = DateTime.UtcNow;

        return Ok(product);
    }
    //7. Upload image 
    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadImage(
        [FromRoute] string? id,
        IFormFile? file)
    {
        var product = FakeWarehouseStore.Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            return NotFound($"Product with id '{id}' was not found.");
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
        }

        const long maxSizeBytes = 2 * 1024 * 1024; // 2 MB
        if (file.Length > maxSizeBytes)
        {
            return BadRequest("File size must not exceed 2 MB.");
        }

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var productImage = new ProductImage
        {
            ProductId = product.Id,
            FileName = fileName,
            FilePath = $"/uploads/{fileName}"
        };
        product.LastUpdatedAt = DateTime.UtcNow;

        return Ok(productImage);
    }
    //8. Delete product 
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct([FromRoute] string? id)
    {
        var product = FakeWarehouseStore.Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            return NotFound($"Product with id '{id}' was not found.");
        }

        product.IsArchived = true;
        product.LastUpdatedAt = DateTime.UtcNow;

        return NoContent();
    }
    //9. Get warehouse server time 
    [HttpGet("server-time")]
    public IActionResult GetServerTime(
        [FromHeader(Name = "Accept-Language")] string? acceptLanguage)
    {
        var culture = (acceptLanguage ?? "en-US").Split(',')[0].Trim();

        string formatted = culture switch
        {
            "fr-FR" => DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss",
                new System.Globalization.CultureInfo("fr-FR")),
            "ar-LB" => DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss",
                new System.Globalization.CultureInfo("ar-LB")),
            _ => DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt",
                new System.Globalization.CultureInfo("en-US"))
        };

        return Ok(new { language = culture, serverTime = formatted });
    }
    //assign supplier to product
    [HttpPost("{id}/assign-supplier/{supplierId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AssignSupplier([FromRoute] string id, [FromRoute] string supplierId)
    {
        var product = FakeWarehouseStore.Products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            return NotFound($"Product with id '{id}' was not found.");
        }

        if (!_supplierService.Exists(supplierId))
        {
            return NotFound($"Supplier with id '{supplierId}' was not found.");
        }

        if (product.IsArchived)
        {
            return BadRequest("Cannot assign a supplier to an archived product.");
        }

        product.SupplierId = supplierId;
        product.LastUpdatedAt = DateTime.UtcNow;

        return Ok(product);
    }
}