using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using RenTN.Application.Services.ApartmentsService;

namespace RenTN.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
        services.AddAutoMapper(applicationAssembly);
        services.AddScoped<IApartmentsService, ApartmentsService>();
        services.AddValidatorsFromAssembly(applicationAssembly).AddFluentValidationAutoValidation();
    }
}

