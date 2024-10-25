namespace Apartments.Application.Dtos.UserReportDtos;

public class UserReportDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public int ReporterId { get; set; }
    public string? ReporterAvatar {  get; set; }
    public string? ReporterInitials { get; set; }
    public string? ReporterRole { get; set; }
    public int? TargetId { get; set; }
    public string TargetRole { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? AttachmentUrl { get; set; }
    public string? Comments { get; set; }
}
