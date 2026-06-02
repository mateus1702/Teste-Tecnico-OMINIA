using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.WebApi.Messaging;

public class RebusSaleEventPublisher : ISaleEventPublisher
{
    private readonly IBus _bus;

    public RebusSaleEventPublisher(IBus bus)
    {
        _bus = bus;
    }

    public Task PublishSaleCreatedAsync(SaleCreatedMessage message, CancellationToken cancellationToken = default) =>
        _bus.Publish(message);

    public Task PublishSaleModifiedAsync(SaleModifiedMessage message, CancellationToken cancellationToken = default) =>
        _bus.Publish(message);

    public Task PublishSaleCancelledAsync(SaleCancelledMessage message, CancellationToken cancellationToken = default) =>
        _bus.Publish(message);

    public Task PublishItemCancelledAsync(ItemCancelledMessage message, CancellationToken cancellationToken = default) =>
        _bus.Publish(message);
}
