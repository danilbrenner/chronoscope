# Thumbnails Stored as Binary in PostgreSQL

Thumbnails are stored as `bytea` in PostgreSQL rather than on local disk. At ~15KB per thumbnail, 200k photos produces ~3GB — acceptable for the target scale. The single-DB backup story outweighs the bloat concern for MVP. Revisit if the archive grows significantly beyond 200k photos.

## Considered Options

- **Local disk** — no DB bloat, fast static serving, but thumbnails aren't covered by DB backup and are a second artifact to manage
- **OneDrive thumbnail API** — zero local storage, but requires network on every page render; conflicts with local-first snappiness
