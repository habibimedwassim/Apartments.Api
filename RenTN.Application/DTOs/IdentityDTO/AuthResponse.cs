namespace RenTN.Application.DTOs.IdentityDTO;

public class AuthResponse
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
    public string AccessToken { get; set; } = string.Empty;
}