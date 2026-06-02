using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<SaleItem, SaleItemResult>();
        CreateMap<Sale, SaleResult>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
    }
}
