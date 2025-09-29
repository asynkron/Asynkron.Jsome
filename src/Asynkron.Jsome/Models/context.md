# Swagger Domain Models

`Models` defines the in-memory structures that both parsers and generators operate on. They provide a simplified Swagger 2.0
object graph (info, paths, parameters, schemas) that centralizes contract definitions for later stages of the pipeline.

## Type Breakdown
- `SwaggerDocument` — Root aggregate containing metadata (`Info`), host/base path, schemes, `Definitions`, and path items.
- `Info` — Title/version/description container surfaced by the CLI summaries and persisted into generated headers.
- `Schema` — Represents schema nodes and validation constraints, including arrays, objects, enums, `$ref`, `allOf`, `additionalProperties`,
  and vendor extensions. Includes `AdditionalPropertiesConverter` to support boolean/object shapes.
- `PathItem`, `Parameter`, `Response` — Minimal request/response metadata used primarily by future expansion points and to keep the
  model faithful to Swagger.
- `Xml`, `ExternalDocs` (nested within `Schema`) — Carry secondary metadata for XML output and documentation links.

These models are populated by [`../SwaggerParser.cs`](../SwaggerParser.cs) and
[`../JsonSchemaParser.cs`](../JsonSchemaParser.cs), then consumed by the view models in
[`../CodeGeneration`](../CodeGeneration/context.md).
