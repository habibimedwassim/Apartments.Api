using Microsoft.AspNetCore.Identity;

namespace Apartments.Domain.Entities;

public class User : IdentityUser
{
    public int SysId { get; init; }
    public string CIN { get; init; } = string.Empty;
    public string? Avatar { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmailCode { get; set; }
    public DateTime? EmailCodeExpiration { get; set; }
    public string? TempEmail { get; set; }
    public bool TempEmailConfirmed { get; set; }
    public string? ResetCode { get; set; }
    public DateTime? ResetCodeExpiration { get; set; }
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