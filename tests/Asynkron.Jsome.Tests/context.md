# Asynkron.Jsome Test Project

xUnit project validating parser correctness, configuration handling, template rendering, and language feature toggles. The
structure resembles Azure Durable Functions' multi-phase testing: unit-level checks for orchestrator logic, integration tests for
external definitions, and compilation validation to ensure generated code executes.

## Test Categories & Files
- **Code Generation Basics**
  - `CodeGenerationTests` — DTO/validator emission, enum handling, namespace overrides, and template fallback scenarios.
  - `CompilationValidationTests` — Runs Roslyn compilation against generated sources to guarantee syntactic validity.
- **Configuration & Modifier Tests**
  - `ConfigurationTests` — Round-trips YAML/JSON modifier files through `ConfigurationLoader`.
  - `ModifierConfigurationIntegrationTests` — Applies property rules during generation to verify inclusion/exclusion behavior.
  - `SchemaValidatorTests` — Ensures modifier paths map to actual schemas and emits Spectre.Console feedback just like CLI runs.
- **Template Extensibility & Serialization**
  - `CustomTemplateTests` — Points the generator at alternate template folders and checks partial template sets.
  - `ProtoTemplateTests` — Exercises `.proto` frontmatter metadata and extension mapping.
  - `SystemTextJsonTests` — Validates System.Text.Json attributes/validation when the corresponding CLI switch is enabled.
- **Modern C# Features**
  - `ModernCSharpFeaturesTests` — Confirms nullable reference type annotations, `required` keyword usage, and record generation.
- **Parser Validation**
  - `SwaggerParserTests` — Guards version checks, error messaging, and summary formatting.
  - `JsonSchemaParserTests` — Covers directory ingestion, duplicate detection, `$ref` resolution, and malformed schema handling.
- **Domain-Specific Integrations**
  - `OcppV16IntegrationTests` — Generates code from [../../../schemas/ocppv16](../../../schemas/ocppv16/context.md) and verifies
    file counts plus representative validators.
  - `OcppV16ComplianceTests` — Focuses on enum/constant expectations for OCPP payloads.
- **Localization / Regression**
  - `LocaleIssueTest` — Prevents culture-specific parsing regressions (e.g., decimal separators).

Shared usings live in `GlobalUsings.cs`, and the project definition resides in `Asynkron.Jsome.Tests.csproj`. The suite depends on
the CLI project at [../../src/Asynkron.Jsome](../../src/Asynkron.Jsome/context.md).
