# Phase 1 — Solution Scaffold

## Goal
Establish the layered solution skeleton, database connectivity, and baseline persistence so later phases can build on a stable foundation.

## Dependencies
- None (starting phase).

## References
- [SPEC — MVP scope](../spec.md#chronoscope--mvp-specification)
- [ARCHITECTURE — Project Structure](../architecture.md#project-structure)
- [ARCHITECTURE — Dependency Rules](../architecture.md#dependency-rules)
- [ARCHITECTURE — DI Registration Convention](../architecture.md#di-registration-convention)

## Tasks

- [x] Task 1.1 — Create solution and projects by layer
- [x] Task 1.2 — Configure EF Core + PostgreSQL + PostGIS
- [x] Task 1.3 — Add first migration and initialize schema baseline
- [x] Task 1.4 — Wire DI setup extensions in Host startup

### Task 1.1 — Create solution and projects by layer
**Depends on:** None  
**Acceptance criteria:**
- `Chronoscope.sln` includes `Domain`, `Application`, `Data`, `Infrastructure`, `Web`, and `Host`.
- Project references follow architecture dependency rules.
- No EF or framework attributes are introduced in `Domain`.

### Task 1.2 — Configure EF Core + PostgreSQL + PostGIS
**Depends on:** Task 1.1  
**Acceptance criteria:**
- `Data` layer configures EF Core with NetTopologySuite support.
- Connection string is resolved from configuration.
- PostGIS-enabled provider configuration is active.

### Task 1.3 — Add first migration and initialize schema baseline
**Depends on:** Task 1.2  
**Acceptance criteria:**
- Initial migration is created and applies successfully.
- Database can be created from migrations without manual SQL.
- Baseline schema is intentionally minimal and ready for later phases.

### Task 1.4 — Wire DI setup extensions in Host startup
**Depends on:** Task 1.1  
**Acceptance criteria:**
- Each layer exposes `<LayerName>Setup` registration extension.
- `Host` startup composes services via `AddApplication()`, `AddData()`, `AddInfrastructure()`, and `AddWeb()`.
- App starts with resolved dependencies and database connectivity check.
