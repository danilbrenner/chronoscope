# Centralized Build and Package Management with Directory Props

Chronoscope uses `Directory.Build.props` and `Directory.Packages.props` at the repository root to centralize shared MSBuild settings and NuGet package versions across the solution. This keeps project files smaller, enforces consistent dependency versions, and reduces drift between projects as the solution grows.
