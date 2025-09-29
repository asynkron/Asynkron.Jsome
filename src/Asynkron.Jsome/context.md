# `Asynkron.Jsome` Project

This project compiles into the `dotnet-jsome` global tool. The code orchestrates schema ingestion, configuration, and template-driven code emission in a way reminiscent of Azure Durable Functions orchestrations—`Program.cs` dispatches work across modular components—but the execution is single-run and stateless rather than long-lived workflows.

## Entry Points
- `Program.cs` wires up `System.CommandLine` commands, Spectre.Console prompts, and dispatches to parsers and generators. It enforces mutual exclusivity between Swagger inputs and schema directories, validates modifier configuration, and invokes code generation.
- `SwaggerParser.cs` and `JsonSchemaParser.cs` convert raw Swagger 2.0 JSON or JSON Schema directories into `SwaggerDocument` models while resolving `$ref` dependencies and detecting conflicting definitions.

## Domain Models
- [`Models`](Models/context.md) defines the in-memory Swagger 2.0 representation consumed by generation.
- [`Configuration`](Configuration/context.md) supplies modifier rules, validation logic, and helpers that function similarly to Durable Functions' bindings—augmenting generation without modifying the core orchestrator.

## Generation Pipeline
- [`CodeGeneration`](CodeGeneration/context.md) contains classes that transform models into template-ready metadata and produce `GeneratedFile` instances.
- [`Templates`](Templates/context.md) exposes the Handlebars templates used by the generator; see the folder context for format details.

## Samples and Assets
- [`Samples`](Samples/context.md) offers example inputs and configuration seeds for quick testing.

Tests validating these behaviors live under [../../tests/Asynkron.Jsome.Tests](../../tests/Asynkron.Jsome.Tests/context.md), providing durable-like regression guarantees.
