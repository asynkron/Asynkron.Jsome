# Code Generation Layer

This folder contains the transformation pipeline that converts `SwaggerDocument` models into strongly-typed output using
Handlebars templates. Think of it as the Durable Functions "activity" stage: the orchestrator (`Program.cs`) supplies the inputs,
these classes compute the payloads, and the templates act like output bindings that persist the work.

## Core Components
- `CodeGenerator` — Loads template files (respecting custom directories or curated template lists), registers helpers, merges in
  optional modifier configuration, and produces `CodeGenerationResult`. Handles proto feature toggles, template frontmatter
  parsing, and missing-template diagnostics surfaced to the CLI.
- `CodeGenerationOptions` — Feature flags toggled by CLI switches (nullable/required support, record DTOs, System.Text.Json vs.
  Newtonsoft.Json, Swashbuckle annotations, proto generation, template overrides, modifier configuration location).
- `CodeGenerationResult` / `GeneratedFile` — Aggregates the emitted artifacts and captures metadata such as output path, logical
  name, file extension (from `TemplateMetadata`), and contents.

## Template View Models
- `ClassInfo` — Represents a generated DTO, including namespace, type name, base types (`allOf`), required properties, enum/const
  dependencies, and validator metadata.
- `PropertyInfo` — Stores property type, nullability, validation hints, JSON attribute info, enum references, and modifier-driven
  overrides. Tests manipulate these heavily in
  [../../tests/Asynkron.Jsome.Tests/ModifierConfigurationIntegrationTests.cs](../../tests/Asynkron.Jsome.Tests/context.md#configuration--modifier-tests).
- `EnumInfo` — Describes integer-backed enum generation with optional descriptions and `[EnumMember]` support.
- `TemplateMetadata` — Reads YAML-style frontmatter from `.hbs` files (`extension`, `description`) so outputs can determine
  filenames and documentation strings.

## Constants & Proto Support
- `ConstantsInfo` (within `ClassInfo`/`PropertyInfo`) and corresponding templates surface string-enum values as static classes.
- Proto helpers ensure message/enum templates reuse the same metadata as C# outputs; see
  [../../tests/Asynkron.Jsome.Tests/ProtoTemplateTests.cs](../../tests/Asynkron.Jsome.Tests/context.md#template-extensibility--serialization).

All of these classes depend on the normalized models in [`../Models`](../Models/context.md) and respond to modifiers defined in
[`../Configuration`](../Configuration/context.md).
