using Microsoft.AspNetCore.Identity;

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
    public bool IsDeleted { get; set; }
}
