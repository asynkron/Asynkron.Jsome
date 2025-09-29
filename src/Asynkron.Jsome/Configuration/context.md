# Generation Configuration

Configuration files let consumers reshape generated types without editing schemas. The pattern parallels Azure Durable Functions' binding metadata—external configuration alters execution while the core orchestrator remains untouched.

## Components
- `ModifierConfiguration` — Strongly typed representation of configuration files (`global` toggles, per-property `rules`, enum handling, etc.).
- `PropertyRule` — Describes overrides for individual property paths (inclusion, renaming, validation hints) similar to Durable Function retry or timeout policies.
- `ConfigurationLoader` — Loads YAML or JSON modifier files asynchronously or synchronously; auto-detects format, handles serialization errors, and can persist configurations.
- `SchemaValidator` — Validates that modifier paths exist in the merged `SwaggerDocument`, surfaces Spectre.Console feedback, and prevents misaligned rule application.

These classes collaborate with [../CodeGeneration](../CodeGeneration/context.md) to adjust `PropertyInfo` metadata before template rendering. Tests covering configuration behaviors live in [../../../tests/Asynkron.Jsome.Tests/ConfigurationTests.cs](../../../tests/Asynkron.Jsome.Tests/context.md#configuration--modifier-tests).
