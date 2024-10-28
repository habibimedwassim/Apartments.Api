using Serilog;
using Apartments.API.Extensions;
using Apartments.API.Middlewares;
using Apartments.Application.Extensions;
using Apartments.Infrastructure.Extensions;
using Apartments.Infrastructure.Seeders;
using Apartments.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// Add services to the container.
builder.AddPresentation();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seeding apartments to an empty DB
var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IAppSeeder>();
await seeder.Seed();

app.Urls.Add("http://0.0.0.0:5286");

// Middlewares - Error Handling should be first to catch all exceptions
app.UseCors("AllowAll");
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

app.MapHub<NotificationHub>("/notifications");

app.MapControllers();

app.Run();