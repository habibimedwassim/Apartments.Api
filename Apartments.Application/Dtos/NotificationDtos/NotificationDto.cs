namespace Apartments.Application.Dtos.NotificationDtos;

public class NotificationDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = default!;
    public string Type { get; set; } = default!;
    public bool IsRead { get; set; }
}