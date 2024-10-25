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

        CreateMap<ChangeLog, ChangeLog>();
        CreateMap<AdminDashboardDetails, AdminDashboardDto>()
            .ForMember(dest => dest.ReportsByMonth, opt => opt.MapFrom(src => src.ReportsByMonth))
            .ForMember(dest => dest.RecentReports, opt => opt.MapFrom(src => src.RecentReports))
            .ForMember(dest => dest.RecentChangeLogs, opt => opt.MapFrom(src => src.RecentChangeLogs));

        CreateMap<ReportsByMonth, ReportsByMonthDto>();
        CreateMap<UserReport, RecentReportDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));
    }
}
