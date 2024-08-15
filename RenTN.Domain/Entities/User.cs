using Microsoft.AspNetCore.Identity;

namespace RenTN.Domain.Entities;

public class User : IdentityUser
{
    public List<Apartment> OwnedApartments { get; set; } = [];
}
