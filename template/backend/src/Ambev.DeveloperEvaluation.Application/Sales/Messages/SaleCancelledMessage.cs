namespace Ambev.DeveloperEvaluation.Application.Sales.Messages;

public class SaleCancelledMessage
{
    public Guid SaleId { get; set; }
    public long SaleNumber { get; set; }
    public DateTime OccurredAt { get; set; }
}
