using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Suppliers.Commands.CreateSupplier;
using Warehouse.Application.Suppliers.Commands.DeactivateSupplier;
using Warehouse.Application.Suppliers.Queries.GetSuppliersById;
using Warehouse.Application.Suppliers.Queries.ListSuppliers;
using warehouse_management_api.Contracts;
using Warehouse.Application.Suppliers.Commands.AddSupplierDocument;
using Warehouse.Application.Suppliers.Commands.DeleteSupplierDocument;
using Warehouse.Application.Suppliers.Queries.DownloadSupplierDocument;
using Warehouse.Application.Suppliers.ViewModels;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ApiUser")]
    public async Task<ActionResult<SupplierViewModel>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListSuppliersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ApiUser")]
    public async Task<ActionResult<SupplierViewModel>> GetById([FromRoute] string id,CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSupplierByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SupplierViewModel>> Create([FromBody] CreateSupplierRequest request,CancellationToken cancellationToken = default)
    {
        var command = new CreateSupplierCommand(request.Name, request.Country, request.ContactEmail, request.PhoneNumber);
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.SupplierId }, result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SupplierViewModel>> Deactivate([FromRoute] string id,CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeactivateSupplierCommand(id), cancellationToken);
        return NoContent();
    }
    [HttpPost("{id}/documents")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SupplierViewModel>> AddDocument(
        [FromRoute] string id, IFormFile file, CancellationToken cancellationToken = default)
    {
        var command = new AddSupplierDocumentCommand(
            id, file.OpenReadStream(), file.FileName, file.Length, file.ContentType);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}/documents/{documentId}")]
    [Authorize(Policy = "ApiUser")]
    public async Task<IActionResult> DownloadDocument(
        [FromRoute] string id, [FromRoute] string documentId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DownloadSupplierDocumentQuery(id, documentId), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpDelete("{id}/documents/{documentId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteDocument(
        [FromRoute] string id, [FromRoute] string documentId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new DeleteSupplierDocumentCommand(id, documentId), cancellationToken);
        return NoContent();
    }
}