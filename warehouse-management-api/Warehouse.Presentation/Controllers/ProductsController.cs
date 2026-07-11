using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Products.Commands.AddProductImage;
using Warehouse.Application.Products.Commands.ArchiveProduct;
using Warehouse.Application.Products.Commands.AssignSupplierToProduct;
using Warehouse.Application.Products.Commands.CreateProduct;
using Warehouse.Application.Products.Commands.UpdateProductPrice;
using Warehouse.Application.Products.Commands.UpdateProductQuantity;
using Warehouse.Application.Products.Queries.GetProductById;
using Warehouse.Application.Products.Queries.ListProducts;
using Warehouse.Application.Products.Queries.SearchProducts;
using warehouse_management_api.Contracts;
using Warehouse.Infrastructure.Models;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator, WarehouseDbFirstContext dbcontext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyAvailable = false)
    {
        var result = await mediator.Send(new ListProductsQuery(onlyAvailable));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id));
        return result is null ? NotFound($"Product with id '{id}' was not found.") : Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] string? supplier)
    {
        var result = await mediator.Send(new SearchProductsQuery(name, supplier));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var command = new CreateProductCommand(
            request.Name, request.Sku, request.Description,
            request.Price, request.QuantityInStock, request.ExpiryDate);

        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id}/quantity")]
    public async Task<IActionResult> UpdateQuantity([FromRoute] string id, [FromBody] UpdateProductQuantityRequest request)
    {
        var result = await mediator.Send(new UpdateProductQuantityCommand(id, request.QuantityInStock));
        return Ok(result);
    }

    [HttpPost("{id}/price")]
    public async Task<IActionResult> UpdatePrice([FromRoute] string id, [FromBody] UpdateProductPriceRequest request)
    {
        var result = await mediator.Send(new UpdateProductPriceCommand(id, request.Price));
        return Ok(result);
    }

    [HttpPost("{id}/image")]
    public async Task<IActionResult> UploadImage([FromRoute] string id, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        await using var stream = file.OpenReadStream();
        var command = new AddProductImageCommand(id, stream, file.FileName, file.Length);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        await mediator.Send(new ArchiveProductCommand(id));
        return NoContent();
    }

    [HttpGet("server-time")]
    public IActionResult GetServerTime([FromHeader(Name = "Accept-Language")] string? acceptLanguage)
    {
        var culture = (acceptLanguage ?? "en-US").Split(',')[0].Trim();

        string formatted = culture switch
        {
            "fr-FR" => DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("fr-FR")),
            "ar-LB" => DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("ar-LB")),
            _ => DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss tt", new System.Globalization.CultureInfo("en-US"))
        };

        return Ok(new { language = culture, serverTime = formatted });
    }

    [HttpPost("{id}/assign-supplier/{supplierId}")]
    public async Task<IActionResult> AssignSupplier([FromRoute] string id, [FromRoute] string supplierId)
    {
        var result = await mediator.Send(new AssignSupplierToProductCommand(id, supplierId));
        return Ok(result);
    }

    [HttpGet("by-supplier")]
    public async Task<IActionResult> GetBySupplier([FromQuery] string supplierName, [FromQuery] string sortOrder)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
        {
            return BadRequest("Supplier name not found");
        }

        var query = dbcontext.Products
            .Include(p => p.Supplier)
            .Where(p => p.Supplier != null && p.Supplier.Name == supplierName);
        query = sortOrder == "asc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
        return Ok(await query.ToListAsync());
    }
    [HttpGet("group-by-expiry-year")]
    public async Task<IActionResult> GroupByExpiryYear()
    {
        var query = dbcontext.Products
            .GroupBy(p => p.ExpiryDate.Year);

        return Ok(await query.ToListAsync());
    }

    [HttpGet("group-by-expiry-year-and-Country")]
    public async Task<IActionResult> GroupByExpiryYearAndCountry()
    {
        var query = dbcontext.Products
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            .GroupBy(p => new { p.ExpiryDate.Year, p.Supplier.Country });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return Ok(await query.ToListAsync()); 
    }

    [HttpGet("total-count")]
    public async Task<IActionResult> GetTotalCount()
    {
        var query = await dbcontext.Products.CountAsync();
        return Ok(value: query);
    }
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("pageNumber and pageSize must both be at least 1.");
        }

        var totalCount = await dbcontext.Products.CountAsync();

        var items = await dbcontext.Products
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            pageNumber,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            items
        });
    }
}