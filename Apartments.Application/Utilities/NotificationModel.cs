namespace Apartments.Application.Utilities;

public class NotificationModel
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string NotificationType { get; set; } = default!;
    public string? Status { get; set; }
    public bool StoreDb { get; set; } = true;
    public bool SendEmail { get; set; } = true;
    public bool SendSignalR { get; set; } = true;
    public bool SendFirebase { get; set; } = true;

}
