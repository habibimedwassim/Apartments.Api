namespace Apartments.Domain.Entities;

public class UserDeviceToken
{
    public int Id { get; init; }
    public User User { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string DeviceToken { get; set; } = default!;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
