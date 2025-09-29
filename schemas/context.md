# Schema Catalog

Reference JSON schemas used to validate generator behavior and provide ready-made inputs for integration scenarios. Treat them as
external contracts that the CLI must coordinate with during code generation.

- [`guidewire`](guidewire/context.md) — Claims management schema sample with deep object graphs and enum coverage.
- [`ocppv16`](ocppv16/context.md) — Open Charge Point Protocol v1.6 message schemas (dozens of request/response pairs) consumed
  heavily by the OCPP tests.

Integration tests under [../tests/Asynkron.Jsome.Tests](../tests/Asynkron.Jsome.Tests/context.md#domain-specific-integrations)
load these directories to ensure the generator handles real-world payload complexity.
