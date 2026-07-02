namespace warehouse_management_api.Contracts;

//creating product domain model
public class Product
{
    private string Id { get; set; } = string.Empty;// since .net6 string cannot be nullable so we can add "=string.empty" to give a default value to the string. we can also add the required field or give it a type string? to make it nullable. 
    private string Name { get; set; } = string.Empty;
    private string SKU { get; set; } =  string.Empty;
    private string Description { get; set; }  = string.Empty;
    private int Price{get;set;}
    private int QuantityInStock { get; set; }
    private string SupplierName{ get; set; } = string.Empty;
    private DateTime ExpiryDate { get; set; }
    private bool IsArchived { get; set; }
    private DateTime CreatedAt { get; set; }
    private DateTime LastUpdatedAt { get; set; }
}