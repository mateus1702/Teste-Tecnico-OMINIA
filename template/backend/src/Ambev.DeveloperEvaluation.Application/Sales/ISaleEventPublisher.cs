using Ambev.DeveloperEvaluation.Application.Sales.Messages;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public interface ISaleEventPublisher
{
    Task PublishSaleCreatedAsync(SaleCreatedMessage message, CancellationToken cancellationToken = default);

    Task PublishSaleModifiedAsync(SaleModifiedMessage message, CancellationToken cancellationToken = default);

    Task PublishSaleCancelledAsync(SaleCancelledMessage message, CancellationToken cancellationToken = default);

    Task PublishItemCancelledAsync(ItemCancelledMessage message, CancellationToken cancellationToken = default);
}
