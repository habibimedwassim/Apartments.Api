using Apartments.Application.Dtos.ApartmentRequestDtos;
using Apartments.Application.Dtos.DashboardDtos;
using Apartments.Application.Dtos.RentTransactionDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class OwnerDashboardMappingProfile : Profile
{
    public OwnerDashboardMappingProfile()
    {
        CreateMap<OwnerDashboardDetails, OwnerDashboardDto>()
            .ForMember(dest => dest.RecentTransactions, opt => opt.MapFrom(src => src.RecentTransactions))
            .ForMember(dest => dest.RecentRequests, opt => opt.MapFrom(src => src.RecentRequests))
            .ForMember(dest => dest.RevenueByMonth, opt => opt.MapFrom(src => src.RevenueByMonth));

        CreateMap<(string Month, decimal Revenue), RevenueByMonthDto>()
            .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Month))
            .ForMember(dest => dest.Revenue, opt => opt.MapFrom(src => src.Revenue));
    }
}
