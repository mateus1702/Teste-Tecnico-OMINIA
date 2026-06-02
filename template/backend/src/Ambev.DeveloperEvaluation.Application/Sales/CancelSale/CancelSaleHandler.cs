using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleCommand : IRequest<SaleResult>
{
    public Guid Id { get; set; }
}

public class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CancelSaleHandler(
        ISaleRepository saleRepository,
        ISaleEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.Cancel();
        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishSaleCancelledAsync(new SaleCancelledMessage
        {
            SaleId = updated.Id,
            SaleNumber = updated.SaleNumber,
            OccurredAt = DateTime.UtcNow
        }, cancellationToken);

        return _mapper.Map<SaleResult>(updated);
    }
}
