# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.8 Data Access + Authorization — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

The next cross-cutting runtime milestone is **0.95 Generic External Host Integration**. It is a planned architectural hardening phase, not a host-specific integration.

## Verified implementation

The repository currently contains verified foundations for:

- provider/agent configuration and routing;
- execution lifecycle, timeout, cancellation, retries, diagnostics, and failure reporting;
- memory, persistent sessions, context budgeting, automatic/episodic/task memory;
- capability discovery and response normalization;
- streaming contracts and live streaming;
- tool definitions, registry, schema validation, provider transport, bounded tool loops, persistence, and per-agent assignment;
- WinForms UI Context with Form/UserControl attachment;
- semantic control and bound/native data-source discovery;
- CurrencyManager/current-item/source relationships;
- control-to-source relationship discovery;
- convention-based custom control adaptation;
- bounded application-object discovery;
- provider-neutral structured data projection/query contracts;
- runtime-instance foundations including independent runtime identity, lifecycle, memory ownership, execution revisions, scheduling, and optional runtime-state persistence;
- `HAgent.Example` verification for completed capabilities.

## Planned generic integration hardening

Phase 0.95 will complete the generic host boundary required for a broad class of LLM-driven software:

- arbitrary bounded host execution input/context;
- host-supplied correlation identity;
- host-defined structured-output contracts and validation;
- race-safe terminal execution semantics against late provider completion;
- runtime/execution identity propagation into tool execution;
- stronger isolation of mutable runtime overrides;
- deterministic verification of concurrent independent runtime instances.

These changes must remain provider-neutral and domain-neutral. Host state, lifecycle, scheduling policy, persistence, authorization, and side effects remain host-owned.

## Active implementation

The active implementation plan is the current file `docs/plan/20-active.md`. It must contain only the work being implemented now.

## Verification rule

A capability becomes complete only after its implementation exists, its matching `HAgent.Example` verification passes locally, and the project documentation reflects the result.

Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — non-negotiable engineering and repository rules.
- `docs/architecture/` — stable architectural design and boundaries.
- `docs/plan/` — master direction, current state, and active implementation only.
- `docs/roadmap/` — ordered path from completed foundations to the long-term target.
- `docs/storage.md` — persistence/backend details.

The root `plan.md` and `roadmap.md` are generated from their source directories. They are views, not independent sources of truth.
