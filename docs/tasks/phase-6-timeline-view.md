# Phase 6 — Timeline View

## Goal
Deliver timeline browsing of indexed Photos with date-range filtering, pagination, and empty-state handling.

## Dependencies
- Requires: [Phase 5](./phase-5-exif-extraction.md)

## References
- [SPEC §5 Timeline View](../spec.md#timeline-view-)
- [SPEC §5 Date Range Filter](../spec.md#date-range-filter)
- [SPEC §5 Summary Bar](../spec.md#summary-bar)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)

## Tasks

- [ ] Task 6.1 — Create timeline query use case and DTOs
- [ ] Task 6.2 — Build timeline page and row partials
- [ ] Task 6.3 — Add HTMX pagination and date-range filtering
- [ ] Task 6.4 — Add timeline empty state behavior

### Task 6.1 — Create timeline query use case and DTOs
**Depends on:** Phase 5 complete  
**Acceptance criteria:**
- Application-layer use case returns timeline rows ordered by newest first.
- Date-range input is validated and applied to query.
- Response includes fields required by timeline row and summary bar.

### Task 6.2 — Build timeline page and row partials
**Depends on:** Task 6.1  
**Acceptance criteria:**
- Timeline full page renders table/list of Photos.
- Row rendering uses strongly typed view models (no domain entity directly in view).
- Photos without GPS are included and clearly marked.

### Task 6.3 — Add HTMX pagination and date-range filtering
**Depends on:** Task 6.2  
**Acceptance criteria:**
- "Load more" appends additional rows via partial response.
- Date-range updates refresh list and summary without full-page reload where applicable.
- HTMX requests return partial views; direct navigation returns full page.

### Task 6.4 — Add timeline empty state behavior
**Depends on:** Task 6.3  
**Acceptance criteria:**
- No-results range renders clear empty state message.
- Empty state is not a blank page and preserves filtering controls.
