using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Seeders;

public interface IAppSeeder
{
    Task Seed();
}

public class AppSeeder(ApplicationDbContext dbContext, UserManager<User> userManager) : IAppSeeder
{
    public async Task Seed()
    {
        if (await dbContext.Database.CanConnectAsync())
        {
            if (!await dbContext.Roles.AnyAsync())
            {
                var roles = GetRoles();
                await dbContext.Roles.AddRangeAsync(roles);
                await dbContext.SaveChangesAsync();
            }

            if (!await dbContext.Users.Where(x => x.Role == UserRoles.Admin).AnyAsync())
            {
                var user = GetTempAdmin();
                var result = await userManager.CreateAsync(user, "Temp*123");
                if (result.Succeeded)
                {
                    user.Role = UserRoles.Admin;
                    await userManager.AddToRoleAsync(user, UserRoles.Admin);
                    await userManager.UpdateAsync(user);
                }
            }

            if (!await dbContext.Apartments.AnyAsync())
            {
                var user = await userManager.Users.FirstOrDefaultAsync(x => x.Role == UserRoles.Admin);
                var apartments = GetApartments(user);
                await dbContext.Apartments.AddRangeAsync(apartments);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private User GetTempAdmin()
    {
        return new User()
        {
            Email = AppConstants.TempAdmin,
            UserName = AppConstants.TempAdmin,
            EmailConfirmed = true
        };
    }

    private IEnumerable<IdentityRole> GetRoles()
    {
        List<IdentityRole> roles =
        [
            new(UserRoles.Owner) { NormalizedName = UserRoles.Owner.ToUpper() },
            new(UserRoles.Admin) { NormalizedName = UserRoles.Admin.ToUpper() }
        ];

        return roles;
    }

    private IEnumerable<Apartment> GetApartments(User? user)
    {
        var owner = user ?? new User() { Email = AppConstants.TempAdmin };

        List<Apartment> apartments =
        [
            new Apartment(owner.Id)
            {
                CreatedDate = DateTime.UtcNow,
                City = "Tunis",
                Street = "Cité des Nymphes",
                PostalCode = "1003",
                Description = "Apartment on the 2nd floor with 3 beds (S+3)",
                Size = 3,
                RentAmount = 550M,
                AvailableFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                ApartmentPhotos =
                [
                    new ApartmentPhoto
                    {
                        CreatedDate = DateTime.UtcNow,
                        Url = "https://www.resident360.com/wp-content/uploads/2019/03/Anthology-1.jpg"
                    },

                    new ApartmentPhoto
                    {
                        CreatedDate = DateTime.UtcNow,
                        Url =
                            "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1e/AIMCO_apartment_interior.jpg/640px-AIMCO_apartment_interior.jpg"
                    }
                ]
            },

            new Apartment(owner.Id)
            {
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                City = "Zaghouan",
                Street = "Cité Ennozha",
                PostalCode = "1100",
                Description = "Apartment on the 1st floor with 2 beds (S+2)",
                Size = 2,
                RentAmount = 400M,
                IsOccupied = true,
                AvailableFrom = null,
                ApartmentPhotos = []
            }
        ];

        return apartments;
    }
}