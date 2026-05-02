# Phase 3 — OneDrive Auth

## Goal
Implement Device Code authentication with token persistence and setup UX for first-run and re-auth flows.

## Dependencies
- Requires: [Phase 2](./phase-2-aspnet-mvc-shell.md)

## References
- [SPEC §2 First-Run Flow](../spec.md#2-first-run-flow)
- [SPEC §5 Setup Page](../spec.md#setup-page-setup)
- [SPEC §6 Error Handling](../spec.md#6-error-handling)
- [ARCHITECTURE — OneDrive Connector](../architecture.md#onedrive-connector)

## Tasks

- [ ] Task 3.1 — Define auth abstractions in Application layer
- [ ] Task 3.2 — Implement MSAL Device Code flow in Infrastructure
- [ ] Task 3.3 — Build Setup page auth step
- [ ] Task 3.4 — Add Graph connectivity verification

### Task 3.1 — Define auth abstractions in Application layer
**Depends on:** Phase 2 complete  
**Acceptance criteria:**
- Auth service interfaces are defined under `Application/Abstractions/`.
- Contracts cover start-auth flow, complete-auth state, and auth status retrieval.
- No interface contracts are introduced outside `Application`.

### Task 3.2 — Implement MSAL Device Code flow in Infrastructure
**Depends on:** Task 3.1  
**Acceptance criteria:**
- Device code flow returns code + verification URL for UI display.
- Access/refresh token cache persists across restarts.
- Silent re-auth is attempted before interactive re-auth.

### Task 3.3 — Build Setup page auth step
**Depends on:** Task 3.2  
**Acceptance criteria:**
- Setup page presents auth step and current connection state.
- User can trigger device code flow and complete sign-in.
- HTMX/full-page behavior follows server-rendered pattern.

### Task 3.4 — Add Graph connectivity verification
**Depends on:** Task 3.2  
**Acceptance criteria:**
- After auth, app verifies Graph API access (e.g., profile call).
- Auth/connectivity failures are surfaced on Sync Status/Setup UX.
- Error behavior matches MVP expectations (UI remains browsable).
