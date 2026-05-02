# Chronoscope — Copilot Instructions

## Project Overview

Chronoscope is a local-first photo indexing app. It connects to OneDrive via Microsoft Graph API, syncs photo metadata incrementally, and presents photos in a Timeline / Map explorer. Only metadata, thumbnails, and (future) embeddings are stored locally — original photos stay in OneDrive.

**Key docs:**
- `CONTEXT.md` — canonical domain language (use these terms, avoid the listed alternatives)
- `docs/SPEC.md` — MVP feature scope (Stages 1–8)
- `docs/ARCHITECTURE.md` — structural rules and technology choices

---

## Architecture

### Layer Structure

```
Domain → Application → Data / Infrastructure / Web → Host
```

| Project | Responsibility |
|---|---|
| `Chronoscope.Domain` | Entities and value objects. No EF attributes. No dependencies on other layers. |
| `Chronoscope.Application` | Use cases, DTOs, and **all interface definitions** under `Application/Abstractions/`. |
| `Chronoscope.Data` | EF Core `DbContext`, Fluent API config, migrations, repository implementations. |
| `Chronoscope.Infrastructure` | Graph API client, MSAL, EXIF reader, FaceONNX runner, temp file manager. |
| `Chronoscope.Web` | MVC controllers, Razor views, HTMX partials, Leaflet views. |
| `Chronoscope.Host` | `Program.cs`, DI composition, `IHostedService` workers. |

### Dependency Rules (strictly enforced)

- `Domain` has **zero** external dependencies — it is the core.
- `Application` depends only on `Domain`.
- `Data` and `Infrastructure` depend on `Application` (to implement interfaces in `Abstractions/`).
- `Web` depends on `Application` only — controllers call use cases, never repositories directly.
- `Host` wires everything together.

### DI Registration Convention

Each layer (except `Domain`) has a `<LayerName>Setup.cs` static class with an `Add<LayerName>()` extension method. `Host/Program.cs` calls all of them:

```csharp
builder.Services.AddApplication();
builder.Services.AddData(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWeb();
```

---

## Non-Negotiable Coding Rules

### No AutoMapper
All mapping is done manually, property by property. Never introduce AutoMapper or any mapping library.

```csharp
// correct
var vm = new PhotoViewModel
{
    Id = photo.InternalId,
    Filename = photo.Filename,
    TakenAt = photo.TakenAt,
};
```

### No EF Attributes on Domain Classes
EF is configured exclusively via **Fluent API** (`IEntityTypeConfiguration<T>`) in the `Data` layer. Domain entities must have no EF attributes (`[Key]`, `[Column]`, `[Required]`, etc.).

### All Interfaces in `Application/Abstractions/`
Repository contracts, service contracts, and any cross-layer interfaces are defined in `Chronoscope.Application/Abstractions/`. `Data` and `Infrastructure` implement them; nothing else defines them.

### Domain Entities Are EF Entities
No separate persistence models. Domain entities are mapped directly to the database via EF Fluent API. No mapping layer between domain and persistence.

### Controllers Are Thin
Business logic belongs in Application use cases / services — not in controllers. Controllers coordinate input → use case → view model → response.

### Strongly-Typed View Models
Never pass raw domain entities to Razor views. Always define a dedicated view model (prefer `record`). Map manually.

### HTMX Returns Partial Views
HTMX interactions return Razor partial views — not JSON. Use `Request.IsHtmx()` (or equivalent) to return a partial for HTMX requests and a full layout for direct navigation.

### Minimize JavaScript
Use HTMX attributes and CSS for interactivity wherever possible. JavaScript is only acceptable for Leaflet map integration.

### EF Migrations Workflow
- Use `dotnet ef` for all migration lifecycle operations (create, apply, rollback, remove).
- `dotnet-ef` must be installed as a **local tool** in the repository (`dotnet new tool-manifest`, then `dotnet tool install dotnet-ef`).
- Do not create or roll back migrations by manually editing migration files.
- Manual migration edits are allowed only for deterministic fixes after `dotnet ef` generation.

---

## Domain Language

Use the terms defined in `CONTEXT.md`. Key terms:

| Use | Avoid |
|---|---|
| Photo | image, file, picture |
| Source | provider, account, integration |
| Processing Status | state, stage, status |
| Sync | import, fetch, crawl |
| Index | database, store, cache |
| Thumbnail | preview, small image |

---

## Tech Stack

- **Backend**: ASP.NET MVC, C# 12+, .NET
- **Database**: PostgreSQL + PostGIS (geospatial queries)
- **ORM**: EF Core with Fluent API only
- **Auth**: MSAL Device Code Flow
- **Cloud**: Microsoft Graph API (OneDrive)
- **Frontend**: Razor views, Pico CSS v2, HTMX, Leaflet

---

## Agent Roles

- **`analytics` agent** — owns `docs/SPEC.md`, `docs/ARCHITECTURE.md`, `docs/ADR/`, `readme.md`, and task phase files. Never edits source code.
- **`developer` agent** — implements features strictly within the current phase (`docs/tasks/phase-X-*.md`). Never edits documentation.

Agents must read the relevant phase task file and referenced SPEC/ARCHITECTURE sections before writing any code.
