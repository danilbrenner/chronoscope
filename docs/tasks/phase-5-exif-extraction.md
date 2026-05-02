# Phase 5 — EXIF Extraction

## Goal
Process discovered Photos by downloading temporarily, extracting EXIF timestamp/GPS, persisting results, and deleting local copies.

## Dependencies
- Requires: [Phase 4](./phase-4-onedrive-sync.md)

## References
- [SPEC §3 Sync](../spec.md#3-sync)
- [SPEC §4 Domain Model](../spec.md#4-domain-model)
- [SPEC §6 Error Handling](../spec.md#6-error-handling)
- [ARCHITECTURE — Indexing Pipeline](../architecture.md#indexing-pipeline-background-worker)

## Tasks

- [ ] Task 5.1 — Add EXIF extraction abstraction and implementation
- [ ] Task 5.2 — Implement temp-file download + cleanup flow
- [ ] Task 5.3 — Persist extracted metadata to Photo
- [ ] Task 5.4 — Update Processing Status transitions and failure behavior

### Task 5.1 — Add EXIF extraction abstraction and implementation
**Depends on:** Phase 4 complete  
**Acceptance criteria:**
- EXIF reader contract is defined in `Application/Abstractions/`.
- Infrastructure implementation extracts timestamp and GPS coordinates.
- Missing EXIF values are represented explicitly (null GPS, timestamp fallback path).

### Task 5.2 — Implement temp-file download + cleanup flow
**Depends on:** Task 5.1  
**Acceptance criteria:**
- Photo is downloaded to temp storage for processing.
- Temp file is deleted after processing attempt (success or failure path).
- No original photo is persisted long-term in local storage.

### Task 5.3 — Persist extracted metadata to Photo
**Depends on:** Task 5.1, Task 5.2  
**Acceptance criteria:**
- `takenAt` and `gpsCoordinates` fields are updated in Index records.
- Fallback timestamp behavior is applied when EXIF timestamp is missing.
- Geospatial value is stored in PostGIS-compatible shape.

### Task 5.4 — Update Processing Status transitions and failure behavior
**Depends on:** Task 5.3  
**Acceptance criteria:**
- Successful processing marks Photo as `Processed`.
- Extraction/processing failures mark Photo as `Failed`.
- Failed Photos are not retried in MVP.
