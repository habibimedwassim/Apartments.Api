using Microsoft.AspNetCore.Identity;

namespace RenTN.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiration { get; set; }
    public List<Apartment> OwnedApartments { get; set; } = [];
}
