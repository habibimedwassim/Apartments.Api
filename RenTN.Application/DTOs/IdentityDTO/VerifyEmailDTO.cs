namespace RenTN.Application.DTOs.IdentityDTO;

public class VerifyEmailDTO
{
    public string Email { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}
