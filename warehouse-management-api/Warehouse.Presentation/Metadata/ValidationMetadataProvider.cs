using System.ComponentModel.DataAnnotations;
using System.Reflection;
using warehouse_management_api.Contracts;

namespace warehouse_management_api.Metadata;

public record PropertyValidationMetadata(string PropertyName, string PropertyType, bool IsRequired, IReadOnlyList<string> Rules);

public record DtoValidationMetadata(string DtoName, IReadOnlyList<PropertyValidationMetadata> Properties);

public class ValidationMetadataProvider
{
    private static readonly IReadOnlyDictionary<string, Type> AllowedDtos =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(CreateProductRequest)] = typeof(CreateProductRequest),
            [nameof(CreateSupplierRequest)] = typeof(CreateSupplierRequest),
            [nameof(UpdateProductPriceRequest)] = typeof(UpdateProductPriceRequest),
            [nameof(UpdateProductQuantityRequest)] = typeof(UpdateProductQuantityRequest),
            [nameof(AdjustProductStockRequest)] = typeof(AdjustProductStockRequest),
        };

    public static IReadOnlyCollection<string> AllowedDtoNames => AllowedDtos.Keys.ToList();

    public DtoValidationMetadata? GetMetadata(string dtoName)
    {
        if (!AllowedDtos.TryGetValue(dtoName, out var type))
        {
            return null;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(DescribeProperty)
            .ToList();

        return new DtoValidationMetadata(type.Name, properties);
    }

    private static PropertyValidationMetadata DescribeProperty(PropertyInfo property)
    {
        var attributes = property.GetCustomAttributes<ValidationAttribute>(inherit: true).ToList();

        var isRequired = attributes.Any(a => a is RequiredAttribute);
        var rules = attributes
            .Where(a => a is not RequiredAttribute)
            .Select(DescribeRule)
            .ToList();

        return new PropertyValidationMetadata(property.Name, property.PropertyType.Name, isRequired, rules);
    }

    private static string DescribeRule(ValidationAttribute attribute) => attribute switch
    {
        RangeAttribute range => $"Range({range.Minimum}, {range.Maximum})",
        StringLengthAttribute length => $"StringLength(min: {length.MinimumLength}, max: {length.MaximumLength})",
        EmailAddressAttribute => "EmailAddress",
        PhoneAttribute => "Phone",
        _ => attribute.GetType().Name.Replace("Attribute", string.Empty)
    };
}