using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Models;

public class ProductImage
{
    [StringLength(50,MinimumLength = 5,ErrorMessage = "invalid ProductId")]
    public string ProductId { get; set; } = string.Empty;
    [StringLength(500,MinimumLength = 1,ErrorMessage = "invalid Filename")]
    public string FileName { get; set; } = string.Empty;
    [StringLength(500,MinimumLength = 5,ErrorMessage = "invalid Filepath")]
    public string FilePath { get; set; } = string.Empty;
}