# Phase 8 — Unified View

## Goal
Link Timeline and Map into a coordinated explorer where shared filters and selection synchronize both surfaces.

## Dependencies
- Requires: [Phase 7](./phase-7-map-view.md)

## References
- [SPEC §5 Date Range Filter](../spec.md#date-range-filter)
- [SPEC §5 Timeline View](../spec.md#timeline-view-)
- [SPEC §5 Map View](../spec.md#map-view-map)
- [ARCHITECTURE — Explorer (Unified View)](../architecture.md#explorer-unified-view)
- [ARCHITECTURE — HTMX interactivity](../architecture.md#web-ui)

## Tasks

- [ ] Task 8.1 — Build unified explorer page composition
- [ ] Task 8.2 — Wire date-range filter to update timeline and map together
- [ ] Task 8.3 — Synchronize selection between map pins and timeline rows
- [ ] Task 8.4 — Finalize zero-result and mixed-location UX

### Task 8.1 — Build unified explorer page composition
**Depends on:** Phase 7 complete  
**Acceptance criteria:**
- A single page hosts timeline region and map region.
- Shared filter state is represented consistently for both regions.
- Full-page load renders both regions with initial range defaults.

### Task 8.2 — Wire date-range filter to update timeline and map together
**Depends on:** Task 8.1  
**Acceptance criteria:**
- Date-range change refreshes both timeline rows and map pins.
- Updates use HTMX partial patterns and preserve UX responsiveness.
- Summary bar values stay consistent with active range.

### Task 8.3 — Synchronize selection between map pins and timeline rows
**Depends on:** Task 8.2  
**Acceptance criteria:**
- Selecting a map pin highlights or focuses corresponding timeline entry.
- Selecting a timeline row opens matching side panel context.
- Behavior is deterministic when multiple entries share nearby coordinates.

### Task 8.4 — Finalize zero-result and mixed-location UX
**Depends on:** Task 8.2  
**Acceptance criteria:**
- When range has no Photos, unified view shows clear empty state.
- When range has only non-GPS Photos, timeline shows results and map shows no pins gracefully.
- Summary text correctly reports with-location vs without-location counts.
