# Changelog

All notable changes to the Restaurant Management System project will be documented in this file.

## [Phase 2] - Colourful UI Kit & Complete Screen Suite - 2026-11-17
### Added
- **Design System & Visual Identity**:
  - Semantic CSS variable tokens: Plum (`#3D1F6D`), Chili (`#C62839`), Saffron (`#FF9F1C`), Teal (`#0B7A70`), Ink (`#1B1033`), and Warm Cream (`#FFF8EC`).
  - Typography: Inter font + Noto Sans Devanagari with local font fallback.
  - POS keyboard shortcuts: `F2` (New Dine-in Order), `F4` (New Parcel), `F5` (Send KOT / Refresh), `F9` (Bill), `F10` (Settle).
- **Reusable Partial Views & Custom Tag Helpers**:
  - `_AppDataGrid.cshtml` with server-side paging, search, and sorting.
  - `_BillingPanel.cshtml` unified POS billing and settlement interface with multi-tender payment chips (Cash, UPI, Card, Udhari) and credit limit alerts.
  - `_Chart.cshtml` wrapper for Chart.js (stacked bar, line, and bar charts).
  - `_KpiCard.cshtml`, `_StatusChip.cshtml`, `_EmptyState.cshtml`, `_PageHeader.cshtml`, `_ReportFilterBar.cshtml`, `_FormLayout.cshtml`.
  - TagHelpers: `<money-input>`, `<quantity-input>`, `<mobile-input>`, `<date-range-picker>`, `<status-chip>`, `<permission>`.
- **Complete Suite of Application Screens**:
  - **Login**: Split layout with Plum/Chili gradient brand panel, cream form panel, show/hide password, English/Marathi switcher, and mobile responsive layout matching design Page 1 & 4.
  - **Dashboard**: 5 KPI cards, sales-by-hour stacked bar chart, top-selling items, 7-day revenue trend, table turnover card, channel split card, active tables table, and settled orders grid matching design Page 3.
  - **Tables Floor Plan**: Floor view by section with status colors (Free, Occupied, Reserved, Cleaning), elapsed duration, running bill amount, and table action menus matching design Page 2.
  - **Parcel / Takeaway**: Active parcel queue, token tracker, time elapsed indicators, and new takeaway creation form with item picker.
  - **Order Taking**: Quick order entry with category filters, search, touch-friendly dish cards, running KOT list, and keyboard shortcuts.
  - **Kitchen Display System (KDS)**: 3-column Kanban board (New, In Kitchen, Ready to Serve) with timer chips, late order warning flashes, and stage transition buttons matching design Page 5.
  - **Billing & Settlement**: POS settlement panel supporting Cash, UPI, Card, and Udhari credit with customer ledger balance and credit limit checks matching design Page 6.
  - **Customers & Credit (Udhari)**: Customer list, quick registration modal, customer account ledger, cash receipt entry, and ageing analysis report (0-30, 31-60, 61-90, 90+ days).
  - **Menu Management**: Category management grid with item counts, dish management table with vegetarian badges, photo preview, pricing, and availability toggle.
  - **Inventory & Stock**: Stock items list with valuation, purchase inward entry, vendor management, and low stock reorder alert screen.
  - **Accounts & Finance**: Chart of Accounts tree view, voucher entry, Day Book, Cash Book, Trial Balance, Profit & Loss statement, and Balance Sheet.
  - **Reports Hub**: Standard reports catalog (Sales, Financial, GST, Operations) with date range filter bar, export to Excel/PDF, and printable tabular layout.
  - **Staff Management**: Employee roster, duty shifts, and on-duty attendance status tracker.
  - **Table Reservations**: Advance booking log, guest count, table assignment, and seating action.
  - **Day Closing (Shift Finalization)**: End-of-day register closing with cash-in-drawer reconciliation, discrepancy tracker, and register lock.
- **UI Data Layer & Sample Data Decoupling**:
  - Typed interfaces in `Application/Common/Interfaces/UiData/` and sample implementations in `Web/SampleData/` registered via `services.AddSampleUiData(useSampleData: true)`.
  - Full Marathi (`SharedResource.mr.resx`) and English (`SharedResource.en.resx`) localizations for all navigation items, KPI metrics, buttons, and validation messages.
- **Navigation & Brand Experience**:
  - `WebCurrentUser` bridges session cookie claims into `ICurrentUser`, ensuring all dynamic navigation items render seamlessly.
  - Made sidebar brand logo and hotel name clickable, routing directly to the Dashboard.
  - Cleaned topbar by removing hamburger and back buttons, maintaining clean breadcrumb navigation.
  - **Unified Masters Module**: Grouped all master data catalogs (Menu, Customers, Inventory, Staff, Accounts, Reservations, Hotel Settings, Users, Roles, Feature Toggles) under a unified **Masters** module with a dedicated Masters Hub page (`/Masters`) and collapsible sidebar navigation.
  - **Modern Toggle Switches**: Restyled `.form-switch .form-check-input` with sleek horizontal sliding pill aesthetics, resolving vertical deformation.
  - **Button & Grid Alignment**: Standardized `.btn` flex centering across the app and enforced strict cell alignments (`text-start`, `text-center`, `text-end`) across all datagrids.
  - **Login Button**: Updated action button label to "Sign In to POS" / "पीओएस मध्ये लॉगिन करा".
  - **Clean Testing State**: Removed all static dummy records across Tables, Orders, Billing, Parcels, and Kitchen display to enable realistic interactive end-to-end testing from zero state.
  - **Mandatory Udhari Validations**: Enforced mandatory Customer Name, valid 10-digit Mobile Number, and Physical Address for all Credit (Udhari) settlements on both UI and backend.
  - **Interactive Billing Suite**: Quick item addition by short code (101-113), browse dishes modal, order type switching, bill hold, real-time discount calculation, and 80mm/50mm thermal receipt printing and WhatsApp sharing.
  - **Accounts & Statutory Audit Hub**: Added complete accounts suite to Masters tab (Chart of Accounts, Vouchers, Day Book, Cash Book, Bank Book, Trial Balance, P&L, Balance Sheet, General Ledger Summary, and Accounts Audit Report with single date and date range selection).
  - **Admin System Audit Log**: Dedicated `/AuditLogs` interface restricted to Admin/Owner tracking settlements, KOT generation, cancellations, discounts, voids, and logins in real time.
  - **Sidebar & Layout Improvements**: Sidebar starts expanded by default, `<` / `>` toggle chevron placed beside the brand logo, and all hamburger and back button icons removed from headers.
- **Unit & Integration Tests**:
  - FluentValidation tests for all Phase 2 view models (`UiValidatorTests.cs`).
  - Web current user claims and dynamic menu tests (`NavMenuAndUserTests.cs`).
  - Total test count expanded to 109 tests passing with 0 failures and 0 warnings.

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
