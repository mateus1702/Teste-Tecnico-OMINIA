using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

public class ListSalesRequest
{
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "_order")]
    public string? OrderBy { get; set; }

    public Guid? CustomerId { get; set; }
    public Guid? BranchId { get; set; }
    public bool? IsCancelled { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }

    [FromQuery(Name = "_minSaleDate")]
    public DateTime? MinSaleDate { get; set; }

    [FromQuery(Name = "_maxSaleDate")]
    public DateTime? MaxSaleDate { get; set; }

    [FromQuery(Name = "_minTotalAmount")]
    public decimal? MinTotalAmount { get; set; }

    [FromQuery(Name = "_maxTotalAmount")]
    public decimal? MaxTotalAmount { get; set; }

    public long? SaleNumber { get; set; }
}

public class ListSalesRequestProfile : Profile
{
    public ListSalesRequestProfile()
    {
        CreateMap<ListSalesRequest, ListSalesQuery>();
    }
}
