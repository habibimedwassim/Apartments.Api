using Apartments.Domain.Common;
using Apartments.Domain.Entities;

namespace Apartments.Application.Dtos.EmailDtos;

public class VerificationRequest
{
    public User User { get; set; } = default!;
    public VerificationCodeType CodeType { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}