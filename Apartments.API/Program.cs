using Serilog;
using Apartments.API.Extensions;
using Apartments.API.Middlewares;
using Apartments.Application.Extensions;
using Apartments.Infrastructure.Extensions;
using Apartments.Infrastructure.Seeders;
using Apartments.Infrastructure.Hubs;
using Apartments.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database");

// Add services to the container.
builder.AddPresentation();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Example: resolve Firebase credentials path from config or env var
var firebaseCredPath = builder.Configuration["Firebase:CredentialsPath"]
                      ?? Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

// TODO: pass firebaseCredPath into your Firebase initialization if applicable

var app = builder.Build();

// Seeding apartments to an empty DB
var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IAppSeeder>();
await seeder.Seed();


// Middlewares - Error Handling should be first to catch all exceptions
app.UseCors("AllowSpecificOrigin");
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapHealthChecks("/health");

app.MapHub<NotificationHub>("/notifications");

app.MapControllers();

app.Run();