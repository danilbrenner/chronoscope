# Web auth with persistent encrypted token cache

Chronoscope uses Microsoft Graph through Authorization Code Flow via `Microsoft.Identity.Web` instead of Device Code Flow because the app is browser-hosted and should use a normal web sign-in experience. OneDrive tokens are persisted in PostgreSQL through a distributed token cache with encryption enabled, while ASP.NET Core Data Protection keys live in a filesystem volume so the encrypted cache survives restarts without storing both ciphertext and master keys in the same place.
