# Changelog

All notable changes to the Restaurant Management System project will be documented in this file.

## [Phase 1] - Authentication, Roles, Permissions, Settings & Feature Toggles - 2026-11-16
### Added
- **Domain Layer**:
  - Entities inheriting `BaseEntity`: `AppUser`, `Role`, `Permission`, `RolePermission`, `HotelSetting`, `FeatureToggle`, `AuditLog`.
  - Enums: `SettingDataType`, `AuditAction`.
  - Constants: `Permissions` (granular Screen.Action codes covering Dashboard, Orders, Tables, Kitchen, Billing, Customers, Credit, Accounts, Reports, Settings, Users, Roles, FeatureToggles), `FeatureKeys`.
- **Application Layer**:
  - Interfaces: `IUserRepository`, `IRoleRepository`, `ISettingRepository`, `IFeatureToggleRepository`, `IAuditLogRepository`, `IPermissionService`, `IFeatureService`, `ISettingService`, `IPasswordHasherService`, `IRequirePermission`, `IAuditableCommand`.
  - Use cases (CQRS via MediatR with Handlers, Validators, DTOs):
    - Authentication: `Login` (with 5-attempt lockout for 10 minutes), `Logout`, `ChangePassword`, `ForgotPassword`.
    - Users: `CreateUser`, `UpdateUser`, `ActivateDeactivateUser`, `ResetUserPassword`, `GetUsers` (server-side paged).
    - Roles: `CreateRole`, `UpdateRole`, `GetRoles`, `GetRolePermissionMatrix`, `SaveRolePermissionMatrix`.
    - Settings: `GetSettings`, `SaveSettings` (hotel name en/mr, address, contact, email, GSTIN with checksum validation, FSSAI, currency, timezone, receipt header/footer).
    - Feature Toggles: `GetFeatureToggles`, `SaveFeatureToggles`.
  - Pipeline behaviors: `ValidationBehavior`, `LoggingBehavior`, `PermissionBehavior`, `AuditBehavior`.
- **Infrastructure Layer**:
  - Entity Framework Core configurations for all Phase 1 entities (`decimal` precision, unique indexes, soft delete query filters, `RowVersion`).
  - Provider flexibility (`Database:Provider` supporting SQL Server and MySQL).
  - Repositories: `UserRepository`, `RoleRepository`, `SettingRepository`, `FeatureToggleRepository`, `AuditLogRepository`.
  - Password hashing abstraction via `PasswordHasherService`.
  - `LocalFileStorage` with file magic byte signature validation (JPG, PNG, WEBP) and 2MB limit.
  - Caching services: `SettingService` and `FeatureService` backed by `IMemoryCache` with cache invalidation on save.
  - Initial EF Core migration: `20261116132811_Phase1_Identity_Settings`.
  - Idempotent `DbInitializer` seeding standard roles (`Owner`, `Manager`, `Cashier`, `Waiter`, `Kitchen`, `Accountant`), all permission codes and role mappings, default hotel settings, all feature toggles ON, and default Admin (`admin` / `Admin@123` with forced password change).
- **Web Layer (ASP.NET Core MVC)**:
  - Controllers: `AccountController`, `UsersController`, `RolesController`, `SettingsController`, `FeatureTogglesController`, `LanguageController`, `HomeController`.
  - Dynamic permission-based navigation view component: `NavMenuViewComponent` and `Components/NavMenu/Default.cshtml`.
  - TagHelpers: `PermissionTagHelper` (`asp-permission`), `MoneyInputTagHelper`.
  - Authorization: `PermissionAuthorizeAttribute`, `PermissionRequirement`, `PermissionAuthorizationHandler`, `PermissionPolicyProvider`.
  - Dual-language support: `SharedResource.en.resx`, `SharedResource.mr.resx`, cookie-based culture switcher, locally served Noto Sans Devanagari font.
  - Views:
    - Login page with branding from settings, show/hide password, language toggle, error alerts.
    - Password management: Change Password, Forgot Password (admin reset notice).
    - User Management: Index (paged table, active badge, reset password modal), Create, Edit.
    - Role Management: Index, Create, Edit, Matrix (grid with screens as rows and View/Create/Edit/Delete/Print/Export as columns).
    - Hotel Settings: Tabbed layout (Hotel profile, Billing, Printing, Language) with logo upload preview.
    - Feature Toggles: Grouped toggle switches with short descriptions.
    - Access Denied view.
  - Shared components: `_PageHeader.cshtml`, `_ConfirmModal.cshtml`, `_Layout.cshtml` with live branding and language toggle.
- **Tests**:
  - Unit tests covering all FluentValidation validators, `LoginCommandHandler` (success, invalid password, 5-attempt lockout, inactive user), `PermissionBehavior` (authorized, forbidden, non-secured), `FeatureService` (on, off, cache invalidation), `LocalFileStorage` (magic byte signatures, unsupported types, >2MB limit).
  - Integration tests verifying idempotent seeding (running twice produces identical counts) and Admin login flow.

## [Phase 0] - Solution Setup & Foundations
### Added
- Clean Architecture solution structure (`RestaurantMS.sln`) with .NET 8 LTS target.
- Core projects created: `Domain`, `Application`, `Infrastructure`, `Api`, `Web`, `SharedKernel`.
- Test projects created: `Domain.Tests`, `Application.Tests`, `Integration.Tests`.
- Project references wired according to Clean Architecture dependency rules.
- Foundational packages installed (MediatR, FluentValidation, EF Core 8, Serilog, FluentAssertions, Moq).
- Core documentation established: `AGENTS.md`, `CODING_STANDARDS.md`, `README.md`, `docs/ARCHITECTURE.md`, `docs/ACCOUNTING.md`, `docs/DECISIONS.md`.
- CI pipeline workflow and `.gitignore`.
- SharedKernel foundational primitives (`BaseEntity`, `Result`, `Error`, `IDomainEvent`).
- Application and Infrastructure dependency injection and basic services.
