using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemCommand : IRequest<SaleResult>
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
}

public class CancelSaleItemCommandValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        ISaleEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        sale.CancelItem(request.ItemId);
        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishItemCancelledAsync(new ItemCancelledMessage
        {
            SaleId = updated.Id,
            ItemId = request.ItemId,
            SaleNumber = updated.SaleNumber,
            OccurredAt = DateTime.UtcNow
        }, cancellationToken);

        return _mapper.Map<SaleResult>(updated);
    }
}
