# Phase 4 — OneDrive Sync

## Goal
Add folder configuration and incremental delta sync that stores discovered Photo metadata in the Index.

## Dependencies
- Requires: [Phase 3](./phase-3-onedrive-auth.md)

## References
- [SPEC §2 First-Run Flow](../spec.md#2-first-run-flow)
- [SPEC §3 Sync](../spec.md#3-sync)
- [SPEC §4 Domain Model](../spec.md#4-domain-model)
- [ARCHITECTURE — Indexing Pipeline](../architecture.md#indexing-pipeline-background-worker)
- [ARCHITECTURE — Data Flow](../architecture.md#data-flow)

## Tasks

- [ ] Task 4.1 — Add Source configuration persistence
- [ ] Task 4.2 — Implement folder path verify/save flow in Setup
- [ ] Task 4.3 — Implement background delta sync worker
- [ ] Task 4.4 — Persist discovered Photo metadata

### Task 4.1 — Add Source configuration persistence
**Depends on:** Phase 3 complete  
**Acceptance criteria:**
- Source folder path and delta token are persisted in the Index.
- One active OneDrive Source configuration is supported for MVP.
- Source terminology is used consistently in code and docs.

### Task 4.2 — Implement folder path verify/save flow in Setup
**Depends on:** Task 4.1  
**Acceptance criteria:**
- User can enter a folder path and run Graph-based verification.
- Invalid path returns inline validation error; value is not saved.
- Valid path is saved and can be edited post-setup.

### Task 4.3 — Implement background delta sync worker
**Depends on:** Task 4.1  
**Acceptance criteria:**
- Worker executes delta query loop on configured schedule.
- New/changed/deleted OneDrive items are reconciled in local Index.
- Worker runs as hosted background service in single deployable app.

### Task 4.4 — Persist discovered Photo metadata
**Depends on:** Task 4.3  
**Acceptance criteria:**
- Metadata fields required for MVP discovery are stored (`sourceId`, filename, modified date, size).
- Processing Status is initialized as `Discovered` for newly synced photos.
- No image download or EXIF extraction is done in this phase.
