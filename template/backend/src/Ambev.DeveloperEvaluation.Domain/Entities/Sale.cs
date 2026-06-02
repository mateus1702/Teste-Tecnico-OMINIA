using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public long SaleNumber { get; private set; }
    public DateTime SaleDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<SaleItem> Items { get; private set; } = new List<SaleItem>();

    private Sale()
    {
    }

    public static Sale Create(
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName,
        IEnumerable<SaleItem> items)
    {
        ValidateExternalIdentity(customerId, customerName, "Customer");
        ValidateExternalIdentity(branchId, branchName, "Branch");

        var itemList = items?.ToList() ?? [];
        if (itemList.Count == 0)
            throw new DomainException("Sale must contain at least one item.");

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleDate = saleDate,
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            BranchId = branchId,
            BranchName = branchName.Trim(),
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        sale.ReplaceItems(itemList);
        return sale;
    }

    public void Update(
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName,
        IEnumerable<SaleItemUpdateInput> items)
    {
        if (IsCancelled)
            throw new DomainException("Cannot update a cancelled sale.");

        ValidateExternalIdentity(customerId, customerName, "Customer");
        ValidateExternalIdentity(branchId, branchName, "Branch");

        var itemList = items?.ToList() ?? [];
        if (itemList.Count == 0)
            throw new DomainException("Sale must contain at least one item.");

        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        BranchId = branchId;
        BranchName = branchName.Trim();
        UpdatedAt = DateTime.UtcNow;

        ReplaceItems(itemList);
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelItem(Guid itemId)
    {
        if (IsCancelled)
            throw new DomainException("Cannot cancel item on a cancelled sale.");

        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Sale item with ID {itemId} was not found.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSaleNumber(long saleNumber) => SaleNumber = saleNumber;

    private void ReplaceItems(IReadOnlyCollection<SaleItem> items)
    {
        Items.Clear();

        foreach (var item in items)
        {
            item.AssignToSale(Id);
            Items.Add(item);
        }

        RecalculateTotal();
    }

    private void ReplaceItems(IReadOnlyCollection<SaleItemUpdateInput> items)
    {
        var duplicateIds = items
            .Where(i => i.Id.HasValue)
            .GroupBy(i => i.Id!.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            throw new DomainException("Update payload contains duplicate sale item IDs.");

        var incomingIds = items
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        var existingById = Items.ToDictionary(i => i.Id);

        var removedItems = Items
            .Where(i => !incomingIds.Contains(i.Id))
            .ToList();

        foreach (var removedItem in removedItems)
        {
            Items.Remove(removedItem);
        }

        foreach (var incoming in items)
        {
            if (incoming.Id.HasValue && existingById.TryGetValue(incoming.Id.Value, out var existingItem))
            {
                existingItem.UpdateDetails(incoming.ProductName, incoming.Quantity, incoming.UnitPrice);
            }
            else
            {
                var newItem = SaleItem.Create(
                    incoming.ProductId,
                    incoming.ProductName,
                    incoming.Quantity,
                    incoming.UnitPrice);
                newItem.AssignToSale(Id);
                Items.Add(newItem);
            }
        }

        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = SalePricingService.RoundMoney(
            Items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount));
    }

    private static void ValidateExternalIdentity(Guid id, string name, string entityLabel)
    {
        if (id == Guid.Empty)
            throw new DomainException($"{entityLabel}Id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException($"{entityLabel}Name is required.");
    }
}

public class SaleItemUpdateInput
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
