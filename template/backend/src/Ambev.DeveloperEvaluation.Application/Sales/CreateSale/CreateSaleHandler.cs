using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Messages;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommand : IRequest<SaleResult>
{
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemInput> Items { get; set; } = [];
}

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(20);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        ISaleEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var items = request.Items
            .Select(i => SaleItem.Create(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        var sale = Sale.Create(
            request.SaleDate,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName,
            items);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishSaleCreatedAsync(new SaleCreatedMessage
        {
            SaleId = created.Id,
            SaleNumber = created.SaleNumber,
            SaleDate = created.SaleDate,
            CustomerId = created.CustomerId,
            CustomerName = created.CustomerName,
            BranchId = created.BranchId,
            BranchName = created.BranchName,
            TotalAmount = created.TotalAmount,
            OccurredAt = DateTime.UtcNow
        }, cancellationToken);

        return _mapper.Map<SaleResult>(created);
    }
}
