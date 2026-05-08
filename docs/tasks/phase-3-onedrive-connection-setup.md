# Phase 3 — OneDrive Connection Setup

## Goal
Establish a web-appropriate OneDrive authorization flow with durable token protection and a first-run setup entry point behind proxy-authenticated app access.

## Dependencies
- Requires: [Phase 2](./phase-2-aspnet-mvc-shell.md)

## References
- [SPEC §2 First-Run Flow](../spec.md#2-first-run-flow)
- [SPEC §5 UI](../spec.md#5-ui)
- [ARCHITECTURE — OneDrive Connector](../architecture.md#onedrive-connector)
- [ARCHITECTURE — Web UI](../architecture.md#web-ui)

## Tasks

- [x] Task 3.1 — Integrate web-based Microsoft sign-in
- [x] Task 3.2 — Persist and protect the OneDrive token cache
- [ ] Task 3.3 — Add the setup auth step

### Task 3.1 — Integrate web-based Microsoft sign-in
**Depends on:** Phase 2 complete  
**Acceptance criteria:**
- OneDrive auth uses Authorization Code Flow via `Microsoft.Identity.Web`.
- App access remains external to Chronoscope and is not replaced by Microsoft sign-in.
- The Azure app registration assumptions are documented in configuration shape: **Web** app, Azure AD and personal Microsoft accounts.
- Requested delegated permissions are `Files.Read`, `offline_access`, and `User.Read`.
- The app can complete sign-in and call Microsoft Graph on behalf of the user.

### Task 3.2 — Persist and protect the OneDrive token cache
**Depends on:** Task 3.1  
**Acceptance criteria:**
- Token cache is stored in PostgreSQL through a distributed cache.
- Token cache encryption is enabled.
- ASP.NET Core Data Protection keys persist to a filesystem path suitable for a Docker-mounted volume.
- Sign-in survives app restarts without requiring interactive auth on every launch.

### Task 3.3 — Add the setup auth step
**Depends on:** Task 3.1  
**Acceptance criteria:**
- `/setup` exposes a OneDrive sign-in step that redirects the user to Microsoft login instead of showing a device code.
- Setup uses controller + strongly typed view model + Razor view/partial pattern.
- Auth success and auth error states are represented in the setup UI.
- The setup flow is ready for the folder-selection step in the next phase.
