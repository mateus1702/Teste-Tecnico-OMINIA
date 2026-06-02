namespace Ambev.DeveloperEvaluation.Application.Sales.Messages;

public class SaleCreatedMessage
{
    public Guid SaleId { get; set; }
    public long SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OccurredAt { get; set; }
}
