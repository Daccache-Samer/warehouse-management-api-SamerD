using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using warehouse_management_api.Contracts;

namespace Warehouse.Api.UnitTests.ProductService;

public class GetExpiringProductsRequestTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(366)]
    public void WithinDays_ShouldFailValidation_WhenOutOfRange(int withinDays)
    {
        var request = new GetExpiringProductsRequest { WithinDays = withinDays };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request, new ValidationContext(request), results, validateAllProperties: true);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void WithinDays_ShouldDefaultTo30()
    {
        new GetExpiringProductsRequest().WithinDays.Should().Be(30);
    }
}