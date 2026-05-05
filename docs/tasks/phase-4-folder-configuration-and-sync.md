# Phase 4 — Folder Configuration and Incremental Sync

## Goal
Let the user choose the OneDrive folder to index and establish incremental Photo discovery through Graph delta queries.

## Dependencies
- Requires: [Phase 3](./phase-3-onedrive-connection-setup.md)

## References
- [SPEC §2 First-Run Flow](../spec.md#2-first-run-flow)
- [SPEC §3 Sync](../spec.md#3-sync)
- [SPEC §4 Domain Model](../spec.md#4-domain-model)
- [ARCHITECTURE — OneDrive Connector](../architecture.md#onedrive-connector)

## Tasks

- [ ] Task 4.1 — Add folder verification and persistence
- [ ] Task 4.2 — Persist Source sync state
- [ ] Task 4.3 — Add incremental discovery worker

### Task 4.1 — Add folder verification and persistence
**Depends on:** Phase 3 complete  
**Acceptance criteria:**
- Setup collects a single typed OneDrive folder path.
- A "Verify" action checks the folder through Graph API before saving.
- Invalid folder paths produce inline validation and are not persisted.
- Successful verification stores the folder path for the configured Source.

### Task 4.2 — Persist Source sync state
**Depends on:** Task 4.1  
**Acceptance criteria:**
- Source persistence includes source type, folder path, and delta token.
- Photo persistence includes `sourceId`, filename, modified date, and size from Graph metadata.
- Discovery state supports uniqueness by Source + source-assigned Photo ID.
- Initial discovery writes Photos in a state ready for processing in the next phase.

### Task 4.3 — Add incremental discovery worker
**Depends on:** Task 4.2  
**Acceptance criteria:**
- Background sync runs as an `IHostedService`.
- Graph delta queries are used to discover new, changed, and deleted Photos.
- Deleted items are reflected in the Index.
- Successful setup starts sync and redirects the user to the Sync Status page.
