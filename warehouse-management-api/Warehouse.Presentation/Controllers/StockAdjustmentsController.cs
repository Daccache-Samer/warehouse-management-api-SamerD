using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Products.Commands.AdjustProductStock;
using Warehouse.Application.Products.ViewModels;
using warehouse_management_api.Contracts;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/stock-adjustments")]
public class StockAdjustmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductViewModel>> Create(
        [FromBody] AdjustProductStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AdjustProductStockCommand(
            request.ProductId, request.AdjustmentType, request.Quantity, request.Reason);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}