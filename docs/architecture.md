# Chronoscope — Architecture Overview

## Guiding Principles

- **Local-first metadata**: Photos live on OneDrive. Only metadata, embeddings, and thumbnails are stored locally.
- **Ephemeral downloads**: Photos are downloaded temporarily for processing (EXIF, thumbnails, face detection) and deleted immediately after.
- **Proxy-enforced app access**: Chronoscope assumes deployment behind a reverse proxy (e.g., Nginx, Traefik, Authentik) that authenticates the human user before they can access the app.
- **Separate OneDrive source auth**: OneDrive access uses delegated Microsoft Graph auth inside the app and is independent from the reverse-proxy session.
- **Single deployable**: One executable hosts both the background worker (sync/indexing) and the web UI (ASP.NET MVC).
- **No rich domain model**: The app is data-centric with no business logic or state machines. Domain entities are used directly as EF entities — no separate persistence model, no mapping layer between them.
- **EF configured via Fluent API only**: No EF attributes on domain classes. All EF configuration lives in the Data layer (`IEntityTypeConfiguration<T>`), keeping `Domain` free of any EF dependency.
- **No AutoMapper**: All object mapping is done manually (`Property = Property`). Explicit mapping is easier to reason about and grep.

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      Chronoscope                        │
│                                                         │
│  ┌─────────────────┐     ┌───────────────────────────┐  │
│  │  Background     │     │      ASP.NET MVC          │  │
│  │  Worker         │     │      Web UI               │  │
│  │  (IHostedService│     │                           │  │
│  │                 │     │  - Settings / Auth setup  │  │
│  │  - Delta sync   │     │  - Timeline view          │  │
│  │  - EXIF extract │     │  - Map view (Leaflet)     │  │
│  │  - Thumbnails   │     │  - Unified explorer       │  │
│  │  - Face pipeline│     │  - HTMX partials          │  │
│  └────────┬────────┘     └────────────┬──────────────┘  │
│           │                           │                  │
│           └──────────┬────────────────┘                  │
│                      │                                   │
│              ┌───────▼────────┐                          │
│              │   EF Core      │                          │
│              │   Data Layer   │                          │
│              └───────┬────────┘                          │
│                      │                                   │
│              ┌───────▼────────┐                          │
│              │  PostgreSQL    │                          │
│              │  + PostGIS     │                          │
│              └────────────────┘                          │
└─────────────────────────────────────────────────────────┘
          │
          │  Microsoft Graph API (OneDrive)
          ▼
   ┌─────────────┐
   │   OneDrive  │
   │   (source   │
   │   of truth) │
   └─────────────┘
```

---

## Components

### OneDrive Connector
- Uses **Microsoft Graph API** with **Authorization Code Flow** via `Microsoft.Identity.Web`
- Azure app registration is a **Web** app scoped to **Azure AD and personal Microsoft accounts**
- Delegated scopes: `Files.Read`, `offline_access`, `User.Read`
- Token cache is persisted in PostgreSQL via a distributed cache and silently refreshes access tokens on restart
- Token cache encryption uses ASP.NET Core Data Protection with keys persisted to a filesystem volume
- **Delta queries** track new/changed/deleted photos efficiently without full re-scans
- User configures one folder at setup time
- This flow authorizes OneDrive access for sync; it is not the app's primary access-control boundary

### Indexing Pipeline (Background Worker)
Runs as `IHostedService` using an in-process **TPL Dataflow ETL workflow** (bounded stages with controlled parallelism). Processing steps per photo:

1. Fetch metadata from Graph API (filename, modified date, size)
2. Download photo temporarily to local disk
3. Extract **EXIF** data: timestamp, GPS coordinates
4. Generate **thumbnail** (resize, store in DB)
5. *(Future)* Run **face detection + embedding** (FaceONNX + ONNX Runtime)
6. Delete local copy
7. Persist all extracted data to PostgreSQL

### Data Layer
- **EF Core** for all DB access (repositories + migrations)
- **PostgreSQL + PostGIS** for geospatial queries (proximity, bounding box)
- EF uses `UseSnakeCaseNamingConvention()` for PostgreSQL naming consistency
- Schema tracks: photos, EXIF metadata, GPS points, thumbnails, face embeddings, person labels
- Domain entities are used directly as EF entities — no separate persistence model
- EF configured exclusively via **Fluent API** (`IEntityTypeConfiguration<T>`) — no EF attributes on domain classes
- Consumers use `IDbContextFactory<ChronoscopeDbContext>` instead of injecting `ChronoscopeDbContext` directly
- Source-specific metadata (e.g., OneDrive item ID, delta token) stored as a **JSON column** on the photo entity, allowing future sources without schema changes

### Migration Strategy
- **Production/staging**: migrations run as a dedicated one-shot container (`--migrate-and-exit`) before the app starts.
- **Development**: migrations are manual (`dotnet ef database update`).
- **Normal app startup never auto-migrates**; it fails fast if there are pending migrations.
- Deploys are orchestrated with Docker Compose using `depends_on: condition: service_completed_successfully` so app startup is blocked until migration succeeds.

### Web UI
- **ASP.NET MVC + Razor** — server-side rendering
- **Pico CSS v2** — styling
- **HTMX** — partial page updates (filtering, pagination) without a JS framework
- **Leaflet** — interactive map with GPS pins
- App access is enforced by a reverse proxy; the app only runs Microsoft sign-in when the user needs to connect or reconnect OneDrive.

### Logging
- **Serilog** is the logging pipeline in all environments.
- Logging configuration is sourced from environment-specific app configuration files.
- **Development** uses Serilog Console sink with standard readable console output.
- **Non-development environments** use Serilog JSON console output (not compact JSON) for structured log ingestion.

### Explorer Views
- Timeline and Map are separate pages that share the same date-range filtering model
- Photos without GPS are shown only in the Timeline view
- Photo details are presented through a shared side-panel pattern backed by server-rendered HTML

---

## Data Flow

```
OneDrive (source of truth)
    │
    │  Graph API delta query
    ▼
Background Worker
    │
    ├─► Download photo (temp)
    │       │
    │       ├─► Extract EXIF (timestamp, GPS)
    │       ├─► Generate thumbnail
    │       └─► [Future] Face detection + embedding
    │
    ├─► Delete temp file
    └─► Persist metadata + thumbnail → PostgreSQL (PostGIS)

Web UI (on user request)
    │
    ├─► Query PostgreSQL for index data
    ├─► Serve timeline + map views
    └─► Fetch original photo from OneDrive for display
```

---

## Project Structure

```
Chronoscope.sln
│
├── src/
│   ├── Chronoscope.Domain/          # Domain entities, value objects, repository interfaces
│   │                                #   No dependencies on other projects
│   │
│   ├── Chronoscope.Application/     # Use cases & application services (business logic)
│   │                                #   Defines all interfaces under Application/Abstractions/
│   │                                #   References: Domain
│   │
│   ├── Chronoscope.Data/            # EF Core DbContext, migrations, repository implementations
│   │                                #   References: Application, Domain
│   │
│   ├── Chronoscope.Infrastructure/  # External integrations — Graph API, MSAL, EXIF, FaceONNX, file I/O
│   │                                #   References: Application, Domain
│   │
│   ├── Chronoscope.Web/             # ASP.NET MVC — controllers, Razor views, HTMX partials
│   │                                #   References: Application, Domain
│   │
│   └── Chronoscope.Host/            # Entry point — Program.cs, DI composition, IHostedService workers
│                                    #   References: Web, Application, Infrastructure, Data
│
└── tests/
    ├── Chronoscope.Tests.Unit/        # Unit tests — controllers (no HTTP pipeline), use cases, domain logic
    │                                  #   Stack: xUnit + Moq + AutoFixture
    │
    ├── Chronoscope.Tests.Integration/ # Integration tests — real DB via TestContainers (PostgreSQL + PostGIS)
    │                                  #   One container per run, per-test reset, migrations only (no EnsureCreated)
    │
    └── Chronoscope.Tests.E2E/         # End-to-end suite placeholder (enabled when dedicated env is ready)
```

### Dependency Rules
- `Domain` has **no dependencies** on other projects — it is the core
- `Application` depends only on `Domain` — defines all interfaces under `Application/Abstractions/`
- `Data` and `Infrastructure` both depend on `Application` — they implement the interfaces defined in `Abstractions/`
- `Web` depends on `Application` only — controllers call use cases, never repositories directly
- `Host` wires everything together via dependency injection

### DI Registration Convention
Each layer (except `Domain`) exposes a `<LayerName>Setup.cs` static class with an extension method that registers its own services. `Host` calls all of them in `Program.cs`:

```csharp
// Chronoscope.Application/ApplicationSetup.cs
builder.Services.AddApplication();

// Chronoscope.Data/DataSetup.cs
builder.Services.AddData(builder.Configuration);

// Chronoscope.Infrastructure/InfrastructureSetup.cs
builder.Services.AddInfrastructure(builder.Configuration);

// Chronoscope.Web/WebSetup.cs
builder.Services.AddWeb();
```

### Layer Responsibilities

| Project | Responsibility |
|---|---|
| `Chronoscope.Domain` | `Photo`, `GpsPoint`, `Face`, `Person` entities and value objects; no EF attributes, no dependencies |
| `Chronoscope.Application` | Use cases, DTOs, and **all interface definitions** (`Abstractions/` — repository contracts, service contracts) |
| `Chronoscope.Data` | EF Core `DbContext`, PostGIS configuration, EF migrations, repository implementations |
| `Chronoscope.Infrastructure` | Graph API client, `Microsoft.Identity.Web` integration, distributed token cache, EXIF reader, FaceONNX runner, temp file manager |
| `Chronoscope.Web` | MVC controllers, Razor views, Pico CSS layout, HTMX partials, Leaflet map views |
| `Chronoscope.Host` | Program.cs, DI registration, `IHostedService` sync/indexing workers, app startup |
| `Chronoscope.Tests.Unit` | Unit tests for controllers (no HTTP), use cases, and domain logic using xUnit + Moq + AutoFixture |
| `Chronoscope.Tests.Integration` | Repository + HTTP integration tests using TestContainers (real PostgreSQL + PostGIS) with migrations and per-test reset |
| `Chronoscope.Tests.E2E` | End-to-end suite placeholder; enabled after dedicated test environment is provisioned |

---

## Testing Strategy

### Test Types
- **Unit tests**: fast tests for application logic and MVC decision logic without real I/O.
- **Integration tests**: real framework and persistence wiring (DI, EF, PostgreSQL/PostGIS, HTTP pipeline where needed).
- **E2E tests**: reserved as a placeholder until a dedicated test environment is available.

### Unit Testing Policy
- Framework/tooling: **xUnit + Moq + AutoFixture**.
- Controller tests assert **observable MVC outputs** (`ViewResult`, `PartialViewResult`, redirects, model values).
- Avoid over-coupling to internals (no assertions on call ordering unless behavior is externally dependent on branch choice).
- Naming convention: `Method_State_Expected`.

### Integration Testing Policy
- Use **PostgreSQL + PostGIS via TestContainers** (no SQLite/InMemory substitution).
- Use **one container per test run** with **per-test reset** for isolation.
- Apply **real EF migrations** in tests; do not use `EnsureCreated`.

### Quality Gate Policy
- No hard coverage threshold initially.
- Coverage is published in CI for visibility and baseline tracking.

---

## Key Technology Choices

| Concern | Choice | Rationale |
|---|---|---|
| Language | C# / .NET | Learnable, strongly typed, good ecosystem |
| Cloud access | Microsoft Graph API | Official OneDrive API |
| OneDrive auth | Authorization Code Flow via `Microsoft.Identity.Web` | Correct fit for a browser-hosted setup flow, supports silent refresh, and works cleanly with persistent encrypted token caching for Azure AD and personal accounts |
| Database | PostgreSQL + PostGIS | Relational + native geospatial support |
| ORM | EF Core | Migrations + data access in one |
| PostgreSQL naming | `UseSnakeCaseNamingConvention()` | Consistent DB naming and smoother SQL interoperability |
| DbContext access | `IDbContextFactory<ChronoscopeDbContext>` | Safe context lifetime handling outside request-scoped flows |
| Migration execution | One-shot `--migrate-and-exit` mode | Keeps schema changes explicit and deterministic in deploy flow |
| Domain model | Anemic (entities = EF entities) | No BL to protect; avoids mapping overhead |
| EF configuration | Fluent API only | Keeps Domain free of EF attributes |
| Object mapping | Manual (`Property = Property`) | Explicit, greppable, no framework magic |
| Source metadata | JSON column on photo entity | Supports future sources without schema changes |
| Face recognition | FaceONNX + ONNX Runtime | Local, no cloud dependency |
| Frontend | ASP.NET MVC + Razor | SSR, no JS framework needed |
| Interactivity | HTMX | Lightweight, server-driven |
| Mapping | Leaflet | Open source, well supported |
| Styling | Pico CSS v2 | Minimal, semantic |
| Unit test stack | xUnit + Moq + AutoFixture | Readable tests with explicit mocking and fixture generation |
| Integration test DB | PostgreSQL + PostGIS via TestContainers | Matches production provider features (geo/vector) |
| App auth | Reverse proxy authentication | Keeps Chronoscope focused on photo indexing while the deployment edge controls who can open the app |
| Logging | Serilog in all environments (config-driven) | Single logging pipeline with environment-specific formatting: readable console in Development, JSON console in non-development |
| MSBuild defaults | `Directory.Build.props` at repo root | Centralizes shared project properties and build settings across all projects |
| NuGet versioning | `Directory.Packages.props` at repo root | Central package management with consistent package versions across the solution |
