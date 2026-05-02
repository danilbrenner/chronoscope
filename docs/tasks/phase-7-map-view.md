# Phase 7 — Map View

## Goal
Provide map exploration for Photos with GPS coordinates within the active date range.

## Dependencies
- Requires: [Phase 6](./phase-6-timeline-view.md)

## References
- [SPEC §5 Map View](../spec.md#map-view-map)
- [SPEC §5 Side Panel](../spec.md#side-panel)
- [SPEC §5 Date Range Filter](../spec.md#date-range-filter)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)

## Tasks

- [ ] Task 7.1 — Create map query use case for GPS photos
- [ ] Task 7.2 — Implement `/map` page with Leaflet integration
- [ ] Task 7.3 — Implement pin click to open Side Panel

### Task 7.1 — Create map query use case for GPS photos
**Depends on:** Phase 6 complete  
**Acceptance criteria:**
- Query returns only Photos with coordinates in selected date range.
- Returned payload includes marker location and panel metadata.
- Photos without GPS are excluded from map results.

### Task 7.2 — Implement `/map` page with Leaflet integration
**Depends on:** Task 7.1  
**Acceptance criteria:**
- Map page renders Leaflet map and marker data from server response.
- Marker rendering works for current filter bounds.
- Page remains server-driven except required map JavaScript.

### Task 7.3 — Implement pin click to open Side Panel
**Depends on:** Task 7.2  
**Acceptance criteria:**
- Clicking a pin opens a panel with thumbnail, filename, date/time, coordinates, and source.
- Panel content is sourced from strongly typed view model.
- Missing optional fields are rendered gracefully.
