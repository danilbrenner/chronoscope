# Phase 7 — Map View

## Goal
Deliver the map explorer for Photos with coordinates while preserving the same server-driven filtering and detail patterns as the timeline.

## Dependencies
- Requires: [Phase 6](./phase-6-timeline-view.md)

## References
- [SPEC §5 UI](../spec.md#5-ui)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)

## Tasks

- [ ] Task 7.1 — Render the Leaflet map page
- [ ] Task 7.2 — Filter map data by date range and coordinates
- [ ] Task 7.3 — Add marker-driven side-panel details

### Task 7.1 — Render the Leaflet map page
**Depends on:** Phase 6 complete  
**Acceptance criteria:**
- `/map` renders a Leaflet-based map page.
- The page uses strongly typed view models for full-page rendering.
- The layout and navigation remain consistent with the rest of the app.

### Task 7.2 — Filter map data by date range and coordinates
**Depends on:** Task 7.1  
**Acceptance criteria:**
- The active date range filters the Photos represented on the map.
- Only Photos with GPS coordinates are included on the map.
- The page shows the same summary bar semantics as the timeline for the active date range.

### Task 7.3 — Add marker-driven side-panel details
**Depends on:** Task 7.2  
**Acceptance criteria:**
- Clicking a marker opens the Photo side panel.
- The side panel shows thumbnail, filename, date/time, GPS coordinates, and Source name.
- Marker interactions use server-rendered HTML for details rather than JSON responses.
