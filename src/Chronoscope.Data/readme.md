# Chronoscope.Data

## EF Core migrations

Run commands from the repository root.

List migrations:

```bash
dotnet ef migrations list --project src/Chronoscope.Data --startup-project src/Chronoscope.Host
```

Add a new migration:

```bash
dotnet ef migrations add <MigrationName> --project src/Chronoscope.Data --startup-project src/Chronoscope.Host
```

Apply migrations with a connection string:

```bash
dotnet ef database update --project src/Chronoscope.Data --startup-project src/Chronoscope.Host --connection "Host=localhost;Port=5438;Database=chronoscope_dev;Username=postgres;Password=postgres"
```
