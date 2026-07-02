namespace warehouse_management_api.Models;

//creating product domain model
public class Product
{
    public string Id { get; set; } = string.Empty;// since .net6 string cannot be nullable so we can add "=string.empty" to give a default value to the string. we can also add the required field or give it a type string? to make it nullable. 
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } =  string.Empty;
    public string Description { get; set; }  = string.Empty;
    public int Price{get;set;}
    public int QuantityInStock { get; set; }
    public string SupplierName{ get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}