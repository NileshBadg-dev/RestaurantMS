# Architectural & Design Decisions

## ADR 001: Technology Stack and Solution Layout
- **Decision**: Adopt Clean Architecture with .NET 8 LTS, C# 12, ASP.NET Core MVC (Razor) for front-of-house and back-office operations, and ASP.NET Core Web API for integrations.
- **Context**: Restaurant operations require high stability on local LAN, responsive UI on PCs and tablets, and strict isolation of domain business logic.
- **Consequences**: Strict dependency hierarchy enforced via project references. Application and Domain contain no external infrastructure dependencies.

## ADR 002: Dual-Language Support (English and Marathi)
- **Decision**: Use standard ASP.NET Core `.resx` localization with `IViewLocalizer` and `IStringLocalizer`.
- **Context**: Operators and floor staff in target regions operate fluently in Marathi and English.
- **Consequences**: No hardcoded UI strings; resources maintained in pairs (`SharedResource.en.resx`, `SharedResource.mr.resx`). Locally bundled fonts (Noto Sans Devanagari) ensure offline rendering.

## ADR 003: EF Core 8 with Relational DB Flexibility
- **Decision**: Code-First EF Core 8 with Fluent Configurations. Default database is SQL Server with capability to switch to MySQL or PostgreSQL via configuration (`Database:Provider`).
- **Context**: Hotels may host on Windows SQL Server or open-source Linux servers.

## ADR 004: Identity & Custom AppUser Implementation
- **Decision**: Implement `AppUser` inheriting `BaseEntity` (long Id) without coupling Domain to `Microsoft.AspNetCore.Identity.EntityFrameworkCore`. Abstract password hashing through `IPasswordHasherService` (implemented via `PasswordHasher<AppUser>`).
- **Context**: Clean Architecture requires Domain entities to have zero external framework dependencies.
- **Consequences**: Pure POCO domain entity; ASP.NET Core Cookie Authentication is used in MVC layer with claims (`ClaimTypes.NameIdentifier`, `ClaimTypes.Name`, `ClaimTypes.Role`, and `Permissions`).

## ADR 005: Permission-Based Authorization & Dynamic Navigation
- **Decision**: Enforce permission codes formatted as `Screen.Action` (e.g. `Settings.View`, `Users.Create`, `Roles.Edit`). Implement `IRequirePermission` for MediatR commands, validated via `PermissionBehavior`. On Web, provide `[PermissionAuthorize]`, `PermissionPolicyProvider`, and `PermissionTagHelper` (`asp-permission`).
- **Context**: Staff roles have granular duties (e.g. Cashier can view orders/billing but cannot edit hotel settings or feature toggles).
- **Consequences**: Unauthorized URL access immediately returns Access Denied (HTTP 403 / `/Account/AccessDenied`). Dynamic `NavMenuViewComponent` displays only navigation items the user is permitted to view.

## ADR 006: In-Memory Caching for System Settings and Feature Toggles
- **Decision**: `SettingService` and `FeatureService` cache configuration in `IMemoryCache` with automatic invalidation upon save commands (`InvalidateCache()`).
- **Context**: Every page render queries hotel branding (name, logo) and permission checks query feature toggles.
- **Consequences**: Near-zero database queries for static configurations; changes take effect immediately across all application threads.

## ADR 007: Image Upload Security & Magic Byte Signature Verification
- **Decision**: Hotel logo uploads enforce whitelist extensions (`.jpg`, `.jpeg`, `.png`, `.webp`), verify file signature magic bytes (`FF D8 FF`, `89 50 4E 47`, `RIFF...WEBP`), and cap file sizes at 2 MB. File names are randomized using GUIDs and saved outside source code trees in `wwwroot/uploads/`.
- **Context**: Protect against malicious file uploads and MIME confusion attacks.
