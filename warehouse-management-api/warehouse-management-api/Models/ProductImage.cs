using System.ComponentModel.DataAnnotations;

namespace warehouse_management_api.Models;

public class ProductImage
{
    [StringLength(50,MinimumLength = 5,ErrorMessage = "invalid ProductId")]
    private string ProductId { get; set; } = string.Empty;
    [StringLength(500,MinimumLength = 1,ErrorMessage = "invalid Filename")]
    private string FileName { get; set; } = string.Empty;
    [StringLength(500,MinimumLength = 5,ErrorMessage = "invalid Filepath")]
    private string FilePath { get; set; } = string.Empty;
}