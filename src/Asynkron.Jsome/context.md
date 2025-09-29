# `Asynkron.Jsome` Project

Compiles into the `dotnet-jsome` global tool. The program emulates a Durable Functions orchestration: `Program.cs` collects input,
invokes parser "activities", validates modifier bindings, and finally calls the code generator that fans out to template writers.

## Entry Points & CLI Flow
- `Program.cs`
  - Bootstraps System.CommandLine commands (`generate`, `help`) and renders Spectre.Console banners/help text.
  - Validates mutually exclusive inputs (single Swagger file vs. schema directory), normalizes paths, and surfaces configuration
    summaries with ANSI formatting.
  - Applies feature toggles (modern C#, records, System.Text.Json attributes, Swashbuckle annotations, proto emission, custom
    template selection) through `CodeGenerationOptions` before executing generation.
  - Provides convenience summaries (`DisplaySwaggerSummary`, `DisplayJsonSchemaSummary`) and confirmation prompts akin to Durable
    Functions checkpoints before the activity work starts.
- `Asynkron.Jsome.csproj` (linked from [../context.md](../context.md)) wires in Handlebars.Net, Spectre.Console, FluentValidation,
  and serialization packages used throughout the pipeline.

## Parser Activities
- `SwaggerParser.cs` — Deserializes Swagger 2.0 JSON, enforces spec-level invariants (version, info fields), and produces
  `SwaggerDocument` aggregates. Offers `GetDocumentSummary` for CLI reporting.
- `JsonSchemaParser.cs` — Walks a directory of JSON Schema files, merging root schemas and internal `definitions`, deduplicating by
  semantic equality, and ensuring `$ref` targets exist. Serves the same role as a Durable fan-in activity composing results before
  orchestration continues.

## Domain Models & Configuration
- Models live in [`Models`](Models/context.md) and capture the normalized Swagger object graph consumed downstream.
- Modifier bindings in [`Configuration`](Configuration/context.md) apply inclusion/exclusion, naming, and validation hints before
  generation. `Program.cs` hydrates these via `ConfigurationLoader` and validates them with `SchemaValidator` prior to code
  emission.

## Generation Pipeline
- [`CodeGeneration`](CodeGeneration/context.md) houses the transformation logic. `Program.cs` constructs `CodeGenerationOptions`
  based on CLI switches, hands control to `CodeGenerator`, and handles file output/overwrite prompts.
- [`Templates`](Templates/context.md) stores the Handlebars bindings. `Program.cs` supports overriding the template directory or
  specifying a curated template list.

## Samples & Support Assets
- [`Samples`](Samples/context.md) contains ready-to-run Swagger documents and modifier configs for demos.
- Root-level configuration examples (`config_demo.cs`, YAML/JSON templates) show how to author modifier files programmatically.

Validation and regression coverage reside in [../../tests/Asynkron.Jsome.Tests](../../tests/Asynkron.Jsome.Tests/context.md),
mirroring Durable Functions' reliance on automated integration tests to protect orchestrator behavior.
