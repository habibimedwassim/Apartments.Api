using Apartments.Application.Dtos.UserReportDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class UserReportMapper : Profile
{
    public UserReportMapper()
    {
        CreateMap<UserReport, UserReport>();

        // Mapping from UserReport to UserReportDto
        CreateMap<UserReport, UserReportDto>()
            .ForMember(dest => dest.ReporterId, opt => opt.MapFrom(src => src.Reporter.SysId))
            .ForMember(dest => dest.TargetId, opt => opt.MapFrom(src => src.Target != null ? src.Target.SysId : (int?)null));

        // Mapping from CreateUserReportDto to UserReport
        CreateMap<CreateUserReportDto, UserReport>()
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ReportStatus.Pending.ToString()));

        CreateMap<UpdateUserReportDto, UserReport>()
            .ForMember(dest => dest.ResolvedDate, opt => opt.Condition(src => src.ResolvedDate.HasValue))
            .ForMember(dest => dest.Status, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Status)))
            .ForMember(dest => dest.Message, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Message)))
            .ForMember(dest => dest.Comments, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Comments)))
            .ForMember(dest => dest.AttachmentUrl, opt => opt.Ignore());

    }
}

