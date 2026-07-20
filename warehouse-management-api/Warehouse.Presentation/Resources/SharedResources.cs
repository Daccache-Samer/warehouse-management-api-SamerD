namespace warehouse_management_api.Resources;

public partial class SharedResources
{
    public const string ValidationErrorsOccurred = nameof(ValidationErrorsOccurred);
    public const string UnexpectedError = nameof(UnexpectedError);
    public const string NameRequired = nameof(NameRequired);
    public const string NameLength = nameof(NameLength);
    public const string SkuRequired = nameof(SkuRequired);
    public const string SkuLength = nameof(SkuLength);
    public const string DescriptionLength = nameof(DescriptionLength);
    public const string PriceRange = nameof(PriceRange);
    public const string QuantityRange = nameof(QuantityRange);
    public const string ExpiryDateFuture = nameof(ExpiryDateFuture);
}