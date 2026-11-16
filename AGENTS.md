# Agent Instructions

## Basic rules (always)
1. Read README.md, CODING_STANDARDS.md and docs/ARCHITECTURE.md before writing code.
2. Follow Clean Architecture. Domain -> nothing. Application -> Domain. Infrastructure, Api and Web -> Application. Never reference Infrastructure from Application or Domain.
3. ONE class, interface, enum or record per file. File name equals type name (Order.cs, IOrderRepository.cs, OrderStatus.cs). Folder equals namespace.
4. Interfaces start with I and live in the layer that needs them (repository and service interfaces in Application; implementations in Infrastructure).
5. No business logic in Razor views, controllers or EF configurations. Logic belongs in Domain entities or Application services/handlers.
6. Every user-facing text comes from resource files (en, mr). No hardcoded strings.
7. Use the shared partial views, tag helpers and view components (AppDataGrid, BillingPanel, ConfirmDialog, MoneyInput, CustomerPicker, ImageUploader). Never build a one-off grid or billing layout.
8. Every command has a FluentValidation validator. Every new feature has unit tests.
9. Money uses decimal and the Money value object, never double or float.
10. Never commit secrets. Never run destructive database commands without being asked.

## Development & Verification Workflow
1. **Do the changes**: Implement the code, tests, and documentation adhering to Clean Architecture.
2. **Build and run in browser**: Compile the solution, ensure tests pass with zero warnings, and run the web application outside sandbox so it is accessible in the browser.
3. **User verification checkpoint**: Present the application URL/status to the user and wait for the user to manually verify in the browser. Do not commit before user verification.
4. **Commit and prepare PR**: Once verified and approved by the user, stage and commit changes locally on the feature branch. Provide the PR details / GitHub PR creation URL.
5. **Manual push by user**: NEVER automatically push commits to remote. The user will push manually (`git push`).

## Definition of done
- Solution builds with zero warnings; all tests pass.
- Validation, permission check, audit log and soft delete handled.
- English and Marathi strings added.
- Feature toggle respected where applicable.
- Migration added and seed data updated if the schema changed.
- Docs and CHANGELOG updated.

## Advanced rules
- **Permissions**: each use case declares its permission code via the `IRequirePermission` marker interface; PermissionBehavior enforces it. Add the code to the seed and to the role matrix.
- **Transactions**: bill settlement, credit posting and voucher creation run in one IUnitOfWork transaction. Make settle idempotent with a unique settlement key.
- **Accounting**: never write to Voucher tables directly from billing code; call IAccountingService. Vouchers must balance and respect locked periods.
- **Credit**: any change in credit amount of a settled bill must create a ledger adjustment, never edit old ledger rows.
- **Concurrency**: use RowVersion; handle DbUpdateConcurrencyException with a friendly message.
- **Performance**: no queries in loops; use projection (Select to DTO) for lists; server-side paging.
- **Files**: upload through IFileStorage only; verify content type by signature; GUID names.
- **Feature toggles**: access with IFeatureService; the UI hides and the handler also rejects.
- **Events**: raise domain events (BillSettledEvent) and handle side effects such as stock deduction and printing in handlers, not inside the entity.
- **Time**: use IDateTime (UTC), never DateTime.Now.
- **MVC**: controllers stay thin (bind ViewModel, call MediatR, return View, Json or Redirect). Pass ViewModels to views, never entities. Use [ValidateAntiForgeryToken] on every POST, Post-Redirect-Get after saves, and one JavaScript module per screen (no inline scripts). AJAX endpoints return a Result<T> JSON shape.
- **Errors**: return Result<T> for expected failures; throw DomainException only for violated invariants.
- If the specification is ambiguous, choose the simplest option, document the assumption in docs/DECISIONS.md and continue.
