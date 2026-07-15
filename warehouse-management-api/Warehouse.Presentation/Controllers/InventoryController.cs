using MediatR;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.InventoryDashboard.Queries;
using Warehouse.Application.InventoryDashboard.ViewModels;

namespace warehouse_management_api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<InventoryDashboardViewModel>> GetDashboard(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInventoryDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}