namespace Apartments.Application.Dtos.AuthDtos;

public class LoginResponseDto
{
    public int Id { get; set; } = default!;
    public string? Avatar { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? TempEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? Role { get; set; }
    public string AccessToken { get; set; } = string.Empty;
}