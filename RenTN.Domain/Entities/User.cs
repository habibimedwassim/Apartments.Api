using Microsoft.AspNetCore.Identity;
using RenTN.Domain.Common;

namespace RenTN.Domain.Entities;

public class User : IdentityUser
{
    public int SysID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiration { get; set; }
    public int? CurrentApartmentID { get; set; }
    public Apartment? CurrentApartment { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Role { get; set; }
    public bool IsDeleted { get; set; }
}
