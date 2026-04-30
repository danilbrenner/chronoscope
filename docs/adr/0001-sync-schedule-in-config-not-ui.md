# Sync Schedule Configured via Config File, Not UI

Sync frequency is set via a cron expression in `appsettings.json` (or environment variable), not through the web UI. The sync schedule is an operator concern — set once at deploy time — and exposing it in the UI adds complexity with no MVP value. It can be promoted to a UI setting post-MVP if needed.
