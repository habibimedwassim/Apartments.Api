namespace RenTN.Application.DTOs.AuthDTOs;

public class AuthResponse
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Role { get; set; }
    public string AccessToken { get; set; } = string.Empty;
}