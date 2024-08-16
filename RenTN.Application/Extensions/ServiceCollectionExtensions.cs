using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using RenTN.Application.Services.ApartmentsService;
using RenTN.Application.Services.IdentityService;
using RenTN.Application.Users;

namespace RenTN.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
        services.AddAutoMapper(applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly).AddFluentValidationAutoValidation();

        services.AddScoped<IApartmentsService, ApartmentsService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserContext, UserContext>();

        services.AddHttpContextAccessor();
    }
}

