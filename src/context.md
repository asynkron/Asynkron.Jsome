# Source Tree Overview

The `src` folder houses the production toolchain. `Asynkron.Jsome.sln` points to a single project,
[`Asynkron.Jsome`](Asynkron.Jsome/context.md), which packs the CLI host, parsers, configuration system, code generator, templates,
and sample inputs. Treat this folder as the Durable Functions "app"—the orchestrator, activities, and bindings all live below it.

Supporting build assets:
- [`Asynkron.Jsome.sln`](../Asynkron.Jsome.sln) ties the CLI project to the tests under [`../tests`](../tests/context.md).
- The project file [`Asynkron.Jsome.csproj`](Asynkron.Jsome/Asynkron.Jsome.csproj) defines dependencies
  (System.CommandLine, Spectre.Console, Handlebars.Net, FluentValidation, Newtonsoft.Json, YamlDotNet) required by the
  orchestration pipeline.

Each subdirectory includes its own `context.md` with Durable-inspired metaphors for the responsibilities they cover:
- [`Asynkron.Jsome`](Asynkron.Jsome/context.md) — entry point and feature pipeline.
- [`Asynkron.Jsome/Templates`](Asynkron.Jsome/Templates/context.md) — template bindings.
- [`Asynkron.Jsome/CodeGeneration`](Asynkron.Jsome/CodeGeneration/context.md) — activity-style transformation logic.
- [`Asynkron.Jsome/Configuration`](Asynkron.Jsome/Configuration/context.md) — modifier bindings and validation.
- [`Asynkron.Jsome/Models`](Asynkron.Jsome/Models/context.md) — shared schema state.
- [`Asynkron.Jsome/Samples`](Asynkron.Jsome/Samples/context.md) — starter payloads and configs.
