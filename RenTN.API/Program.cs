using RenTN.API.Extensions;
using RenTN.API.Middlewares;
using RenTN.Application.Extensions;
using RenTN.Infrastructure.Extensions;
using RenTN.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seeding apartments to an empty DB
var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IApartmentSeeder>();
await seeder.Seed();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
