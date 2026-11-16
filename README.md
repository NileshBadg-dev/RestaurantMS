# Restaurant Management System

A Clean Architecture .NET 8 application for restaurants and hotels: tables, orders, kitchen,
billing, credit (udhari), accounts and reports, in English and Marathi.

## Quick start
1. Install .NET 8 SDK and PostgreSQL (or SQL Server).
2. Copy `src/RestaurantMS.Api/appsettings.Development.sample.json` to `appsettings.Development.json`
and set the connection string.
3. `dotnet restore`
4. `dotnet ef database update --project src/RestaurantMS.Infrastructure --startup-project src/RestaurantMS.Api`
5. `dotnet run --project src/RestaurantMS.Web`
6. Log in with the seeded Admin user and change the password.

## Documents
- docs/ARCHITECTURE.md - layers, dependency rules, request flow
- CODING_STANDARDS.md - naming, style, testing, git rules
- AGENTS.md - instructions for AI coding agents
- docs/ACCOUNTING.md - posting rules and report definitions
- docs/CHANGELOG.md - changes per phase
