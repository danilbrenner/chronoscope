# Testing strategy and tooling baseline

Chronoscope standardizes on three test types: unit, integration, and E2E (placeholder). Unit tests use xUnit + Moq + AutoFixture and focus on observable outputs; integration tests use real PostgreSQL/PostGIS via TestContainers with one container per run, per-test reset, and real EF migrations (no `EnsureCreated`). Coverage has no hard gate initially and is tracked in CI until a stable baseline is established.
