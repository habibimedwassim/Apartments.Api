namespace RenTN.Application.DTOs.AuthDTOs;

public class VerifyEmailDTO
{
    public string Email { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}
