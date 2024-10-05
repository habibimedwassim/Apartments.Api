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
                await AddRoles();
            }

            if (!await dbContext.Users.Where(x => x.Role == UserRoles.Admin).AnyAsync())
            {
                await RegisterTempUser();
            }
        }
    }

    private async Task AddRoles()
    {
        List<IdentityRole> roles =
        [
            new(UserRoles.Owner) { NormalizedName = UserRoles.Owner.ToUpper() },
            new(UserRoles.Admin) { NormalizedName = UserRoles.Admin.ToUpper() }
        ];

        await dbContext.Roles.AddRangeAsync(roles);
        await dbContext.SaveChangesAsync();
    }

    private async Task RegisterTempUser()
    {
        var user = new User()
        {
            Email = AppConstants.TempAdmin,
            UserName = AppConstants.TempAdmin,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "Temp*123");
        if (result.Succeeded)
        {
            user.Role = UserRoles.Admin;
            await userManager.AddToRoleAsync(user, UserRoles.Admin);
            await userManager.UpdateAsync(user);
        }
    }
}