using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Given active sale When cancelling item Then total is recalculated")]
    public void CancelItem_ActiveSale_RecalculatesTotal()
    {
        var item1 = SaleItem.Create(Guid.NewGuid(), "Keyboard", 5, 100m);
        var item2 = SaleItem.Create(Guid.NewGuid(), "Mouse", 2, 50m);

        var sale = Sale.Create(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [item1, item2]);

        sale.TotalAmount.Should().Be(550m);

        sale.CancelItem(item2.Id);

        sale.Items.First(i => i.Id == item2.Id).IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(450m);
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws domain exception")]
    public void Update_CancelledSale_ThrowsDomainException()
    {
        var item = SaleItem.Create(Guid.NewGuid(), "Keyboard", 4, 100m);
        var sale = Sale.Create(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [item]);

        sale.Cancel();

        var act = () => sale.Update(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [new SaleItemUpdateInput
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Mouse",
                Quantity = 1,
                UnitPrice = 50m
            }]);

        act.Should().Throw<DomainException>()
            .WithMessage("*cancelled*");
    }

    [Fact(DisplayName = "Given sale with no items When creating Then throws domain exception")]
    public void Create_WithoutItems_ThrowsDomainException()
    {
        var act = () => Sale.Create(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            []);

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one item*");
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling item Then throws domain exception")]
    public void CancelItem_CancelledSale_ThrowsDomainException()
    {
        var item = SaleItem.Create(Guid.NewGuid(), "Keyboard", 4, 100m);
        var sale = Sale.Create(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [item]);

        sale.Cancel();

        var act = () => sale.CancelItem(item.Id);
        act.Should().Throw<DomainException>();
    }
}
