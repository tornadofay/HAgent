# Current project state

## Project

HAgent is a lightweight, provider-neutral .NET cognition and execution runtime. Its purpose is to provide reusable LLM infrastructure for software projects of different types without requiring HAgent.Core to understand any host-specific domain model.

## Supported targets

- .NET Framework 4.8.1
- .NET 9 where supported
- No GPU requirement
- Low-memory operation is a design constraint

## Current milestone

**0.95 Generic External Host Integration — active**

0.7 WinForms UI Context + Data Discovery is complete and locally verified.

0.8 Data Access + Authorization + Internal Storage foundations are substantially implemented and manually verified across supported storage backends; remaining internal repository parity is intentionally deferred.

0.9 Runtime Agent Instances is complete for the generic runtime contract and manually verified through deterministic Example coverage. HWorld is an external consumer rather than an HAgent dependency.

0.10 Workspaces, Routing + Chat has an implemented and locally verified routing foundation, but workspace execution/chat remains deferred until the 0.95 host boundary is stable.

0.95 Generic External Host Integration is the current active hardening milestone.

The next major capability layer after the generic host boundary is **Knowledge + Skills + Memory Governance + Learning**, including management UI and profile/runtime capability controls.

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
- HAgent-owned storage configuration for File, SQL Server, and MySQL backends;
- application-specific File storage layout;
- HAgent-owned SQL Server/MySQL database bootstrap foundations;
- bounded internal inventory, memory, conversation, and execution-audit read tools;
- automatic payload-free execution auditing with configurable bounded retention;
- runtime-instance identity, scope, runtime-only overrides, independent memory ownership, concurrent execution, stale-result protection, host-controlled scheduling, shutdown semantics, and optional runtime-state persistence;
- provider-neutral workspace participants, message metadata, and default-recipient routing;
- canonical generic host execution requests with multiple messages, host correlation identity, and bounded host context (pending local verification).

## Planned Knowledge / Skills / Learning layer

The next capability layer must establish one coherent model rather than treating Wiki, Skills, and Memory as unrelated features:

```text
Skills    = reusable executable capabilities
Knowledge = reusable retrievable information
Wiki      = managed persistent knowledge source
Memory    = scoped experience/state
Learning  = controlled transformation of experience into typed candidates
```

Learning candidates are typed as `MemoryCandidate`, `KnowledgeCandidate`, or `SkillCandidate` and retain provenance/evidence. Candidate creation does not automatically make information authoritative.

`LearningMode` is configurable as:

- `Disabled` — no learning candidates;
- `SuggestOnly` — candidates are reviewable but not promoted automatically;
- `AutomaticWithPolicy` — explicit policy may promote approved candidate classes/scopes;
- `FullyAutomatic` — explicit advanced opt-in for automatic promotion.

The default governance target is `SuggestOnly` unless the existing configuration model establishes a safer default during implementation.

## Capability configuration target

Agent profiles provide reusable defaults. A runtime instance inherits those defaults and can apply runtime-only tri-state overrides (`Inherit`, `Enabled`, `Disabled`). The policy must support at least:

- skill capability and individual skill resources;
- Wiki/knowledge capability and individual knowledge resources;
- memory capability and individual memory families/types;
- future resource types through a generic resource/type ID model.

Execution snapshots capture the effective policy so configuration or runtime changes cannot alter already-running work.

## Management UI target

`HAgent.WinForms` must add:

- Learning Review: pending suggestions, provenance/evidence, source execution/runtime, proposed scope, approve/reject;
- Wiki/Knowledge Manager: new/edit/delete, search/filter, relationships, and which agents use/access each resource;
- Skill Manager: new/edit/delete, version/status, relationships, and which agents use each skill;
- Agent Configuration knowledge view: when an agent is selected, show effective Skills, Wiki/Knowledge access, Memory families, and all other resource types exposed by the generic inventory;
- profile-level enable/disable and runtime-instance-level overrides for skills, Wiki/knowledge, and individual memory types.

The agent knowledge view must be future-proof: known types may have specialized UI, but unknown/new types remain visible through the generic resource inventory rather than requiring a new hard-coded agent property.

## Boundaries

Knowledge, skills, memory, learning, and their management UI remain HAgent-owned generic infrastructure. No host-specific application or HWorld type may be introduced into Core.

Storage implementations remain responsible for persistence. WinForms owns administration surfaces. The model is never the authorization authority; learning promotion and capability enforcement occur through code/policy.

## Planned generic integration hardening

Phase 0.95 completes the generic host boundary required for a broad class of LLM-driven software:

- arbitrary bounded host execution input/context;
- host-supplied correlation identity;
- host-defined structured-output contracts and validation;
- race-safe terminal execution semantics against late provider completion;
- runtime/execution identity propagation into tool execution;
- stronger isolation of mutable runtime overrides;
- deterministic verification of concurrent independent runtime instances.

These changes remain provider-neutral and domain-neutral. Host state, lifecycle, scheduling policy, persistence, authorization, and side effects remain host-owned.

## Active implementation

The active implementation plan is `docs/plan/20-active.md`. It contains only work being implemented now. Knowledge/Skills/Learning remains planned until the generic host boundary and subsequent capability-governance work are ready.

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
