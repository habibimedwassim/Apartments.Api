using Microsoft.AspNetCore.Identity;

namespace RenTN.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<Apartment> OwnedApartments { get; set; } = [];
}
