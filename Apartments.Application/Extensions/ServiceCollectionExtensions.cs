using Apartments.Application.Common;
using Apartments.Application.IServices;
using Apartments.Application.RequestHandlers;
using Apartments.Application.Services;
using Apartments.Domain.Common;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Apartments.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
        services.AddAutoMapper(applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly).AddFluentValidationAutoValidation();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IApartmentService, ApartmentService>();
        services.AddScoped<IApartmentPhotoService, ApartmentPhotoService>();
        services.AddScoped<IRentTransactionService, RentTransactionService>();
        services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IApartmentRequestService, ApartmentRequestService>();

        services.AddHttpContextAccessor();

        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorizationManager, AuthorizationManager>();

        // Handlers
        services.AddScoped<IRentRequestHandler, RentRequestHandler>();
        services.AddScoped<ILeaveRequestHandler, LeaveRequestHandler>();
        services.AddScoped<IDismissRequestHandler, DismissRequestHandler>();

        services.RegisterConfigurations(configuration);
    }
    private static void RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.Configure<AzureBlobStorageSettings>(configuration.GetSection("AzureStorage"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
            options.DefaultScheme =
            options.DefaultSignInScheme =
            options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection("Jwt").Bind(jwtSettings);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)
                ),
            };
        });
    }
}
