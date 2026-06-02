using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<ISaleEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _eventPublisher, _mapper);
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns mapped result and publishes event")]
    public async Task Handle_ValidRequest_ReturnsResultAndPublishesEvent()
    {
        var command = new CreateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            Items =
            [
                new SaleItemInput
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Keyboard",
                    Quantity = 5,
                    UnitPrice = 100m
                }
            ]
        };

        Sale? persistedSale = null;

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                persistedSale = callInfo.Arg<Sale>();
                persistedSale.SetSaleNumber(1);
                return persistedSale;
            });

        _mapper.Map<SaleResult>(Arg.Any<Sale>())
            .Returns(callInfo =>
            {
                var sale = callInfo.Arg<Sale>();
                return new SaleResult
                {
                    Id = sale.Id,
                    SaleNumber = sale.SaleNumber,
                    TotalAmount = sale.TotalAmount
                };
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(1);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishSaleCreatedAsync(Arg.Any<SaleCreatedMessage>(), Arg.Any<CancellationToken>());
    }
}
