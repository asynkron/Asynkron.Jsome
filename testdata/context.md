# Test Data

Holds large or external sample inputs used during tests and demos.
- `stripe-swagger.json` — Realistic Swagger 2.0 spec for validating parser resilience and generation performance. Referenced by
  parser tests and manual experiments when you need a production-grade contract.

Treat these artifacts like Durable Functions emulator payloads—feed them into the generator to observe behavior without touching
production definitions.
