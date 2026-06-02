using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SalesHandlersTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly ISaleEventPublisher _eventPublisher = Substitute.For<ISaleEventPublisher>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    public SalesHandlersTests()
    {
        _mapper.Map<SaleResult>(Arg.Any<Sale>())
            .Returns(callInfo =>
            {
                var sale = callInfo.Arg<Sale>();
                return new SaleResult
                {
                    Id = sale.Id,
                    SaleNumber = sale.SaleNumber,
                    TotalAmount = sale.TotalAmount,
                    IsCancelled = sale.IsCancelled
                };
            });

        _mapper.Map<IReadOnlyList<SaleResult>>(Arg.Any<IReadOnlyList<Sale>>())
            .Returns(callInfo =>
            {
                var sales = callInfo.Arg<IReadOnlyList<Sale>>();
                return sales
                    .Select(s => new SaleResult
                    {
                        Id = s.Id,
                        SaleNumber = s.SaleNumber,
                        TotalAmount = s.TotalAmount,
                        IsCancelled = s.IsCancelled
                    })
                    .ToList();
            });
    }

    [Fact]
    public async Task GetSaleHandler_WhenSaleExists_ReturnsMappedResult()
    {
        var sale = CreateSale();
        sale.SetSaleNumber(10);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var handler = new GetSaleHandler(_saleRepository, _mapper);
        var result = await handler.Handle(new GetSaleQuery { Id = sale.Id }, CancellationToken.None);

        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(10);
    }

    [Fact]
    public async Task GetSaleHandler_WhenSaleMissing_ThrowsKeyNotFoundException()
    {
        var handler = new GetSaleHandler(_saleRepository, _mapper);
        var act = () => handler.Handle(new GetSaleQuery { Id = Guid.NewGuid() }, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ListSalesHandler_ReturnsPagedResult()
    {
        var sale = CreateSale();
        _saleRepository.ListAsync(Arg.Any<SaleListCriteria>(), Arg.Any<CancellationToken>())
            .Returns((new List<Sale> { sale } as IReadOnlyList<Sale>, 1));

        var handler = new ListSalesHandler(_saleRepository, _mapper);
        var result = await handler.Handle(new ListSalesQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task UpdateSaleHandler_WhenSaleExists_UpdatesAndPublishesEvent()
    {
        var sale = CreateSale();
        sale.SetSaleNumber(2);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        var handler = new UpdateSaleHandler(_saleRepository, _eventPublisher, _mapper);
        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = DateTime.UtcNow,
            CustomerId = sale.CustomerId,
            CustomerName = "Updated Customer",
            BranchId = sale.BranchId,
            BranchName = sale.BranchName,
            Items =
            [
                new SaleItemInput
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Monitor",
                    Quantity = 4,
                    UnitPrice = 250m
                }
            ]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _eventPublisher.Received(1).PublishSaleModifiedAsync(Arg.Any<SaleModifiedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelSaleHandler_WhenSaleExists_CancelsAndPublishesEvent()
    {
        var sale = CreateSale();
        sale.SetSaleNumber(3);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        var handler = new CancelSaleHandler(_saleRepository, _eventPublisher, _mapper);
        var result = await handler.Handle(new CancelSaleCommand { Id = sale.Id }, CancellationToken.None);

        result.IsCancelled.Should().BeTrue();
        await _eventPublisher.Received(1).PublishSaleCancelledAsync(Arg.Any<SaleCancelledMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelSaleItemHandler_WhenItemExists_CancelsItemAndPublishesEvent()
    {
        var sale = CreateSale();
        sale.SetSaleNumber(4);
        var itemId = sale.Items.First().Id;
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        var handler = new CancelSaleItemHandler(_saleRepository, _eventPublisher, _mapper);
        var result = await handler.Handle(new CancelSaleItemCommand { SaleId = sale.Id, ItemId = itemId }, CancellationToken.None);

        result.Should().NotBeNull();
        await _eventPublisher.Received(1).PublishItemCancelledAsync(Arg.Any<ItemCancelledMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteSaleHandler_WhenSaleExists_ReturnsTrue()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.DeleteAsync(saleId, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new DeleteSaleHandler(_saleRepository);
        var result = await handler.Handle(new DeleteSaleCommand { Id = saleId }, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSaleHandler_WhenSaleMissing_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.DeleteAsync(saleId, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new DeleteSaleHandler(_saleRepository);
        var act = () => handler.Handle(new DeleteSaleCommand { Id = saleId }, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static Sale CreateSale()
    {
        var item = SaleItem.Create(Guid.NewGuid(), "Keyboard", 5, 100m);
        return Sale.Create(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [item]);
    }
}
