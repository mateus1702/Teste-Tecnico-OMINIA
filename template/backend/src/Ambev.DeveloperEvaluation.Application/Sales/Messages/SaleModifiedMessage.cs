namespace Ambev.DeveloperEvaluation.Application.Sales.Messages;

public class SaleModifiedMessage
{
    public Guid SaleId { get; set; }
    public long SaleNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OccurredAt { get; set; }
}
