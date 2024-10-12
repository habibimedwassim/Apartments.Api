namespace Apartments.Application.Dtos.AuthDtos;

public class UpdateEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
}
