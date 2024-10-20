namespace Apartments.Domain.Entities;

public class UserReport
{
    public int Id { get; init; }

    // The ID of the user who submitted the report
    public string ReporterId { get; init; } = default!;
    public User Reporter { get; set; } = default!;

    // The target of the report (could be an owner or admin)
    public string? TargetId { get; set; }
    public User? Target { get; set; }

    // The role of the target (owner or admin)
    public string TargetRole { get; set; } = default!;

    // The content of the report
    public string Message { get; set; } = default!;

    // Optional URL to an attachment related to the report
    public string? AttachmentUrl { get; set; }

    // Timestamps to track when the report was created and optionally resolved
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
