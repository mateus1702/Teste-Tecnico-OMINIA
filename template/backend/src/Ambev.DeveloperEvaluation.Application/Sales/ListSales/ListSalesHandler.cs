using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesQuery : IRequest<ListSalesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? OrderBy { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? BranchId { get; set; }
    public bool? IsCancelled { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public DateTime? MinSaleDate { get; set; }
    public DateTime? MaxSaleDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
    public long? SaleNumber { get; set; }
}

public class ListSalesResult
{
    public IReadOnlyList<SaleResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class ListSalesQueryValidator : AbstractValidator<ListSalesQuery>
{
    public ListSalesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).InclusiveBetween(1, 100);
    }
}

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var criteria = new SaleListCriteria
        {
            Page = request.Page,
            Size = request.Size,
            OrderBy = request.OrderBy,
            CustomerId = request.CustomerId,
            BranchId = request.BranchId,
            IsCancelled = request.IsCancelled,
            CustomerName = request.CustomerName,
            BranchName = request.BranchName,
            MinSaleDate = request.MinSaleDate,
            MaxSaleDate = request.MaxSaleDate,
            MinTotalAmount = request.MinTotalAmount,
            MaxTotalAmount = request.MaxTotalAmount,
            SaleNumber = request.SaleNumber
        };

        var (items, totalCount) = await _saleRepository.ListAsync(criteria, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.Size);

        return new ListSalesResult
        {
            Items = _mapper.Map<IReadOnlyList<SaleResult>>(items),
            TotalCount = totalCount,
            CurrentPage = request.Page,
            TotalPages = totalPages
        };
    }
}
