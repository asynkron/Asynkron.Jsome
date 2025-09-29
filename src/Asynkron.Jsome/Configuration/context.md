# Generation Configuration

Configuration files let consumers reshape generated types without editing schemas. External configuration alters execution while
the core generator remains untouched.

## Components
- `ModifierConfiguration` — Strongly typed representation of configuration files (`global` toggles, per-property `rules`, enum
  handling, default include semantics). Provides helpers to fetch rules, inspect inclusion, and enumerate child paths.
- `PropertyRule` — Describes overrides for individual property paths (inclusion flags, `renameTo`, `enumType`, `jsonPropertyName`,
  validation hints like `minLength`/`maxLength`).
- `ConfigurationLoader` — Loads YAML or JSON modifier files, autodetects format, handles serialization exceptions, and exposes
  asynchronous/synchronous APIs plus a `Save` helper for writing configurations back to disk.
- `SchemaValidator` — Validates that modifier paths exist in the merged `SwaggerDocument`, surfaces Spectre.Console tables, and
  prevents misaligned rule application before generation.

These classes collaborate with [../CodeGeneration](../CodeGeneration/context.md) to adjust `PropertyInfo` metadata before template
rendering. Tests covering configuration behaviors live in
[../../../tests/Asynkron.Jsome.Tests/ConfigurationTests.cs](../../../tests/Asynkron.Jsome.Tests/context.md#configuration--modifier-tests)
and related fixtures.
