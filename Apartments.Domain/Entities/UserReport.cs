namespace Apartments.Domain.Entities;

public class UserReport
{
    public int Id { get; init; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedDate { get; set; }
    public string ReporterId { get; init; } = default!;
    public User Reporter { get; set; } = default!;
    public string? TargetId { get; set; }
    public User? Target { get; set; }
    public string TargetRole { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Status { get; set; } = ReportStatus.Pending.ToString();
    public string? AttachmentUrl { get; set; }
    public string? Comments { get; set; }
}
public enum ReportStatus
{
    Pending,
    InProgress,
    Resolved,
    Closed
}