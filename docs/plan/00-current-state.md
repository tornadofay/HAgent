# Current project state

HAgent is a lightweight, provider-neutral .NET agent runtime. It can be used for simple chat or embedded into a host application, simulation, game, or other environment.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.8 Data Access + Authorization — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

## Verified platform foundations

- Provider/agent configuration and routing.
- Execution lifecycle, timeout, cancellation, retries, and diagnostics.
- Memory, persistent sessions, context budgeting, automatic/episodic/task memory.
- Capability discovery and response normalization.
- Streaming contracts and live streaming.
- Tool definitions, registry, schema validation, provider tool transport, bounded tool loops, persistence, and per-agent assignment.
- WinForms UI Context with Form/UserControl attachment.
- Semantic control and bound/native data-source discovery.
- CurrencyManager/current-item/source relationships.
- Convention-based custom control adapters.
- Bounded live application-object discovery.
- Provider-neutral structured data projection/query contracts.

## Development rule

A capability is complete only when implementation exists, the relevant `HAgent.Example` verification passes locally, and project documentation reflects the result.

Implement one focused slice at a time. Do not claim local build/test success unless it was actually performed.

## Documentation ownership

- `README.md` — public introduction and quick start.
- `AGENTS.md` — engineering invariants and non-negotiable rules.
- `docs/architecture/` — stable architectural design.
- `docs/plan/` — current implementation state and completed ledger.
- `docs/roadmap/` — future phases and ordering.
- `docs/storage.md` — persistence/storage-specific design.

Root `plan.md` and `roadmap.md` are generated from the corresponding source directories.
