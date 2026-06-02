using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

public class SalePricingServiceTests
{
    private const decimal UnitPrice = 100m;

    [Theory(DisplayName = "Given quantity below 4 When calculating Then no discount is applied")]
    [InlineData(1)]
    [InlineData(3)]
    public void CalculateItemTotals_BelowMinimumDiscount_ReturnsNoDiscount(int quantity)
    {
        var (discount, total) = SalePricingService.CalculateItemTotals(UnitPrice, quantity);

        discount.Should().Be(0m);
        total.Should().Be(UnitPrice * quantity);
    }

    [Theory(DisplayName = "Given quantity between 4 and 9 When calculating Then 10% discount is applied")]
    [InlineData(4, 40, 360)]
    [InlineData(9, 90, 810)]
    public void CalculateItemTotals_MediumTier_AppliesTenPercentDiscount(int quantity, decimal expectedDiscount, decimal expectedTotal)
    {
        var (discount, total) = SalePricingService.CalculateItemTotals(UnitPrice, quantity);

        discount.Should().Be(expectedDiscount);
        total.Should().Be(expectedTotal);
    }

    [Theory(DisplayName = "Given quantity between 10 and 20 When calculating Then 20% discount is applied")]
    [InlineData(10, 200, 800)]
    [InlineData(20, 400, 1600)]
    public void CalculateItemTotals_HighTier_AppliesTwentyPercentDiscount(int quantity, decimal expectedDiscount, decimal expectedTotal)
    {
        var (discount, total) = SalePricingService.CalculateItemTotals(UnitPrice, quantity);

        discount.Should().Be(expectedDiscount);
        total.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Given quantity above 20 When calculating Then throws domain exception")]
    public void CalculateItemTotals_AboveMaximum_ThrowsDomainException()
    {
        var act = () => SalePricingService.CalculateItemTotals(UnitPrice, 21);

        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }
}
