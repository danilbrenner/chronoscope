# Folder Selection via Typed Path, Not Tree Picker

The OneDrive folder to sync is configured by typing a path (e.g. `/Photos/Vacations`) with a "verify" button that confirms the folder exists via Graph API. A tree picker UI was considered but deferred — it requires recursive Graph API calls and a non-trivial UI component for no meaningful gain in a personal app where the user knows their own folder structure.

## Future enhancements

- Interactive folder tree picker
- Multiple folder selection
