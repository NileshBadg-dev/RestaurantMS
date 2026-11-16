# Architecture

## 1. Layers and Dependency Rule
The solution follows Clean Architecture principles. Dependencies point inward only:
- **Domain** depends on nothing (except SharedKernel).
- **Application** depends on Domain and SharedKernel.
- **Infrastructure**, **Api**, and **Web** depend on Application, Domain, and SharedKernel.
- Infrastructure and Web never reference each other directly for business logic; Web references Infrastructure only for Dependency Injection composition at startup.

```text
+-------------------------------------------------------------+
|                     Web (MVC) / Api (REST)                  |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                     Application Layer                       |
|  - Features (Commands / Queries / Handlers / Validators)    |
|  - Interfaces (Repositories, Services, Providers)           |
|  - Pipeline Behaviors (Validation, Logging, Permission)     |
+-------------------------------------------------------------+
               |                               |
               v                               v
+-----------------------------+ +-----------------------------+
|        Domain Layer         | |     Infrastructure Layer    |
|  - Entities, Aggregates     | |  - EF Core Persistence      |
|  - Value Objects, Enums     | |  - Repositories Impl        |
|  - Domain Events            | |  - External Services        |
|  - Domain Exceptions        | |  - Serilog, Identity        |
+-----------------------------+ +-----------------------------+
               \                               /
                \                             /
                 v                           v
              +---------------------------------+
              |          SharedKernel           |
              |  - BaseEntity, Result<T>, Error |
              +---------------------------------+
```

## 2. Request Flow
1. **User interaction**: Browser triggers action (Form POST or AJAX JSON request).
2. **MVC Controller**: Thin controller binds to ViewModel, calls MediatR `ISender.Send(command/query)`.
3. **Pipeline Behaviors**:
   - `LoggingBehavior`: logs request name, user, timing.
   - `PermissionBehavior`: checks `IRequirePermission` against current user's claims/permissions.
   - `ValidationBehavior`: executes FluentValidation rules; short-circuits on failure.
4. **Command / Query Handler**:
   - Executes use case logic via Domain models and Repositories / Services.
   - Raises domain events if applicable.
   - Persists state changes through `IUnitOfWork`.
5. **Result & Response**: Returns `Result<T>` to Controller; Controller renders Razor view or returns `Result<T>` JSON response.

## 3. Cross-Cutting Concerns
- **Logging**: Serilog structured logging writing to file and database sinks.
- **Auditing**: `IAuditable` tracking `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` on entities; explicit `AuditLog` records for sensitive operations.
- **Localisation**: `.resx` files (`en` and `mr`) for all user-facing labels, messages, and validation errors.
- **Feature Toggles**: `IFeatureService` determines if features (Kitchen, Credit, Inventory, etc.) are enabled. UI hides corresponding menus and handlers reject requests when disabled.

## 4. Billing and Settlement Transaction Sequence
1. Order items and discounts validated.
2. `BillingService` calculates subtotal, taxes (CGST/SGST), service charges, round-off, and grand total.
3. Settlement initiated with unique idempotency key.
4. In a single atomic `IUnitOfWork` database transaction:
   - Bill updated to `Settled`.
   - Payment records created (`BillPayment`).
   - If payment includes credit (udhari): customer outstanding increased, `CustomerLedgerEntry` (debit) created.
   - Double-entry accounting vouchers automatically generated and posted.
5. Domain event `BillSettledEvent` published (triggering inventory deduction, receipt printing, SignalR table status update).

## 5. Credit / Udhari and Accounting Posting
- Credit enabled per customer with credit limits and status checks.
- Changes to settled bills create adjustment entries; historical ledger records remain immutable.
- Auto-posting rules balance every transaction across double-entry accounts.

## 6. File Storage and Image Processing
- File uploads handled via `IFileStorage`.
- Files validated by content magic bytes (JPG/PNG/WEBP), limited to 2 MB.
- Images resized to 800px main image and 200px thumbnail.
- Stored with GUID filenames, saving only relative paths in the database.

## 7. Deployment and Backup
- Designed for hotel LAN deployment with local database server (SQL Server or MySQL/PostgreSQL).
- Automatic local scheduled backups with configurable retention and Admin restore capability.
- Multi-branch readiness: `BranchId` placeholder prepared for future multi-property extension.
