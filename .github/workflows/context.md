# Workflow Pipelines

GitHub Actions automation ensuring build and release quality:
- `ci.yml` — Restores, builds, and tests on pushes/PRs against `main` and `develop`. Serves as the continuous verification gate before releases.
- `publish.yml` — Triggered on version tags or release publication; rebuilds, tests, packs the NuGet tool, and pushes to nuget.org using `NUGET_API_KEY`.

These workflows depend on `dotnet 8.0.x` runners and enforce the contract validated by [../../tests/Asynkron.Jsome.Tests](../../tests/Asynkron.Jsome.Tests/context.md).
