namespace RenTN.Application.DTOs.IdentityDTO;

public class ResetPasswordDTO
{
    public string Email { get; set; } = default!;
    public string VerificationCode { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
