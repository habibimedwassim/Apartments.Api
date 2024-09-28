using Apartments.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Apartments.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Repositories;
using Apartments.Infrastructure.Seeders;

namespace Apartments.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ApartmentsDb");
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString).EnableSensitiveDataLogging());

        services.AddIdentityApiEndpoints<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IAppSeeder, AppSeeder>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IApartmentRepository, ApartmentRepository>();
        services.AddScoped<IApartmentPhotoRepository, ApartmentPhotoRepository>();
        services.AddScoped<IRentTransactionRepository, RentTransactionRepository>();
        services.AddScoped<IApartmentRequestRepository, ApartmentRequestRepository>();

    }
}
