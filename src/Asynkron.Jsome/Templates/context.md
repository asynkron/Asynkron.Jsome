# Handlebars Templates

These templates define the textual output for the generator. Each file may start with optional frontmatter consumed by `TemplateMetadata` to determine extensions (e.g., `.proto`).

## Default Outputs
- `DTO.hbs`, `DTORecord.hbs` — Generate C# classes or records with configurable validation attributes, System.Text.Json or Newtonsoft.Json annotations, and optional Swashbuckle metadata.
- `Validator.hbs` — FluentValidation validator classes aligned with property rules.
- `Enum.hbs` — Enum definitions for integer-backed enums.
- `Constants.hbs` — Static classes exposing string enum choices.
- `proto.hbs`, `proto.enum.hbs`, `proto.string_enum.hbs` — Optional Protocol Buffer message/enum renderers.
- `FSharp.hbs`, `FSharpModule.hbs`, `TypeScript.hbs` — Ancillary templates for alternate target languages.

Templates act like Durable Functions output bindings: they express the shape of generated artifacts while `CodeGenerator` injects runtime metadata from `ClassInfo`/`PropertyInfo` view models.
