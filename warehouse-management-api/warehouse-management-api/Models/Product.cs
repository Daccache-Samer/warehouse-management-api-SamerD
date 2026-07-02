using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Models;

//creating product domain model
public class Product
{
    [StringLength(50, MinimumLength = 5, ErrorMessage = "invalid Id")]
    public required string Id { get; set; } = string.Empty;// since .net6 string cannot be nullable so we can add "=string.empty" to give a default value to the string. we can also add the required field or give it a type string? to make it nullable. 
    [StringLength(50,MinimumLength = 5,ErrorMessage = "invalid Name")]
    public required string Name { get; set; } = string.Empty;
    [StringLength(10,MinimumLength = 2,ErrorMessage = "invalid SKU")]
    public string SKU { get; set; } =  string.Empty;
    [StringLength(500,MinimumLength = 5,ErrorMessage = "invalid Description")]
    public string Description { get; set; }  = string.Empty;
    
    public required int Price{get;set;}
    public required int QuantityInStock { get; set; }
    [StringLength(50,MinimumLength = 5,ErrorMessage = "invalid SupplierName")]
    public required string SupplierName{ get; set; } = string.Empty;
    public required DateTime ExpiryDate { get; set; }
    public required bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}