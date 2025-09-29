# Code Generation Layer

This folder contains the transformation pipeline that converts `SwaggerDocument` models into strongly-typed output using Handlebars templates. Think of it as the "activity function" layer compared to Azure Durable Functions—the orchestrator (`Program.cs`) supplies the input, these classes perform the heavy lifting, and templates act like Durable output bindings.

## Key Types
- `CodeGenerator` — Loads templates, registers helper functions, enforces template availability, and materializes DTOs, validators, enums, constants, and optional Protocol Buffer artifacts according to `CodeGenerationOptions`.
- `CodeGenerationOptions` — Feature toggles for enum generation, modern C# (nullable + `required` keyword), System.Text.Json, Swashbuckle annotations, record types, proto files, and custom template selection.
- `CodeGenerationResult` / `GeneratedFile` — Data holders for generated source; results aggregate per-entity artifacts while `GeneratedFile` tracks extension metadata extracted from template frontmatter.
- `ClassInfo`, `PropertyInfo`, `EnumInfo`, `ConstantsInfo` — Template-ready view models capturing schema descriptions, validation metadata, enum values, and optional references to dedicated enum/constant types.

## Supporting Concepts
- `TemplateMetadata` reads optional YAML-style frontmatter from `.hbs` files, similar to how Durable Functions use trigger metadata to customize behavior.
- Validation rule constructs mirror FluentValidation DSL calls so templates can enumerate them consistently.

Refer to [../Templates/context.md](../Templates/context.md) for the default templates that consume these models and [../Configuration/context.md](../Configuration/context.md) for the rule system that influences property shaping.
