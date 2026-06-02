using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Applies quantity-based discount rules for sale items.
/// </summary>
public static class SalePricingService
{
    public const int MaxQuantityPerProduct = 20;
    public const int MinQuantityForDiscount = 4;
    public const int MinQuantityForHighDiscount = 10;

    public static decimal GetDiscountPercent(int quantity)
    {
        ValidateQuantity(quantity);

        return quantity switch
        {
            >= MinQuantityForHighDiscount => 0.20m,
            >= MinQuantityForDiscount => 0.10m,
            _ => 0m
        };
    }

    public static (decimal DiscountAmount, decimal TotalAmount) CalculateItemTotals(decimal unitPrice, int quantity)
    {
        ValidateQuantity(quantity);

        var discountPercent = GetDiscountPercent(quantity);
        var subtotal = unitPrice * quantity;
        var discountAmount = RoundMoney(subtotal * discountPercent);
        var totalAmount = RoundMoney(subtotal - discountAmount);

        return (discountAmount, totalAmount);
    }

    public static void ValidateQuantity(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Quantity must be at least 1.");

        if (quantity > MaxQuantityPerProduct)
            throw new DomainException($"Cannot sell more than {MaxQuantityPerProduct} identical items.");
    }

    public static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
