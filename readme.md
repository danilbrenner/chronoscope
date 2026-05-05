# Chronoscope  

Chronoscope connects to cloud photo archive and builds a powerful index from it. It it by **time, location, and people** — creating a searchable timeline that understands who was where and when, across years.

Everything stays under your control: photos are processed locally, and the index is stored on your machine.

### Core Goals
- Connects to OneDrive
- Multi-dimensional indexing: Time + GPS + People (face recognition)
- Learnable .NET implementation using modern libraries
- Incremental updates when new photos appear in OneDrive

### Key Features
- **OneDrive Sync** — Automatically discover and fetch new or changed photos
- **Time Indexing** — Chronological timeline
- **Location Indexing** — GPS-based map view
- **People Indexing** — Face detection + recognition (handles aging)
- **Unified Search** — Query by date range, location, person, or any combination
- **Local Database** — Fast querying even with large archives

### How It Works (High-Level Pipeline)

1. **OneDrive Connector** — Authenticates and lists/downloads photos from your chosen folders.
2. **EXIF Processor** — Extracts timestamps and GPS coordinates.
3. **Face Pipeline** (using FaceONNX + ONNX Runtime):
    - Detect faces → align → generate embedding vectors
    - Cluster similar faces across years and support manual labeling
4. **Unified Index** — Stores everything in PostgreSQL
5. **Explorer** — Interactive map + timeline + people browser

### Technology Stack
- **Language**: C# / .NET
- **Cloud Access**: Microsoft Graph API (OneDrive)
- **App Access Auth**: Reverse proxy authentication
- **OneDrive Auth**: Microsoft sign-in via `Microsoft.Identity.Web` for Azure AD and personal Microsoft accounts
- **Face Recognition**: FaceONNX + ONNX Runtime
- **Database**: PsotgreSQL
- **Frtontend**: ASP.NET MVC + Razor (Server-Side Rendering)
  * `Styling`: Pico CSS v2
  * `Interactivity`: HTMX
  * `Mapping`: Leaflet

### Project Status
Early development.  
Focus is on reliable OneDrive integration + multi-dimensional indexing (time + location + people).

### Migrations & Deployment Mode
- **Production/Staging**: run migrations in dedicated mode before app startup:
  - `Chronoscope.Host --migrate-and-exit`
  - Use `compose.prod.yml` where `migrator` must complete successfully before `app` starts.
- **Development**: run migrations manually:
  - `dotnet ef database update --project src/Chronoscope.Data --startup-project src/Chronoscope.Host`
- In normal mode, the app fails fast when pending migrations exist.

### Testing Strategy (Current)
- **Unit tests**: xUnit + Moq + AutoFixture.
- **Integration tests**: PostgreSQL + PostGIS via TestContainers, one container per run, per-test reset, real migrations.
- **E2E tests**: placeholder until dedicated test environment is available.
