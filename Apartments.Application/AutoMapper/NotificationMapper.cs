
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Domain.Entities;
using AutoMapper;

namespace Apartments.Application.AutoMapper;

public class NotificationMapper : Profile
{
    public NotificationMapper()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
