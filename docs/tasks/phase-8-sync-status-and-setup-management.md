# Phase 8 — Sync Status and Setup Management

## Goal
Finish MVP operational UX with sync progress, connection health, re-authentication prompts, and post-setup configuration access.

## Dependencies
- Requires: [Phase 7](./phase-7-map-view.md)

## References
- [SPEC §2 First-Run Flow](../spec.md#2-first-run-flow)
- [SPEC §5 UI](../spec.md#5-ui)
- [SPEC §6 Error Handling](../spec.md#6-error-handling)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)
- [ARCHITECTURE — OneDrive Connector](../architecture.md#onedrive-connector)

## Tasks

- [ ] Task 8.1 — Add sync status page with live updates
- [ ] Task 8.2 — Surface operational auth and connection errors
- [ ] Task 8.3 — Support post-setup re-entry and reconfiguration

### Task 8.1 — Add sync status page with live updates
**Depends on:** Phase 7 complete  
**Acceptance criteria:**
- `/sync/status` shows OneDrive connection status and indexing progress (`N / M photos indexed`).
- The page updates through HTMX polling.
- The page remains accessible whether sync is running or stopped.

### Task 8.2 — Surface operational auth and connection errors
**Depends on:** Task 8.1  
**Acceptance criteria:**
- Expired or failed auth surfaces a re-authenticate prompt.
- OneDrive network/connectivity problems surface a clear connection error.
- Timeline and Map remain browsable from the local Index when sync is stopped.

### Task 8.3 — Support post-setup re-entry and reconfiguration
**Depends on:** Task 8.2  
**Acceptance criteria:**
- The Setup page remains accessible from navigation after initial setup.
- Users can re-authenticate without recreating the rest of the app state.
- Users can change the configured folder path through the same setup surface.
