# Phase 2 — ASP.NET MVC Shell

## Goal
Deliver a minimal, working web shell with shared layout and navigation, ready for setup/sync/explorer pages in later phases.

## Dependencies
- Requires: [Phase 1](./phase-1-solution-scaffold.md)

## References
- [SPEC §5 UI](../spec.md#5-ui)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)
- [ARCHITECTURE — Layer Responsibilities](../architecture.md#layer-responsibilities)

## Tasks

- [x] Task 2.1 — Bootstrap MVC host and routing
- [x] Task 2.2 — Add shared layout with top navigation skeleton
- [x] Task 2.3 — Integrate Pico CSS v2
- [x] Task 2.4 — Add placeholder home/index page

### Task 2.1 — Bootstrap MVC host and routing
**Depends on:** Phase 1 complete  
**Acceptance criteria:**
- MVC pipeline is configured in `Host`/`Web`.
- Root route resolves to a working page.
- App renders server-side Razor views.

### Task 2.2 — Add shared layout with top navigation skeleton
**Depends on:** Task 2.1  
**Acceptance criteria:**
- Shared layout includes nav entries for Timeline, Map, Sync Status, and Setup.
- Bell icon placeholder is visible (no functional notifications yet).
- Navigation is consistent across full-page views.

### Task 2.3 — Integrate Pico CSS v2
**Depends on:** Task 2.2  
**Acceptance criteria:**
- Pico CSS is loaded globally by layout.
- Base typography, spacing, and controls are readable without custom framework JS.
- Styling approach remains server-rendered and lightweight.

### Task 2.4 — Add placeholder home/index page
**Depends on:** Task 2.2  
**Acceptance criteria:**
- Home page renders through controller + strongly typed view model.
- Page establishes baseline pattern for future view models and Razor pages.
- Controller remains thin (no business logic).
