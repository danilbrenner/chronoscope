# Serilog Structured Logging Outside Development

Chronoscope uses Serilog for structured logging in non-development environments so logs are consistent, machine-readable, and easier to aggregate/query during operations. In Development, the app keeps standard ASP.NET Core console logging to preserve a simple local debugging experience with minimal setup friction.
