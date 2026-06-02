using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration;

public class ValidationPipelineIntegrationTests
{
    [Fact]
    public void ApplicationValidators_AreRegisteredInDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<ApplicationLayer>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationLayer).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var provider = services.BuildServiceProvider();
        var validator = provider.GetService<IValidator<CreateSaleCommand>>();

        Assert.NotNull(validator);
    }

    [Fact]
    public async Task ValidationBehavior_RejectsInvalidCreateSaleCommand()
    {
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<ApplicationLayer>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationLayer).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<IRequestHandler<CreateSaleCommand, SaleResult>>(new NoOpCreateSaleHandler());

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.Empty,
            CustomerName = string.Empty,
            BranchId = Guid.Empty,
            BranchName = string.Empty,
            Items = []
        };

        await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(command));
    }

    private sealed class NoOpCreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
    {
        public Task<SaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken) =>
            Task.FromResult(new SaleResult());
    }
}
