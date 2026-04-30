# Chronoscope

A local-first photo indexing app that connects to a cloud photo archive and builds a searchable index by time, location, and people. Photos are processed locally; only metadata, thumbnails, and embeddings are stored.

## Language

**Photo**:
A single image file discovered from a Source, identified internally by `internalId` and externally by `sourceId`. Uniqueness is enforced on `(Source, sourceId)`.
_Avoid_: image, file, picture

**Source**:
A first-class entity representing a connected photo archive (e.g. OneDrive). Owns the folder configuration, delta sync token, and auth state for that connection.
_Avoid_: provider, account, integration

**Processing Status**:
The lifecycle state of a Photo through the indexing pipeline. Values: `Discovered` (metadata only, not yet processed), `Processed` (EXIF + thumbnail extracted), `Failed` (processing error — terminal for MVP, no retry).
_Avoid_: state, stage, status

**Sync**:
The background process that reconciles a Source with the local index — discovering new, changed, and deleted Photos via delta queries.
_Avoid_: import, fetch, crawl

**Index**:
The local PostgreSQL database that stores all extracted metadata, thumbnails, and embeddings. The Source is the source of truth for photos; the Index is derived.
_Avoid_: database, store, cache

**Thumbnail**:
A resized image derived from a Photo, stored as binary in the Index. Generated at 400px on the long edge. Used for all UI rendering — scaled down via CSS for table rows, displayed at generated size in the Side Panel. The original photo is never persisted locally.
_Avoid_: preview, small image

## Relationships

- A **Source** has many **Photos**
- A **Photo** belongs to exactly one **Source**
- A **Photo** has one `internalId` (system-assigned) and one `sourceId` (assigned by the Source); uniqueness is `(Source, sourceId)`
- A **Sync** runs against one **Source**
- A **Photo** may have zero or one **GPS coordinates** — those without appear in Timeline View only, marked as "no coordinates"

## UI Concepts

**Timeline View**: Table-based view of Photos ordered by date, with small inline thumbnails. Scoped to the active date range filter. Photos without GPS are included but marked.

**Map View**: Leaflet map with pins at GPS coordinates. Scoped to the same date range filter. Photos without GPS are excluded from the map.

**Date Range Filter**: Shared across both views. Defaults to the last 30 days on first load.

**Side Panel**: Appears when a Photo is selected (pin click or table row click). Shows: thumbnail, filename, date/time, GPS coordinates (if any), Source.

**Summary Bar**: Displayed in both views. Shows count of Photos with GPS and without GPS for the active date range.

**Sync Status Page** (`/sync/status`): Dedicated page showing OneDrive connection status and indexing progress (photos indexed count). Updates via HTMX polling. Surfaces auth errors with a re-authenticate prompt. UI remains fully browsable when sync is stopped due to auth/network errors.

**Navigation**: `Timeline | Map | Sync Status | Setup` with a bell icon (🔔) top-right for future notifications. Bell is present in MVP but always shows 0 — notification content is post-MVP.

## Flagged ambiguities

_(none yet)_
