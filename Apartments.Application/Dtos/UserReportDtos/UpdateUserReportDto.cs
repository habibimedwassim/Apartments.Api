using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Dtos.UserReportDtos;

public class UpdateUserReportDto
{
    public DateTime? ResolvedDate { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string? Comments { get; set; }
    public IFormFile? Attachment { get; set; }
}
