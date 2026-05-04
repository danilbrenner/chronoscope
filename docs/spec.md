# Chronoscope — MVP Specification

> **Scope**: Stages 1–8. Everything from Stage 9 (thumbnails for face pipeline, face detection, recognition, labeling) is post-MVP.

---

## 1. Goals

Chronoscope MVP delivers a single deployable that:

1. Connects to a user's OneDrive and incrementally syncs photo metadata
2. Extracts EXIF timestamps and GPS coordinates from each photo
3. Generates and stores a thumbnail for each photo
4. Presents photos in a switchable Timeline / Map explorer, filterable by date range

---

## 2. First-Run Flow

A new user opens the browser and is presented with a **Setup page**. The setup flow is sequential:

1. **Authenticate** — Redirect user through Microsoft login (Authorization Code Flow via `Microsoft.Identity.Web`). Tokens are acquired and refreshed silently after initial sign-in.
2. **Select folder** — User types an OneDrive folder path (e.g. `/Photos/Vacations`). A "Verify" button confirms the folder exists via Graph API before saving.
3. **Sync starts** — Background worker begins immediately. User is redirected to the **Sync Status page**.
4. **Explore** — Once photos are indexed, user navigates to Timeline or Map.

After initial setup, the Setup page remains accessible from the nav for re-authentication or folder path changes.

### Auth and Token Persistence Requirements

- Azure app registration is a **Web** app scoped to **Personal Microsoft accounts only**
- Delegated permissions: `Files.Read`, `offline_access`, `User.Read`
- Token cache is persisted in PostgreSQL using a distributed cache (survives app/container restarts)
- Token cache encryption is enabled
- ASP.NET Core Data Protection keys are persisted to a Docker-mounted filesystem volume

---

## 3. Sync

### Schedule

Sync frequency is configured via a cron expression in `appsettings.json` (or environment variable). It is not configurable from the UI.

### Mechanism

- Uses Microsoft Graph API **delta queries** to discover new, changed, and deleted Photos without full re-scans.
- Runs as a background `IHostedService`.
- Each Photo goes through the indexing pipeline after being discovered.

### Indexing Pipeline (per Photo)

1. Fetch metadata from Graph API (filename, `sourceId`, modified date, size)
2. Download photo to a temp path
3. Extract EXIF: timestamp, GPS coordinates
4. Generate Thumbnail (400px long edge, stored as binary in DB)
5. Delete local copy
6. Persist all data to PostgreSQL — Processing Status set to `Processed`

If any step fails, Processing Status is set to `Failed`. Failed photos are not retried in MVP.

### Folder Selection

Single folder, typed as a path. Tree picker and multiple folder selection are post-MVP enhancements.

---

## 4. Domain Model

### Photo

| Field | Description |
|---|---|
| `internalId` | System-assigned UUID. Primary key. |
| `sourceId` | ID assigned by the Source (OneDrive item ID). |
| `source` | The Source this Photo belongs to. Uniqueness: `(source, sourceId)`. |
| `filename` | Original filename. |
| `takenAt` | Timestamp from EXIF, or OneDrive modified date as fallback. |
| `gpsCoordinates` | PostGIS point. Null if EXIF has no GPS data. |
| `thumbnail` | Binary blob, 400px long edge. Null until Processed. |
| `processingStatus` | `Discovered` → `Processed` or `Failed`. |
| `sizeBytes` | File size from OneDrive metadata. |

### Source

| Field | Description |
|---|---|
| `id` | System-assigned UUID. |
| `type` | Source type (e.g. `"onedrive"`). |
| `folderPath` | Configured OneDrive folder path. |
| `deltaToken` | Last delta query token for incremental sync. |
| `authState` | MSAL token cache reference. |

### Processing Status

| Value | Meaning |
|---|---|
| `Discovered` | Metadata fetched, not yet processed. |
| `Processed` | EXIF extracted, thumbnail generated. |
| `Failed` | Processing error. Terminal in MVP — no retry. |

---

## 5. UI

### Navigation

```
[ Timeline ]  [ Map ]  [ Sync Status ]  [ Setup ]          🔔
```

Bell icon (🔔) is present in MVP but always shows 0. Notification content is post-MVP (planned: sync session success/failure alerts, similar to Azure Portal notifications).

### Date Range Filter

Shared across Timeline and Map views. Defaults to **last 30 days** on load. When the range returns zero photos, the UI displays a clear empty state (not a blank page).

### Summary Bar

Displayed in both views within the active date range:

> `142 photos with location · 38 without location`

### Timeline View (`/`)

- Table of Photos ordered by date (newest first)
- Each row: small thumbnail (CSS-scaled from 400px), filename, date/time, GPS indicator
- Photos without GPS included, marked "no coordinates"
- **Load more** button appends next page of rows (HTMX partial update)
- Clicking a row opens the Side Panel

### Map View (`/map`)

- Leaflet map with pins at GPS coordinates of Photos in the active date range
- Photos without GPS are excluded from the map
- Clicking a pin opens the Side Panel

### Side Panel

Appears on photo selection (row click or pin click). Contains:

- Thumbnail (400px, full generated size)
- Filename
- Date/time
- GPS coordinates (if available)
- Source name

### Sync Status Page (`/sync/status`)

- OneDrive connection status (connected / auth error / unreachable)
- Indexing progress: `N / M photos indexed`
- Updates via HTMX polling (every ~3 seconds)
- When auth has expired: surfaces a "Re-authenticate" prompt
- The rest of the UI (Timeline, Map) remains fully functional when sync is stopped — the Index is local and read-only access is unaffected

### Setup Page (`/setup`)

- Step 1: Microsoft sign-in via browser redirect (Authorization Code Flow)
- Step 2: Folder path input + "Verify" button
- Accessible post-setup for re-authentication or folder reconfiguration

---

## 6. Error Handling

| Scenario | Behaviour |
|---|---|
| Auth token expires / refresh fails | Sync stops. UI remains browsable. Sync Status page shows auth error + re-authenticate prompt. |
| OneDrive unreachable (network) | Sync stops. UI remains browsable. Sync Status page shows connection error. |
| Photo EXIF extraction fails | Photo marked `Failed`. Logged. No retry in MVP. |
| Folder path not found on verify | Inline validation error on Setup page. Folder not saved. |

---

## 7. Out of Scope (Post-MVP)

- Face detection, embeddings, clustering, labeling (Stages 10–12)
- Unified search across people (Stage 13)
- Thumbnail generation as part of face pipeline (Stage 9)
- Multiple OneDrive folders
- Folder tree picker UI
- Sync schedule configurable from UI
- Notification bell content (sync session results)
- Full photo display (original fetched from OneDrive on demand)
- Retry of `Failed` photos
