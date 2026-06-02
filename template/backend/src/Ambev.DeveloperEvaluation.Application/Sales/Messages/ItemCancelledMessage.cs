namespace Ambev.DeveloperEvaluation.Application.Sales.Messages;

public class ItemCancelledMessage
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
    public long SaleNumber { get; set; }
    public DateTime OccurredAt { get; set; }
}
