# Coding Standards

## Naming
- PascalCase for types, methods, properties, constants. camelCase for locals and parameters. `_camelCase` for private fields. Async methods end with `Async`.
- Entities are singular (Order). DbSet properties are plural (Orders). Tables use plural snake_case.
- DTOs end with Dto; commands with Command; queries with Query; handlers with Handler; validators with Validator; configurations with Configuration.

## Files and structure
- One type per file, namespace matches folder path, file-scoped namespaces.
- Maximum 300 lines per file and 40 lines per method; split if larger.
- Razor views: display logic only; use ViewModels, tag helpers and view components; JavaScript lives in wwwroot/js/modules, one file per screen.

## Style
- Nullable reference types enabled. Treat warnings as errors.
- Prefer immutability, records for DTOs and value objects, `required` members for mandatory data.
- No magic numbers or strings; use constants/enums (Permissions.cs, FeatureKeys.cs).
- Use `var` only when the type is obvious. Use expression-bodied members for one-liners.
- Comments explain why, not what. Public APIs have XML summaries.

## Architecture
- Controllers only call MediatR or application services.
- Repositories return domain entities or projections, never IQueryable to upper layers.
- Use dependency injection; no static state, no service locator.

## Database
- Code-first migrations, one migration per change with a clear name.
- Fluent configuration in Configurations/*.cs, never data annotations on entities.
- Decimal precision (18,2) for money, (18,3) for quantity.

## Testing
- Arrange-Act-Assert; method names `Method_Scenario_ExpectedResult`.
- Domain and Application logic coverage target 80 percent. Integration tests for repositories and settle-bill transaction.
- Each validator and each feature toggle has tests.

## Git
- Branch per phase/feature: `feature/phase-5-billing`. Conventional commits (feat:, fix:, refactor:, test:, docs:).
- Small commits; build and tests must pass before commit.

## UI consistency
- Same page layout, grid behaviour, button placement and colours everywhere.
- Currency, date and number formats come from settings, not from the code.
- Accessible: labels on inputs, keyboard operable, colour contrast 4.5:1.
