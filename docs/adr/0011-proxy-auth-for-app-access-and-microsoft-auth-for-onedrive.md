# Proxy auth for app access and Microsoft auth for OneDrive

Chronoscope separates two authentication concerns: a reverse proxy controls who can open the app, while `Microsoft.Identity.Web` is used only to obtain delegated Microsoft Graph access for the configured OneDrive source. This supersedes the broader interpretation in ADR-0009 and ADR-0010 that blurred app access auth with OneDrive source authorization.
