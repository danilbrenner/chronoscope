# Phase 5 — Photo Processing Pipeline

## Goal
Process each discovered Photo end-to-end: temporary download, EXIF extraction, thumbnail generation, cleanup, and final Processing Status.

## Dependencies
- Requires: [Phase 4](./phase-4-folder-configuration-and-sync.md)

## References
- [SPEC §3 Sync](../spec.md#3-sync)
- [SPEC §4 Domain Model](../spec.md#4-domain-model)
- [SPEC §6 Error Handling](../spec.md#6-error-handling)
- [ARCHITECTURE — Indexing Pipeline (Background Worker)](../architecture.md#indexing-pipeline-background-worker)

## Tasks

- [ ] Task 5.1 — Download Photos to temporary storage for processing
- [ ] Task 5.2 — Extract EXIF metadata with fallback timestamp handling
- [ ] Task 5.3 — Generate thumbnails and finalize Processing Status

### Task 5.1 — Download Photos to temporary storage for processing
**Depends on:** Phase 4 complete  
**Acceptance criteria:**
- Processing downloads each discovered Photo to a temp path only for the duration of processing.
- Temporary file management is isolated from permanent storage.
- Temp files are deleted after processing completes or fails.

### Task 5.2 — Extract EXIF metadata with fallback timestamp handling
**Depends on:** Task 5.1  
**Acceptance criteria:**
- EXIF extraction reads timestamp and GPS coordinates when present.
- Missing GPS data results in a null GPS value without failing processing.
- Missing EXIF timestamp falls back to the OneDrive modified date.

### Task 5.3 — Generate thumbnails and finalize Processing Status
**Depends on:** Task 5.2  
**Acceptance criteria:**
- Processing generates a 400px long-edge thumbnail and stores it in the DB.
- Successfully processed Photos are marked `Processed`.
- Processing failures mark the Photo `Failed`.
- Failed Photos are not retried in MVP.
