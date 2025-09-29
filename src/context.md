# Source Tree Overview

This directory contains the production code for the `Asynkron.Jsome` .NET tool. The layout intentionally mirrors the major phases of the generation pipeline:

- [`Asynkron.Jsome`](Asynkron.Jsome/context.md) — CLI host, parsers, and all generation logic.

While Azure Durable Functions organizes work into orchestrations, activity functions, and bindings, this source tree separates concerns into parsing, configuration, and templated emission. Each subdirectory's `context.md` explains how those responsibilities map onto Durable-like concepts (e.g., modifier rules behaving like orchestrator policies).
