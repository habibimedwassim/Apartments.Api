namespace Apartments.Application.Dtos.AuthDtos;

public class VerifyEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}