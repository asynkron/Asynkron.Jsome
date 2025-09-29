# OCPP v1.6 JSON Schemas

Collection of per-message schema files mirroring the Open Charge Point Protocol 1.6 specification. Filenames follow the `<Action>[Response].json` pattern. Use them to:
- Stress-test `JsonSchemaParser.ParseDirectory` with dozens of definitions and `$ref` links.
- Validate enum generation, array constraints, and nested structures under real-world load.

Integration tests in [../../../tests/Asynkron.Jsome.Tests/OcppV16IntegrationTests.cs](../../../tests/Asynkron.Jsome.Tests/context.md#ocpp-v16-tests) rely on this directory. The experience parallels Durable Functions fan-out/fan-in orchestrations over a large set of activities, except here the workload is schema ingestion.
