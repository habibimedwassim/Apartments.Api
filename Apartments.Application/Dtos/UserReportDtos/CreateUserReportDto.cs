using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Dtos.UserReportDtos;

public class CreateUserReportDto
{
    public int? TargetId { get; set; }
    public string TargetRole { get; set; } = default!;
    public string Message { get; set; } = default!;
    public IFormFile? Attachment { get; set; }
}

