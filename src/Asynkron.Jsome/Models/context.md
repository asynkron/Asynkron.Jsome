# Swagger Domain Models

`Models` defines the in-memory structures that both parsers and generators operate on. They provide a simplified Swagger 2.0 object graph (info, paths, parameters, schemas) analogous to Durable Functions' durable entity state—centralized objects that capture contract definitions for later execution.

## Highlights
- `SwaggerDocument` — Root aggregate containing metadata (`Info`), host/base path, schema `Definitions`, and `Paths` operations.
- `Schema` — Represents schema nodes, tracks type information, validation constraints, `$ref` references, enum values, array/object metadata, and vendor extensions.
- `PathItem`, `Parameter`, `Response` — Minimal structures for endpoint data (used primarily when generating validator context or future expansions).
- `Info` — Title/version/description container used for output metadata.

These models are populated by [../SwaggerParser.cs](../SwaggerParser.cs) and [../JsonSchemaParser.cs](../JsonSchemaParser.cs), then consumed by [../CodeGeneration](../CodeGeneration/context.md) to derive template view models.
