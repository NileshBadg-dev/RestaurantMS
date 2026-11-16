# Changelog

All notable changes to the Restaurant Management System project will be documented in this file.

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
