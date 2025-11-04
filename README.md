# Apartments API

A .NET 8-based REST API for managing apartments, including user authentication, property listings, and integrations with Azure Blob Storage and Firebase.

## Features

- User management and JWT-based authentication
- Apartment/property CRUD operations
- Image storage via Azure Blob Storage
- Push notifications via Firebase
- Database seeding for development
- Middleware for error handling and logging

## Prerequisites

- .NET 8 SDK
- SQL Server (or compatible database)
- (Optional) Azure Storage account for blob storage
- (Optional) Firebase service account for notifications

## Installation

1. Clone the repository.
2. Copy example configuration:
   - On Linux/macOS: `cp Apartments.API/appsettings.example.json Apartments.API/appsettings.Development.json`
   - On Windows: `Copy-Item Apartments.API\appsettings.example.json Apartments.API\appsettings.Development.json`
3. Edit [Apartments.API/appsettings.Development.json](Apartments.API/appsettings.Development.json) to configure:
   - Database connection string (`ApartmentsDb`)
   - Azure Blob settings (connection string and container)
   - JWT settings (Key, Issuer, Audience)
   - Firebase credentials path (or set `GOOGLE_APPLICATION_CREDENTIALS` env var)
4. Ensure database is set up; the app will run seeder on startup (see [`IAppSeeder`](Apartments.Infrastructure/Extensions/ServiceCollectionExtensions.cs)).

## Usage

- Build: `dotnet build Apartments.sln`
- Run: `dotnet run --project Apartments.API/Apartments.API.csproj`
- API endpoints are defined in [Apartments.API/Controllers/](Apartments.API/Controllers/)

## Security Notes

- Sensitive data is removed from the repo. Use example files and environment variables for real credentials.
- Do not commit private keys, API keys, or production secrets.
- Firebase credentials template: [Apartments.API/Templates/firebase-adminsdk.example.json](Apartments.API/Templates/firebase-adminsdk.example.json)

## License

MIT License (see [LICENSE](LICENSE)).
