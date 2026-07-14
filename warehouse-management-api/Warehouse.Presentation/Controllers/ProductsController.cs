using MediatR;
using Microsoft.AspNetCore.Mvc;
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
using Warehouse.Application.Products.ViewModels;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProductViewModel>> GetAll([FromQuery] bool onlyAvailable = false,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListProductsQuery(onlyAvailable), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductViewModel>> GetById([FromRoute] string id,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result is null ? NotFound($"Product with id '{id}' was not found.") : Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ProductViewModel>> Search([FromQuery] string? name, [FromQuery] string? supplier,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new SearchProductsQuery(name, supplier), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductViewModel>> Create([FromBody] CreateProductRequest request,CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            request.Name, request.Sku, request.Description,
            request.Price, request.QuantityInStock, request.ExpiryDate);

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id}/quantity")]
    public async Task<ActionResult<ProductViewModel>> UpdateQuantity([FromRoute] string id, [FromBody] UpdateProductQuantityRequest request,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateProductQuantityCommand(id, request.QuantityInStock), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/price")]
    public async Task<ActionResult<ProductViewModel>> UpdatePrice([FromRoute] string id, [FromBody] UpdateProductPriceRequest request,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateProductPriceCommand(id, request.Price), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/image")]
    public async Task<ActionResult<ProductViewModel>> UploadImage([FromRoute] string id, IFormFile? file,CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        await using var stream = file.OpenReadStream();
        var command = new AddProductImageCommand(id, stream, file.FileName, file.Length);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ProductViewModel>> Delete([FromRoute] string id,CancellationToken cancellationToken = default)
    {
        await mediator.Send(new ArchiveProductCommand(id), cancellationToken);
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
    public async Task<ActionResult<ProductViewModel>> AssignSupplier([FromRoute] string id, [FromRoute] string supplierId,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new AssignSupplierToProductCommand(id, supplierId), cancellationToken);
        return Ok(result);
    }
}