# Schema Catalog

Reference JSON schemas used to validate generator behavior and provide ready-made inputs for integration scenarios. These serve
as Durable Functions-style external services that the orchestrator must coordinate with.

- [`guidewire`](guidewire/context.md) — Claims management schema sample with deep object graphs and enum coverage.
- [`ocppv16`](ocppv16/context.md) — Open Charge Point Protocol v1.6 message schemas (dozens of request/response pairs) consumed
  heavily by the OCPP tests.

Integration tests under [../tests/Asynkron.Jsome.Tests](../tests/Asynkron.Jsome.Tests/context.md#domain-specific-integrations)
load these directories to ensure the generator handles real-world payload complexity.
