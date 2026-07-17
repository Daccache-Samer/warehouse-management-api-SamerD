using MediatR;
using Warehouse.Application.InventoryDashboard.ViewModels;

namespace Warehouse.Application.InventoryDashboard.Queries;

public record GetInventoryDashboardQuery : IRequest<InventoryDashboardViewModel>;