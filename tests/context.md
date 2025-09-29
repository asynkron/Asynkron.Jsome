# Test Suite Overview

The `tests` folder exercises the generator end to end: generated artifacts must compile, configuration must bind
correctly, and large schema workloads must succeed before shipping the CLI. All tests use xUnit and target .NET 8.

- [`Asynkron.Jsome.Tests`](Asynkron.Jsome.Tests/context.md) — Primary test project covering code generation, configuration,
  parser behaviors, template customizations, proto output, modern C# toggles, and OCPP v1.6 integrations.

The solution ties this project to [`src/Asynkron.Jsome`](../src/Asynkron.Jsome/context.md); workflows under
[../.github/workflows](../.github/workflows/context.md) execute it on every commit.
