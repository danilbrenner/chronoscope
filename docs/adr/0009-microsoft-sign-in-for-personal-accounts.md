# Microsoft sign-in for personal Microsoft accounts

Chronoscope uses Microsoft sign-in via `Microsoft.Identity.Web` with an Azure web app registration scoped to personal Microsoft accounts only. This matches the app's OneDrive-first setup flow and avoids the mismatch of relying on reverse-proxy auth for a browser-hosted product that still needs delegated Graph access.
