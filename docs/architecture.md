# Chronoscope — Architecture Overview

## Guiding Principles

- **Local-first metadata**: Photos live on OneDrive. Only metadata, embeddings, and thumbnails are stored locally.
- **Ephemeral downloads**: Photos are downloaded temporarily for processing (EXIF, thumbnails, face detection) and deleted immediately after.
- **No app-level authentication**: The app assumes deployment behind a reverse proxy (e.g., Nginx, Traefik, Authentik) that enforces access control.
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
- Uses **Microsoft Graph API** with **MSAL Device Code Flow**
- Refresh token persisted via MSAL token cache — silent re-auth on restart
- **Delta queries** track new/changed/deleted photos efficiently without full re-scans
- User configures one folder at setup time

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
- Schema tracks: photos, EXIF metadata, GPS points, thumbnails, face embeddings, person labels
- Domain entities are used directly as EF entities — no separate persistence model
- EF configured exclusively via **Fluent API** (`IEntityTypeConfiguration<T>`) — no EF attributes on domain classes
- Source-specific metadata (e.g., OneDrive item ID, delta token) stored as a **JSON column** on the photo entity, allowing future sources without schema changes

### Web UI
- **ASP.NET MVC + Razor** — server-side rendering
- **Pico CSS v2** — styling
- **HTMX** — partial page updates (filtering, pagination) without a JS framework
- **Leaflet** — interactive map with GPS pins
- No app-level auth — relies on reverse proxy (Nginx, Traefik, Authentik, etc.)

### Explorer (Unified View)
- Timeline and map are **linked**: selecting a date range updates map pins via HTMX
- Photos without GPS are shown only in the timeline
- On-demand photo display fetches the original from OneDrive at view time

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
    │                                  #   All dependencies mocked
    │
    ├── Chronoscope.Tests.Integration/ # Integration tests — real DB via TestContainers (PostgreSQL + PostGIS)
    │                                  #   Covers: repository implementations + HTTP layer (WebApplicationFactory)
    │
    └── Chronoscope.Tests.E2E/         # End-to-end tests — Playwright against a running instance
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
| `Chronoscope.Infrastructure` | Graph API client, MSAL token cache, EXIF reader, FaceONNX runner, temp file manager |
| `Chronoscope.Web` | MVC controllers, Razor views, Pico CSS layout, HTMX partials, Leaflet map views |
| `Chronoscope.Host` | Program.cs, DI registration, `IHostedService` sync/indexing workers, app startup |
| `Chronoscope.Tests.Unit` | Unit tests for controllers (no HTTP), use cases, and domain logic; all dependencies mocked |
| `Chronoscope.Tests.Integration` | Repository + HTTP integration tests using TestContainers (real PostgreSQL + PostGIS) and `WebApplicationFactory` |
| `Chronoscope.Tests.E2E` | End-to-end browser tests using Playwright against a running instance |

---

## Key Technology Choices

| Concern | Choice | Rationale |
|---|---|---|
| Language | C# / .NET | Learnable, strongly typed, good ecosystem |
| Cloud access | Microsoft Graph API | Official OneDrive API |
| Auth | MSAL Device Code Flow | Simple, familiar, token persisted |
| Database | PostgreSQL + PostGIS | Relational + native geospatial support |
| ORM | EF Core | Migrations + data access in one |
| Domain model | Anemic (entities = EF entities) | No BL to protect; avoids mapping overhead |
| EF configuration | Fluent API only | Keeps Domain free of EF attributes |
| Object mapping | Manual (`Property = Property`) | Explicit, greppable, no framework magic |
| Source metadata | JSON column on photo entity | Supports future sources without schema changes |
| Face recognition | FaceONNX + ONNX Runtime | Local, no cloud dependency |
| Frontend | ASP.NET MVC + Razor | SSR, no JS framework needed |
| Interactivity | HTMX | Lightweight, server-driven |
| Mapping | Leaflet | Open source, well supported |
| Styling | Pico CSS v2 | Minimal, semantic |
| App auth | None (reverse proxy) | Keeps app simple; proxy handles access |
