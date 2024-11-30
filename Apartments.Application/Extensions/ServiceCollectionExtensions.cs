using Apartments.Application.BackgroundServices;
using Apartments.Application.Common;
using Apartments.Application.IServices;
using Apartments.Application.Services;
using Apartments.Application.Services.ApartmentRequestHandlers;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using FirebaseAdmin;
using FluentValidation;
using FluentValidation.AspNetCore;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
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
        services.AddScoped<IUserReportService, UserReportService>();
        services.AddScoped<IApartmentPhotoService, ApartmentPhotoService>();
        services.AddScoped<IRentTransactionService, RentTransactionService>();
        services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IApartmentRequestService, ApartmentRequestService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFcmService, FcmService>();
        services.AddScoped<INotificationUtilities, NotificationUtilities>();

        services.AddHttpContextAccessor();

        // Auth services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorizationManager, AuthorizationManager>();

        // Handlers
        services.AddScoped<IRentRequestHandler, RentRequestHandler>();
        services.AddScoped<ILeaveRequestHandler, LeaveRequestHandler>();
        services.AddScoped<IDismissRequestHandler, DismissRequestHandler>();
        services.AddSingleton<IUserIdProvider, ClaimsPrincipalUserIdProvider>();

        // Background Services
        services.AddHostedService<RentTransactionScheduler>();

        services.RegisterConfigurations(configuration);

        services.AddSignalR();
    }

    private static void RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
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
                )
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Allow token in query string for SignalR WebSockets
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for the SignalR hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                    {
                        // Attach the token to the request
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(Path.GetFullPath("Templates\\firebase-adminsdk.json")),
        });
    }
}