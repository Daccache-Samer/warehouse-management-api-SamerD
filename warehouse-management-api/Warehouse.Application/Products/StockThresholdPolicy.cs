namespace Warehouse.Application.Products;

public static class StockThresholdPolicy // this is so i dont implement the same threshold in both updateproductquantity and adjustproductstock.
{
    public static bool CrossedIntoLowStock(int previousQuantity, int newQuantity, int threshold) =>
        previousQuantity > threshold && newQuantity <= threshold;
}