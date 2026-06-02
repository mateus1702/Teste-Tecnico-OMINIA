using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.WebApi.Messaging.Handlers;

public class SaleCreatedMessageHandler : IHandleMessages<SaleCreatedMessage>
{
    private readonly ILogger<SaleCreatedMessageHandler> _logger;

    public SaleCreatedMessageHandler(ILogger<SaleCreatedMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedMessage message)
    {
        _logger.LogInformation(
            "SaleCreated event received: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            message.SaleId,
            message.SaleNumber,
            message.TotalAmount);

        return Task.CompletedTask;
    }
}

public class SaleModifiedMessageHandler : IHandleMessages<SaleModifiedMessage>
{
    private readonly ILogger<SaleModifiedMessageHandler> _logger;

    public SaleModifiedMessageHandler(ILogger<SaleModifiedMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedMessage message)
    {
        _logger.LogInformation(
            "SaleModified event received: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            message.SaleId,
            message.SaleNumber,
            message.TotalAmount);

        return Task.CompletedTask;
    }
}

public class SaleCancelledMessageHandler : IHandleMessages<SaleCancelledMessage>
{
    private readonly ILogger<SaleCancelledMessageHandler> _logger;

    public SaleCancelledMessageHandler(ILogger<SaleCancelledMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledMessage message)
    {
        _logger.LogInformation(
            "SaleCancelled event received: SaleId={SaleId}, SaleNumber={SaleNumber}",
            message.SaleId,
            message.SaleNumber);

        return Task.CompletedTask;
    }
}

public class ItemCancelledMessageHandler : IHandleMessages<ItemCancelledMessage>
{
    private readonly ILogger<ItemCancelledMessageHandler> _logger;

    public ItemCancelledMessageHandler(ILogger<ItemCancelledMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledMessage message)
    {
        _logger.LogInformation(
            "ItemCancelled event received: SaleId={SaleId}, ItemId={ItemId}, SaleNumber={SaleNumber}",
            message.SaleId,
            message.ItemId,
            message.SaleNumber);

        return Task.CompletedTask;
    }
}
