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

## Stage 3 — OneDrive Auth
MSAL Device Code Flow with token persistence and a setup UI.

- Integrate MSAL .NET for Device Code Flow
- Settings/setup page: display device code + verification URL to the user
- Persist token cache so app re-authenticates silently on restart
- Verify Graph API connectivity (e.g., fetch user profile)

---

## Stage 4 — OneDrive Sync
Folder selection and incremental delta sync loop.

- UI for user to specify the OneDrive folder to sync
- Background worker polls using Graph API **delta queries**
- New/changed/deleted items reflected in the DB
- Photo metadata stored (filename, OneDrive item ID, modified date, size)
- No image download yet — metadata only

---

## Stage 5 — EXIF Extraction
Download, extract, persist, delete.

- For each synced photo: download to temp path
- Extract **timestamp** and **GPS coordinates** from EXIF
- Persist to DB (update photo record with EXIF fields)
- Delete temp file immediately after extraction
- Handle photos with missing EXIF gracefully (null GPS, fallback timestamp)

---

## Stage 6 — Timeline View
Chronological photo browser.

- List view of photos ordered by timestamp
- HTMX pagination (load more / page navigation)
- Display thumbnail (placeholder until Stage 9) or filename
- Filter by date range

---

## Stage 7 — Map View
Leaflet map with GPS-located photos.

- Leaflet map rendered server-side (tile layer + markers)
- Markers placed at GPS coordinates of indexed photos
- Clicking a marker shows photo info (filename, date)
- Photos without GPS are excluded from the map

---

## Stage 8 — Unified View
Timeline and map linked via HTMX.

- Single page hosts both timeline list and Leaflet map
- Selecting a date range in the timeline filters map pins (HTMX partial update)
- Clicking a map cluster/pin highlights corresponding timeline entries
- Photos without GPS appear only in the timeline

---

## Stage 9 — Thumbnail Generation
Local thumbnails stored in DB for fast UI rendering.

- During indexing: download photo, resize to thumbnail dimensions, store in DB as binary or file ref
- Delete original after thumbnail is generated
- Timeline and map views render thumbnails
- On-demand full photo display fetches original from OneDrive at view time

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
