# Repository Overview

Asynkron.Jsome is a .NET 8 global tool that converts Swagger 2.0 and JSON Schema documents into C# DTOs, validators, enums, and optional Protocol Buffer definitions.

## Key Capabilities
- Command-line entry point in [src/Asynkron.Jsome/Program.cs](src/Asynkron.Jsome/Program.cs) builds a `generate` pipeline around `CodeGenerator`, configuration loading, and schema parsing.
- Parsing libraries in [src/Asynkron.Jsome/SwaggerParser.cs](src/Asynkron.Jsome/SwaggerParser.cs) and [src/Asynkron.Jsome/JsonSchemaParser.cs](src/Asynkron.Jsome/JsonSchemaParser.cs) normalize Swagger 2.0 or JSON Schema directories into shared `SwaggerDocument` models.
- Code emission uses Handlebars templates in [src/Asynkron.Jsome/Templates](src/Asynkron.Jsome/Templates/context.md) and runtime options defined under [src/Asynkron.Jsome/CodeGeneration](src/Asynkron.Jsome/CodeGeneration/context.md).

- Extensive verification lives in [tests/Asynkron.Jsome.Tests](tests/Asynkron.Jsome.Tests/context.md), ensuring generated code compiles, adheres to configuration, and covers integrations such as OCPP v1.6 schemas.

## Support Assets
- Sample schemas and configuration examples reside under [schemas](schemas/context.md), [testdata](testdata/context.md), and [src/Asynkron.Jsome/Samples](src/Asynkron.Jsome/Samples/context.md).
- Automation pipelines are tracked in [.github/workflows](.github/workflows/context.md).
- Refer to [src/context.md](src/context.md) for a deeper breakdown of the production code tree.
