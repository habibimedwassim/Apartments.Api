namespace Apartments.Application.Dtos.NotificationDtos;

public class NotifyUserRequest
{
    public string UserId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
}
