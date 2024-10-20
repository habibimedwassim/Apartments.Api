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
            .ForMember(dest => dest.RecentRentRequests, opt => opt.MapFrom(src => src.RecentRentRequests))
            .ForMember(dest => dest.RecentLeaveRequests, opt => opt.MapFrom(src => src.RecentLeaveRequests))
            .ForMember(dest => dest.RecentDismissRequests, opt => opt.MapFrom(src => src.RecentDismissRequests))
            .ForMember(dest => dest.RevenueByMonth, opt => opt.MapFrom(src => src.RevenueByMonth));

        CreateMap<(string Month, decimal Revenue), RevenueByMonthDto>()
            .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Month))
            .ForMember(dest => dest.Revenue, opt => opt.MapFrom(src => src.Revenue));
    }
}
