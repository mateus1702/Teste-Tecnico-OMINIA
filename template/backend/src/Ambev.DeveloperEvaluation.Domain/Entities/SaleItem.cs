using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    private SaleItem()
    {
    }

    public static SaleItem Create(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId is required.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ProductName is required.");

        if (unitPrice <= 0)
            throw new DomainException("UnitPrice must be greater than zero.");

        var (discount, total) = SalePricingService.CalculateItemTotals(unitPrice, quantity);

        return new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = SalePricingService.RoundMoney(unitPrice),
            Discount = discount,
            TotalAmount = total,
            IsCancelled = false
        };
    }

    public void UpdateDetails(string productName, int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException("Cannot update a cancelled item.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("ProductName is required.");

        if (unitPrice <= 0)
            throw new DomainException("UnitPrice must be greater than zero.");

        var (discount, total) = SalePricingService.CalculateItemTotals(unitPrice, quantity);

        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = SalePricingService.RoundMoney(unitPrice);
        Discount = discount;
        TotalAmount = total;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Item is already cancelled.");

        IsCancelled = true;
    }

    public void AssignToSale(Guid saleId) => SaleId = saleId;
}
