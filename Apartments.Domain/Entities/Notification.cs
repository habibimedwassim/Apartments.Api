namespace Apartments.Domain.Entities;

public class Notification
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string UserId { get; init; } = default!;
    public User User { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string Type { get; init; } = default!;
    public bool IsRead { get; set; }
}
