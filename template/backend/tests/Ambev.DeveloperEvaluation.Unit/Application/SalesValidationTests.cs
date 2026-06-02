using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SalesValidationTests
{
    [Fact]
    public void CreateSaleCommandValidator_WhenQuantityAboveTwenty_IsInvalid()
    {
        var validator = new CreateSaleCommandValidator();
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new SaleItemInput
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Keyboard",
                    Quantity = 21,
                    UnitPrice = 100m
                }
            ]
        };

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ListSalesQueryValidator_WhenPageIsZero_IsInvalid()
    {
        var validator = new ListSalesQueryValidator();
        var result = validator.Validate(new ListSalesQuery { Page = 0, Size = 10 });
        result.IsValid.Should().BeFalse();
    }
}
