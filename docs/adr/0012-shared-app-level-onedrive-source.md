# ADR 0012: Shared App-Level OneDrive Source

## Status
Accepted

## Context
Chronoscope app access is controlled by reverse-proxy authentication, while OneDrive access is required only for Source-backed operations (setup, sync, source status).

Earlier wording left room for a per-HTTP-user token model (`HttpContext`-bound token lookup). That model is fragile for background sync and does not match the desired product behavior where one Source link powers the app.

## Decision
- Chronoscope uses one app-level OneDrive Source link per app instance.
- The Source link is shared by authorized app users.
- Microsoft Graph access and refresh tokens are stored in the Index (PostgreSQL), encrypted at rest.
- Graph token retrieval and refresh are server-managed and must not depend on `HttpContext`, auth cookies, or browser session state.
- Any authorized app user can execute Source-backed operations against the shared Source.

## Consequences
### Positive
- Background workers and non-HTTP flows can refresh and use Graph tokens safely.
- Source behavior is consistent across authorized app users.
- Security boundary is clearer: app authorization controls feature access; Source identity controls which OneDrive account is linked.

### Trade-offs
- Shared Source means operational impact if that OneDrive link is revoked or rotated.
- Token encryption key management becomes a critical operational concern.

## Implementation Notes
- Keep controllers thin and delegate Source operations to application services.
- Application abstractions define Source/token interfaces; Data and Infrastructure provide implementations.
- Never log token plaintext.

## Related ADRs
- `0011-proxy-auth-for-app-access-and-microsoft-auth-for-onedrive.md`
- `0010-microsoft-sign-in-for-azure-ad-and-personal-accounts.md`
- `0008-web-auth-with-persistent-encrypted-token-cache.md`

