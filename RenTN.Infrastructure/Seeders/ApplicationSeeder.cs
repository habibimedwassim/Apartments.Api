using Microsoft.AspNetCore.Identity;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Infrastructure.Data;

namespace RenTN.Infrastructure.Seeders;

internal class ApplicationSeeder(ApplicationDbContext _dbContext) : IApplicationSeeder
{
    public async Task Seed()
    {
        if (await _dbContext.Database.CanConnectAsync())
        {
            if (!_dbContext.Apartments.Any())
            {
                var apartments = GetApartments();
                await _dbContext.Apartments.AddRangeAsync(apartments);
                await _dbContext.SaveChangesAsync();
            }

            if (!_dbContext.Roles.Any())
            {
                var roles = GetRoles();
                await _dbContext.Roles.AddRangeAsync(roles);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
    private IEnumerable<IdentityRole> GetRoles()
    {
        List<IdentityRole> roles =
            [
                new (UserRoles.User){ NormalizedName = UserRoles.User.ToUpper()},
                new (UserRoles.Owner){ NormalizedName = UserRoles.Owner.ToUpper()},
                new (UserRoles.Admin){ NormalizedName = UserRoles.Admin.ToUpper()}
            ];

        return roles;
    }
    private IEnumerable<Apartment> GetApartments()
    {
        User owner = new User()
        {
            Email = "test@test.com"
        };

        List<Apartment> apartments = new()
        {
            new()
            {
                Owner = owner,
                City = "Zaghouan",
                Street = "Cité des Nymphes",
                PostalCode = "1100",
                Description = "Apartment on the 2nd floor with 3 beds (S+3)",
                Size = 3,
                Price = 550M,
                IsAvailable = true,
                ApartmentPhotos = new List<ApartmentPhoto>
                {
                    new ApartmentPhoto
                    {
                        Url = "https://www.resident360.com/wp-content/uploads/2019/03/Anthology-1.jpg"
                    },
                    new ApartmentPhoto
                    {
                        Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1e/AIMCO_apartment_interior.jpg/640px-AIMCO_apartment_interior.jpg"
                    }
                }
            },
            new()
            {
                Owner = owner,
                City = "Zaghouan",
                Street = "Cité Ennozha",
                PostalCode = "1100",
                Description = "Apartment on the 1st floor with 2 beds (S+2)",
                Size = 2,
                Price = 400M,
                IsAvailable = true,
                ApartmentPhotos = new List<ApartmentPhoto>() // Empty, no photos for this one
            }
        };

        return apartments;
    }
}
