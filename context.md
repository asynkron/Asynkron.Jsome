# Repository Overview

Asynkron.Jsome ships as a .NET 8 global tool (`dotnet-jsome`) that ingests Swagger 2.0 or loose JSON Schema files and emits
C#/Proto code via Handlebars templates. The repository is organized around that pipeline: the CLI host lives in `src`, supporting
inputs sit beside it, and the tests/automation keep the entire flow deterministic.

## Top-Level Map
- [`src`](src/context.md) — Production code. `Asynkron.Jsome` is the sole project and contains the CLI host, parsers, code
  generation pipeline, and templates.
- [`tests`](tests/context.md) — xUnit suites that compile generated output, validate configuration edge cases, and stress-test the
  parsers with large schema catalogs.
- [`schemas`](schemas/context.md) & [`testdata`](testdata/context.md) — Real-world schema corpora consumed by integration tests
  and demos (OCPP v1.6, Guidewire, Stripe). Use these when you need exhaustive payload coverage.
- [`src/Asynkron.Jsome/Samples`](src/Asynkron.Jsome/Samples/context.md) — Quick-start inputs plus modifier configuration
  templates for experimentation.
- [`generate.sh`](generate.sh) / [`validate-nuget-setup.sh`](validate-nuget-setup.sh) — Convenience scripts for local generation
  and verifying the tool’s NuGet packaging prerequisites.
- [`config_demo.cs`](config_demo.cs) and the YAML files at the root illustrate how configuration objects are materialized from the
  `ModifierConfiguration` model.
- CI/CD definitions reside under [`.github/workflows`](.github/workflows/context.md); Copilot/editor guidance sits in
  [`.copilot`](.copilot/context.md).

## Quick Entry Points
- Start at [`src/Asynkron.Jsome/Program.cs`](src/Asynkron.Jsome/context.md#entry-points) for CLI wiring and option handling.
- Follow the end-to-end flow through the parser layers
  ([`SwaggerParser`](src/Asynkron.Jsome/context.md#entry-points),
  [`JsonSchemaParser`](src/Asynkron.Jsome/context.md#entry-points)), configuration modifiers
  ([`Configuration`](src/Asynkron.Jsome/Configuration/context.md)), generation activities
  ([`CodeGeneration`](src/Asynkron.Jsome/CodeGeneration/context.md)), and template bindings
  ([`Templates`](src/Asynkron.Jsome/Templates/context.md)).
- For regression expectations, consult [`tests/Asynkron.Jsome.Tests`](tests/Asynkron.Jsome.Tests/context.md) which documents the
  suite layout and links back to the assets it exercises.

This `context.md` and its descendants form the AI-facing search index—skim them whenever you need to orient yourself before
reading code.
