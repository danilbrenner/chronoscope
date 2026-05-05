# Chronoscope — Project Stages

Each stage is a self-contained milestone that leaves the project in a working, testable state.

---

## Stage 1 — Solution Scaffold
Set up the .NET solution structure, EF Core, and PostgreSQL + PostGIS connectivity.

- Create `.NET` solution with initial projects (web, worker, data layer)
- Configure EF Core with `NetTopologySuite` for PostGIS support
- Establish PostgreSQL connection
- Write and apply first migration (empty schema baseline)
- Verify DB connectivity on startup

---

## Stage 2 — ASP.NET MVC Shell
Minimal working web app with layout and routing.

- ASP.NET MVC project with Razor views
- Pico CSS v2 integrated
- Base layout with navigation skeleton
- Placeholder home/index route

---

## Stage 3 — OneDrive Connection Setup
Browser-based Microsoft sign-in with persistent token protection and the first-run setup entry point.

- Integrate `Microsoft.Identity.Web` using Authorization Code Flow for personal Microsoft accounts
- Setup page starts Microsoft sign-in via browser redirect instead of showing a device code
- Persist the token cache in PostgreSQL and enable encryption
- Persist ASP.NET Core Data Protection keys to a Docker-mounted filesystem volume
- Verify Graph API connectivity and expose connection state in the UI

---

## Stage 4 — Folder Configuration and Incremental Sync
Folder verification plus discovery of new, changed, and deleted Photos.

- UI for user to enter a single OneDrive folder path and verify it through Graph API before saving
- Persist the Source configuration including folder path and delta token
- Background worker runs Graph API **delta queries**
- New/changed/deleted Photos are reflected in the Index
- Photo discovery persists metadata (`filename`, `sourceId`, modified date, size) with processing ready for the next stage

---

## Stage 5 — Photo Processing Pipeline
Complete per-Photo processing from temporary download through thumbnail generation.

- Download each discovered Photo to a temp path
- Extract **timestamp** and **GPS coordinates** from EXIF
- Fallback `takenAt` to OneDrive modified date when EXIF timestamp is missing
- Generate a 400px thumbnail and store it in the DB
- Delete the temp file immediately after processing
- Mark Photos as `Processed` or `Failed` with no retry in MVP

---

## Stage 6 — Timeline View
Chronological photo browser.

- Date range filter defaults to the last 30 days
- Summary bar shows counts with and without GPS coordinates
- Table view lists Photos ordered by timestamp (newest first)
- Each row shows thumbnail, filename, date/time, and GPS indicator
- HTMX "Load more" appends additional rows
- Selecting a row opens the Side Panel

---

## Stage 7 — Map View
Leaflet map with GPS-located photos.

- Leaflet map renders Photos with GPS coordinates inside the active date range
- Photos without GPS are excluded from the map
- Shared date range filter and summary bar apply to the map page
- Clicking a marker opens the Side Panel

---

## Stage 8 — Sync Status and Setup Management
Operational UI for sync progress, connection health, and post-setup reconfiguration.

- Sync Status page shows OneDrive connection state and indexing progress (`N / M`)
- Sync Status updates via HTMX polling
- Auth failures surface a re-authenticate prompt
- Network failures surface a connection error without blocking local browsing
- Setup page remains available after first run for re-authentication and folder changes

---

## Stage 9 — Face Pipeline Thumbnails
Additional thumbnail and crop preparation dedicated to the face pipeline.

- Generate any extra thumbnail/crop artifacts needed by downstream face processing
- Keep original Photos ephemeral during processing
- Do not change the MVP thumbnail behavior already delivered in Stage 5

---

## Stage 10 — Face Detection
FaceONNX integration — detect and align faces per photo.

- Integrate FaceONNX + ONNX Runtime
- During indexing: download photo, run face detection, store bounding boxes + aligned face crops
- Delete photo after processing
- No recognition yet — detection only

---

## Stage 11 — Face Embeddings + Clustering
Generate embedding vectors and group similar faces.

- Generate embedding vector per detected face (FaceONNX)
- Cluster embeddings across all photos (similarity threshold)
- Store clusters in DB — each cluster is a candidate "person"

---

## Stage 12 — Manual Labeling UI
Let the user name face clusters.

- UI to browse face clusters (grid of face crops per cluster)
- Assign a name/label to a cluster
- Merge clusters manually
- Split a cluster if incorrectly grouped

---

## Stage 13 — Unified Search
Query across time + location + people simultaneously.

- Search UI: date range + map bounding box + person filter (any combination)
- Single query against PostgreSQL (PostGIS for location, indexed timestamps, person labels)
- Results displayed in unified timeline + map view
