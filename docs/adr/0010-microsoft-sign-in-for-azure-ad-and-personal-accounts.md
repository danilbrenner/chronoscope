# Microsoft sign-in for Azure AD and personal Microsoft accounts

Chronoscope uses `Microsoft.Identity.Web` with an Azure web app registration that supports both Azure AD and personal Microsoft accounts. That keeps the browser-based setup flow aligned with delegated Graph access while broadening the app beyond the earlier personal-only constraint recorded in ADR-0009.
