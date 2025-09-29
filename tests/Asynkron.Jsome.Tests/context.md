# Asynkron.Jsome Test Project

xUnit project validating parser correctness, configuration handling, template rendering, and language feature toggles. The structure resembles Azure Durable Functions' multi-phase testing: unit-level checks for orchestrator logic, integration tests for external definitions, and compilation validation to ensure generated code executes.

## Test Categories
- **Code Generation Basics** (`CodeGenerationTests`, `CompilationValidationTests`) — Ensure DTOs/validators compile, required attributes appear, and generated code is syntactically valid.
- **Configuration & Modifier Tests** (`ConfigurationTests`, `ModifierConfigurationIntegrationTests`, `SchemaValidatorTests`) — Verify `ModifierConfiguration` loading, rule application, and schema path validation. Mirrors Durable Function binding configuration tests.
- **Template Extensibility** (`CustomTemplateTests`, `ProtoTemplateTests`, `SystemTextJsonTests`) — Guard template selection, proto output, and System.Text.Json enhancements.
- **Modern C# Features** (`ModernCSharpFeaturesTests`) — Confirm nullable references, `required` keyword, and record generation toggles.
- **Parser Validation** (`SwaggerParserTests`, `JsonSchemaParserTests`) — Validate error handling and parsing semantics for Swagger documents and JSON Schema directories.
- **Domain-Specific Integrations** (`OcppV16ComplianceTests`, `OcppV16IntegrationTests`) — Exercise large schema sets from [../../schemas/ocppv16](../../schemas/ocppv16/context.md) to mimic Durable fan-out workloads over many definitions.
- **Localization / Regression** (`LocaleIssueTest`) — Prevent culture-specific parsing regressions.

Global usings live in `GlobalUsings.cs`, and the project is defined by `Asynkron.Jsome.Tests.csproj`.
