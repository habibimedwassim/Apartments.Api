using RenTN.API.Extensions;
using RenTN.API.Middlewares;
using RenTN.Application.Extensions;
using RenTN.Domain.Entities;
using RenTN.Infrastructure.Extensions;
using RenTN.Infrastructure.Seeders;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddPresentation();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seeding apartments to an empty DB
var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IApplicationSeeder>();
await seeder.Seed();

// Configure the HTTP request pipeline.
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.MapGroup("api/identity")
//        .WithTags("Identity")
//        .MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();
