# Phase 6 — Timeline View

## Goal
Deliver the main chronological Photo explorer with filtering, summary information, pagination, and side-panel details.

## Dependencies
- Requires: [Phase 5](./phase-5-photo-processing-pipeline.md)

## References
- [SPEC §5 UI](../spec.md#5-ui)
- [SPEC §6 Error Handling](../spec.md#6-error-handling)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)

## Tasks

- [ ] Task 6.1 — Add date filtering and summary bar
- [ ] Task 6.2 — Render the timeline list
- [ ] Task 6.3 — Add HTMX pagination and side-panel details

### Task 6.1 — Add date filtering and summary bar
**Depends on:** Phase 5 complete  
**Acceptance criteria:**
- Timeline defaults to the last 30 days on first load.
- Date range filtering is server-driven.
- The page shows a summary bar with counts for Photos with and without GPS coordinates.
- Zero-result filters show a clear empty state.

### Task 6.2 — Render the timeline list
**Depends on:** Task 6.1  
**Acceptance criteria:**
- Timeline route renders Photos ordered by date descending.
- Each row shows thumbnail, filename, date/time, and a GPS/no-coordinates indicator.
- Strongly typed view models are used for the full page and row rendering.

### Task 6.3 — Add HTMX pagination and side-panel details
**Depends on:** Task 6.2  
**Acceptance criteria:**
- "Load more" appends additional rows via HTMX partial responses.
- Selecting a Photo opens a side panel with thumbnail, filename, date/time, GPS coordinates when present, and Source name.
- HTMX requests return partial Razor views while direct navigation returns full pages.
