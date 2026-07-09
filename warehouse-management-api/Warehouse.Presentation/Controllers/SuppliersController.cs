using MediatR;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Suppliers.Commands.CreateSupplier;
using Warehouse.Application.Suppliers.Commands.DeactivateSupplier;
using Warehouse.Application.Suppliers.Queries.GetSuppliersById;
using Warehouse.Application.Suppliers.Queries.ListSuppliers;
using warehouse_management_api.Contracts;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new ListSuppliersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id));
        return result is null ? NotFound($"Supplier with id '{id}' was not found.") : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var command = new CreateSupplierCommand(request.Name, request.Country, request.ContactEmail, request.PhoneNumber);
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate([FromRoute] string id)
    {
        await _mediator.Send(new DeactivateSupplierCommand(id));
        return NoContent();
    }
}