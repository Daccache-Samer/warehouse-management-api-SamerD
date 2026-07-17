using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Commands.AdjustProductStock;

public record AdjustProductStockCommand(
    string ProductId,
    StockAdjustmentType AdjustmentType,
    int Quantity,
    string? Reason) : IRequest<ProductViewModel>;