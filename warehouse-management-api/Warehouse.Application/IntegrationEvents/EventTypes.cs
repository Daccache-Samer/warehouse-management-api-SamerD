namespace Warehouse.Application.IntegrationEvents;

public static class EventTypes
{
    public const string StockLow = "stock.low";
    public const string StockAdjusted = "stock.adjusted";
    public const string ProductCreated = "product.created";
    public const string FileUploaded = "file.uploaded";
}