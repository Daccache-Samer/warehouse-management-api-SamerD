using MediatR;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Suppliers.Commands.CreateSupplier;
using Warehouse.Application.Suppliers.Commands.DeactivateSupplier;
using Warehouse.Application.Suppliers.Queries.GetSuppliersById;
using Warehouse.Application.Suppliers.Queries.ListSuppliers;
using warehouse_management_api.Contracts;
using Warehouse.Application.Suppliers.ViewModels;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SupplierViewModel>> GetAll()
    {
        var result = await mediator.Send(new ListSuppliersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierViewModel>> GetById([FromRoute] string id)
    {
        var result = await mediator.Send(new GetSupplierByIdQuery(id));
        return result is null ? NotFound($"Supplier with id '{id}' was not found.") : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierViewModel>> Create([FromBody] CreateSupplierRequest request)
    {
        var command = new CreateSupplierCommand(request.Name, request.Country, request.ContactEmail, request.PhoneNumber);
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.SupplierId }, result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<SupplierViewModel>> Deactivate([FromRoute] string id)
    {
        await mediator.Send(new DeactivateSupplierCommand(id));
        return NoContent();
    }
}