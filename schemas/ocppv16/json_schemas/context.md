# OCPP v1.6 JSON Schemas

Collection of per-message schema files mirroring the Open Charge Point Protocol 1.6 specification. Filenames follow the
`<Action>[Response].json` pattern. Use them to:
- Stress-test `JsonSchemaParser.ParseDirectory` with dozens of definitions and `$ref` links.
- Validate enum generation, array constraints, and nested structures under real-world load.

Integration tests in
[../../../tests/Asynkron.Jsome.Tests/OcppV16IntegrationTests.cs](../../../tests/Asynkron.Jsome.Tests/context.md#domain-specific-integrations)
and `OcppV16ComplianceTests` rely on this directory. Expect fan-out/fan-in style workloads because the generator must ingest a
large set of related schemas.
