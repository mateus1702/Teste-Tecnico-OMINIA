using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Ambev.DeveloperEvaluation.WebApi.Messaging.Handlers;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Ambev.DeveloperEvaluation.WebApi.Messaging;

public static class RebusConfiguration
{
    public const string InputQueueName = "ambev.sales";

    public static IServiceCollection AddSalesMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
            ?? "amqp://developer:ev@luAt10n@localhost:5672";

        services.AddRebus((configure, _) => configure
            .Transport(t => t.UseRabbitMq(rabbitMqConnectionString, InputQueueName))
            .Routing(r => r.TypeBased()
                .MapAssemblyOf<SaleCreatedMessage>(InputQueueName)),
            isDefaultBus: true,
            onCreated: async bus =>
            {
                await bus.Subscribe<SaleCreatedMessage>();
                await bus.Subscribe<SaleModifiedMessage>();
                await bus.Subscribe<SaleCancelledMessage>();
                await bus.Subscribe<ItemCancelledMessage>();
            });

        services.AutoRegisterHandlersFromAssemblyOf<SaleCreatedMessageHandler>();
        services.AddScoped<ISaleEventPublisher, RebusSaleEventPublisher>();

        return services;
    }
}
