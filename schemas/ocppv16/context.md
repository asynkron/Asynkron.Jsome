# OCPP v1.6 Schemas

Wrapper for the Open Charge Point Protocol message definitions. These files feed integration tests that verify the generator can
handle a large schema corpus.

- [`json_schemas`](json_schemas/context.md) — Individual request/response schema files (Authorize, BootNotification, etc.).

Like Durable Functions orchestrating IoT command flows, these schemas model bidirectional charger interactions, giving the
generator realistic validation paths. See
[../../tests/Asynkron.Jsome.Tests/context.md#domain-specific-integrations](../../tests/Asynkron.Jsome.Tests/context.md#domain-specific-integrations)
for the test suites that consume this directory.
