using Microsoft.AspNetCore.Identity;

namespace Apartments.Domain.Entities;

public class User : IdentityUser
{
    public int SysId { get; init; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiration { get; set; }
    public string? Gender { get; set; } = UserGender.Male;
    public DateOnly? DateOfBirth { get; set; }
    public string? Role { get; set; }
    public bool IsDeleted { get; set; }
}

public static class UserGender
{
    public const string Male = nameof(Male);
    public const string Female = nameof(Female);
}