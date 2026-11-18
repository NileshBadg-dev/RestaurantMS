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


## ADR 008: UI Data Abstraction via SampleData Provider Pattern (Phase 2)
- **Decision**: In Phase 2, all UI screens interface with typed `I*UiData` interfaces (e.g. `IDashboardUiData`, `ITablesUiData`, `IKitchenUiData`, `IBillingUiData`) implemented by `Sample*UiData` classes in `RestaurantMS.Web/SampleData/`. Dependency injection switches between sample data and future MediatR implementations via a single extension call `services.AddSampleUiData(useSampleData: true)`.
- **Context**: Rapid frontend UI development and verification without locking database schema or implementing premature backend mutations in Phase 2.
- **Consequences**: Controllers remain thin, views bind strictly to ViewModels and DTOs, zero hardcoded Razor view mockups, and seamless transition to production MediatR query handlers in Phase 3 without touching view markup.

## ADR 009: Web Current User Claims Resolution & Breadcrumb Navigation (Phase 2 Refinement)
- **Decision**: In `RestaurantMS.Web`, register `WebCurrentUser : ICurrentUser` using `IHttpContextAccessor` to bridge authentication cookie claims directly into `ICurrentUser` (User ID, Name, Roles, Permissions). Grant SuperUser status (`Owner`, `Admin`, `admin`) automatic visibility to all operational screens and permission actions. Add back button (`history.back()`) and breadcrumbs (`Home / [Module] / [Action]`) to `_Layout.cshtml`, and make the sidebar brand header clickable to `/Dashboard`.
- **Context**: In Phase 2, `CurrentUserService` was an infrastructure placeholder with empty claims, which caused `NavMenuViewComponent` to filter out all operational module links. Also, users required quick top-level back navigation and breadcrumbs across deep views.
- **Consequences**: Navigation menu reliably renders all allowed modules based on live user claims or superuser role; breadcrumbs provide contextual orientation; sidebar logo acts as a reliable home link.

## ADR 010: State Isolation, Interactive Zero-Dummy State & Udhari Credit Compliance
- **Decision**: Sample UI data stores maintain thread-safe in-memory collections initialized to clean zero state (empty active orders, empty KDS tickets, empty today's bills, all tables free). Billing operations dynamically transition table states and record settled bills in real-time. For Credit (Udhari) settlements, customer name, 10-digit phone, and physical address are mandatory prerequisites enforced on both client and controller layers. Provide statutory double-entry audit reporting in the Accounts module and dedicated system audit trail for Admin users.
- **Context**: Static pre-seeded records prevented comprehensive end-to-end POS validation by testers. Legal and financial recovery for restaurant udhari accounts requires verifiable customer contact and address data.
- **Consequences**: Realistic, testable POS workflows; audit trail records all cash/credit actions; statutory accounts reports verify double-entry consistency.

## ADR 011: Combined Staff Hub, Salary Calculation Service & Double-Entry Employee Advances
- **Decision**: Consolidate Staff List, Daily Attendance (with monthly calendar and payroll month lock), and Monthly Salary Statement into one 3-tab `/Staff` module, with a 4-tab `/Staff/Details/{id}` profile page (including read-only Advances summary linking to Accounts). Move Employee Advances management to `/Accounts/EmployeeAdvances` (`EmployeeAdvancesController`), posting balanced double-entry vouchers exclusively through `IAccountingService` (`Dr Employee Advances A/c 1150 / Cr Cash 1110 or Bank 1120`, reversed on recovery). Daily salary staff follow the unified formula `Basic (DailyRate × DaysInMonth) + Overtime (1.5× hourly) - AbsentDeduction (DailyRate × (Absent + 0.5×HalfDay)) - AdvanceRecovery`, with net pay floored at zero.
- **Context**: Prevents fragmented payroll/advance workflows while enforcing Clean Architecture rules (accounting writes only via `IAccountingService`, period locks respected, salary cap configurable via `MaxAdvanceSalaryPercent`, default 100%).
- **Consequences**: Every advance and recovery immediately appears as balanced lines in Day Book and Vouchers; salary calculation and advance rules are unit-tested independently of UI views.

## ADR 012: Billing Payment-Mode Bug Root Cause & Server-Backed BillingService / ReceiptRenderer Architecture
- **Root Cause Diagnosis (Why clicking Cash, UPI, Card, Split, and Udhari did nothing)**:
  1. **Missing mode-specific UI panels & handlers**: In `Views/Shared/_BillingPanel.cshtml`, all 5 payment radio buttons (`#payCash`, `#payUpi`, `#payCard`, `#paySplit`, `#payUdhari`) invoked `togglePaymentInputs()`, which exclusively checked `document.getElementById('payUdhari').checked` to show/hide `#creditDetailsSection`. No UI containers or handlers existed for Cash tender/change, UPI QR/UTR, Card last-4/approval, or Split multi-row payment breakdowns, so clicking Cash, UPI, Card, or Split produced zero visible response.
  2. **Missing CSS active-state selectors for custom button variants**: Bootstrap's `.btn-check:checked + .btn-outline-*` rules do not cover custom theme classes `.btn-outline-teal` and `.btn-outline-chili`, so clicking payment chips gave no visual selected highlight.
  3. **Inline partial script & full-page POST redirect**: `_BillingPanel.cshtml` embedded ~500 lines of inline JS instead of an external module (`billing.js`), lacked event delegation, had no client/server validation parity or idempotency key, and performed a full-page form POST redirect to `/Billing` instead of an AJAX `Result<T>` settlement that opens an iframe receipt print preview.
  4. **Missing double-entry accounting & receipt rendering**: `BillingController.Settle` wrote only to static sample lists without calling `IAccountingService` or rendering printable thermal/A4 receipts.
- **Decision**:
  - Introduce `IBillingService` / `BillingService` in Application to own all bill total calculations (subtotal, discount, packing charge, CGST/SGST, round-off, grand total), payment-mode validations (exact EN/MR messages), idempotency-key deduplication, customer credit checks, and balanced voucher posting via `IAccountingService`.
  - Refactor `_BillingPanel.cshtml` (zero inline scripts) + `wwwroot/js/modules/billing.js` using delegated event binding, live mode panels (Cash, UPI QR, Card, Split, Udhari via `_CustomerPicker`), server-mirrored math, and antiforgery-protected AJAX settlement.
  - Introduce `IReceiptRenderer` / `ReceiptRenderer` and `ReceiptController` serving printable 58 mm, 80 mm, and A4 HTML receipts (plus Parcel Token slips and KOT slips) inside a popup-free modal `<iframe>` (`_ReceiptPreviewModal.cshtml` + `receiptPreview.js`) with `Printing.AutoPrintOnSettle` and `DUPLICATE` reprint watermarking.
- **Consequences**: Every payment mode works reliably with instant visual feedback and server-enforced math; settled bills post balanced vouchers visible in Day Book and automatically display the print preview modal without browser popup blockers.
