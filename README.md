# Apartments.Api

.NET backend for the renting app. API controllers, SignalR notifications, EF Core migrations, and seeders.

Requirements
- .NET SDK 7+ (or version used by the solution)
- A database (configured via connection string in appsettings.*.json or environment variables)

Quick start (local)
1. Restore and build
   dotnet restore
   dotnet build

2. Configure database connection
   Update Apartments.API/appsettings.Development.json or set environment variable for the connection string.

3. Apply EF migrations (if using CLI)
   dotnet ef database update --project Apartments.Application --startup-project Apartments.API

4. Run the API
   dotnet run --project Apartments.API

## Configuration

Copy appsettings.example.json to appsettings.json and adjust non-secret defaults.
Put real secrets in environment variables or dotnet user-secrets.

Using user-secrets (development):
cd Apartments.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=..."
dotnet user-secrets set "Jwt:Secret" "CHANGE_ME"
dotnet user-secrets set "AzureBlob:ConnectionString" "..."

Firebase:
- Save your service account JSON locally (not committed).
- Set either appsettings Firebase:CredentialsPath or env var GOOGLE_APPLICATION_CREDENTIALS to its path.

## Run
dotnet restore
dotnet run --project Apartments.API

Notes
- Program entry: Apartments.API/Program.cs
- CORS and app services configured in Extensions/WebApplicationBuilderExtensions.cs
- SignalR hub path: /notifications
- Seeder runs on startup when configured in Program.cs — ensure DB is reachable before running.

Development tips
- Use Visual Studio or VS Code with C# extension for debugging.
- To run migrations, use the .NET EF tools and point to the correct projects.

License & Contributing
- See repository LICENSE if present.
