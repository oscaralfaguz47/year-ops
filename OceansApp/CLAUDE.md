# Ripple by Oceans — Claude Context

## Project Overview
ASP.NET Core 8 MVC + Razor Pages app called **Ripple by Oceans**. Solution file: `OceansApp.sln`.

## Solution Structure
```
OceansApp/
├── OceansAppWeb/          # Main web app — controllers, Razor views, Vanilla JS, auth
├── OceansApp.DataAccess/  # EF Core — DbContext, Repositories, Unit of Work, DbInitializer
├── OceansApp.Models/      # Domain models, ViewModels, DTOs
├── OceansApp.Utility/     # Constants (SD.cs), auth policies, shared helpers, email templates
└── AzureFunctionsApp/     # Background email processing via Azure Queue Storage
```

## MVC Areas
`AccountManagement`, `AdminCenter`, `Finances`, `General`, `Recruiting`, `Resources`, `TrackingTool`

## Key Integrations
- **SQL Server** via EF Core (Repository + Unit of Work pattern)
- **Azure Blob Storage** — file uploads (`FilesStorageAccountENV`)
- **Azure Queue Storage** — email queue (`AzureWebJobsStorage`, queue name: `emailqueue`)
- **Azure Form Recognizer** — OCR document validation
- **Azure App Configuration + Key Vault** — secrets in Production only
- **OpenAI** — `OpenAIRepository`
- **Bonusly API** — `BonuslyRepository`
- **Slack** — `SlackRepository`
- **ASP.NET Core Identity** — roles: `Master`, `Admin`, `Computer Consultant`

## Local Development Setup (macOS)

### Prerequisites
- .NET 8 SDK
- Docker (SQL Server runs in a container)
- VS Code + C# Dev Kit extension

### SQL Server Docker Container
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=RippleLocal2025$" \
  -p 1433:1433 --name ripple-sql -d \
  mcr.microsoft.com/mssql/server:2022-latest
```
To restart after reboot: `docker start ripple-sql`

### Environment Variables
All set in `OceansAppWeb/Properties/launchSettings.json` under the `OceansAppWeb` profile.
**Do not set these in `~/.zshrc`** — launchSettings.json scopes them to this project only.

Local master user credentials:
- Email: `esteban.rojas@oceanscode.com`
- Password: `RippleLocal2025$`

SQL Server connection (Docker, SA auth — Mac doesn't support Windows auth):
```
Server=localhost,1433;Database=RippleLocal;User Id=sa;Password=RippleLocal2025$;TrustServerCertificate=True;Connection Timeout=30;
```

### Run the app
```bash
cd OceansApp
dotnet run --project OceansAppWeb/OceansAppWeb.csproj
```
URL: https://localhost:7115

On first run, `DbInitializer` auto-migrates the DB and seeds the master user.

### EF Core Migrations
```bash
cd OceansApp
dotnet ef migrations add <MigrationName> --project OceansApp.DataAccess --startup-project OceansAppWeb
dotnet ef database update --project OceansApp.DataAccess --startup-project OceansAppWeb
```

## Branch Strategy
- `main` — protected, admin-merge only via PR
- `demo` — staging branch, cut feature branches from here
- Always branch from `demo`, not `main`

## Demo DB (Azure — requires IP whitelist)
- Server: `rippleserverdemo.database.windows.net`
- User: `rippleDemoAdmin2025`

## Important Compatibility Notes
- **Timezones**: Code uses `"Central America Standard Time"` (Windows ID). Works on macOS in .NET 8+ via built-in cross-platform mapping — no code changes needed.
- **SQL auth**: Use `User Id=sa;Password=...` in local connection string. `Trusted_Connection=True` (Windows auth) does not work on macOS.
- **Azure App Configuration**: Only loads in Production (when `AppConfigConnectionString` env var is present). Safe to ignore locally.
