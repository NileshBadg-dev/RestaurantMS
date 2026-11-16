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
- **Decision**: Code-First EF Core 8 with Fluent Configurations. Default database is SQL Server with capability to switch to MySQL or PostgreSQL via configuration.
- **Context**: Hotels may host on Windows SQL Server or open-source Linux servers.
