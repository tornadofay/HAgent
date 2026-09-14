# Functionality Preservation and Deduplication

## 1. Rule

The rebuild preserves **required behavior**, not obsolete implementation details.

Every significant existing HAgent capability is classified as:

- `KEEP` — the capability remains HAgent-owned;
- `MAF` — MAF now provides the required capability and HAgent adopts it;
- `ADAPT` — MAF provides the mechanism and HAgent adds a thin contract/adapter;
- `EXTEND` — MAF provides only part; HAgent supplies a replaceable missing layer;
- `DECIDE` — MAF and HAgent have materially different contracts and the owner must choose;
- `REMOVE` — duplicated or obsolete implementation with no remaining required behavior.

## 2. Existing HAgent capability inventory

| HAgent capability | Target classification | Notes |
|---|---|---|
| Provider-neutral agent execution | ADAPT / DECIDE | Use MAF + MEAI underneath; public HAgent contract needs explicit owner decision |
| Agent profiles | KEEP | HAgent-specific reusable configuration identity |
| Runtime instances | KEEP | Distinct HAgent ownership/lifecycle concept |
| Immutable execution snapshots | KEEP | Strong HAgent concurrency invariant |
| Cancellation / timeout handling | EXTEND | Use MAF execution cancellation plus HAgent terminal-state arbitration |
| Stale-result protection | KEEP | HAgent runtime/lifecycle invariant |
| Provider adapters | REDUCE / ADAPT | Replace generic transport with MEAI/MAF where equivalent |
| Provider discovery | KEEP | HAgent resource/planning capability |
| Logical model identity | KEEP | HAgent planning metadata |
| Execution targets | KEEP | HAgent planning/admission concept |
| Capability evidence | KEEP | HAgent-specific evidence/provenance model |
| Quota/admission | KEEP | HAgent operational planning requirement |
| Tools | MAF + ADAPT | MAF invocation mechanics; retain HAgent authorization/host boundary |
| Structured output | MAF + ADAPT | Use MAF/MEAI semantics, preserve HAgent validation contract if stronger |
| Middleware | MAF / REMOVE duplicates | HAgent policy remains where semantically distinct |
| Events | DECIDE / ADAPT | Keep HAgent domain events; consume MAF lifecycle/instrumentation rather than duplicating it |
| Observability | ADAPT | Consume MAF/OpenTelemetry; retain HAgent cognitive/runtime events |
| Evaluation | KEEP | HAgent evaluation/quality domain remains distinct unless MAF directly satisfies it |
| Identity / tenancy / ownership | KEEP | HAgent canonical model |
| Policy engine | KEEP / REMOVE duplicate pipeline checks | Retain authority model; use MAF middleware as execution point |
| Skills | DECIDE | MAF has Skills; compare semantics to HAgent governed versioned Skills |
| Knowledge | KEEP | HAgent broader governed Knowledge model |
| Wiki | KEEP | One HAgent managed knowledge source |
| Memory | EXTEND | Reuse MAF context/memory mechanisms where suitable; retain HAgent memory governance |
| Learning | KEEP / EXTEND | No assumed MAF replacement for governed learning promotion |
| Learning candidate review | KEEP | HAgent-specific governance |
| Persistent goals | KEEP | HAgent cognition |
| Intentions | KEEP | HAgent cognition |
| Plans | KEEP / EXTEND | MAF workflows may execute portions; HAgent cognitive plans remain distinct |
| Cognitive Kernel | KEEP | HAgent-specific |
| Cognitive Strategies | KEEP | HAgent-specific and provider-neutral |
| Reasoning Requirement | KEEP | HAgent decides required capability before execution planning |
| Execution Planner | KEEP | HAgent selects eligible concrete target |
| Lifecycle / health | KEEP / EXTEND | MAF execution health is not automatically HAgent runtime lifecycle |
| Human intervention | EXTEND | Use MAF HITL mechanics; keep HAgent intervention semantics and persistence |
| Goal/plan persistence and recovery | EXTEND | Reuse MAF checkpoint/state mechanisms where semantically compatible |
| Configuration portability | KEEP | HAgent authoritative configuration model |
| File / SQL Server / MySQL storage | KEEP / REDESIGN | Preserve storage goals; remove duplicated serialization/state where MAF storage can own the same state |
| WinForms management UI | KEEP | HAgent-specific management product surface |
| UI Context / Control Adapters | KEEP | HAgent-specific host integration |
| HWorld integration boundary | KEEP | HWorld remains external |
| Generic workflow engine implementation | REMOVE where equivalent | MAF is the workflow foundation |
| Generic multi-agent orchestration duplication | REMOVE where equivalent | Use MAF orchestrations |
| Generic agent middleware implementation | REMOVE | Use MAF middleware |
| Duplicate model/provider invocation plumbing | REMOVE | Use MEAI/MAF |

## 3. Required preservation principle

The rebuild is successful only when the new system still supports all project requirements represented by the existing architecture, roadmap, tests, examples, and host integrations.

However, preserving a behavior does not mean preserving the existing class, database table, API shape, or algorithm. The repository is explicitly in redesign/rebuild mode and permits obsolete mechanisms to be removed.

## 4. Deduplication procedure

For each existing HAgent implementation:

1. Identify the externally required behavior.
2. Find the closest current MAF/MEAI capability.
3. Compare semantics, lifecycle, concurrency, persistence, security, extension, and observability.
4. If equivalent, delete the duplicate implementation and integrate MAF.
5. If partial, move only the missing semantics into a replaceable HAgent layer.
6. If contracts differ, record both options and stop for owner decision before permanently choosing a public boundary.
7. Update tests and Examples to verify the new ownership model.

## 5. What must not be duplicated

The following should have one canonical implementation after the rebuild:

- generic agent invocation pipeline;
- generic middleware chaining;
- generic workflow graph execution;
- generic tool/function-call invocation;
- generic provider chat transport already covered by MEAI/MAF;
- generic MAF orchestration patterns;
- generic MAF HITL workflow mechanics;
- generic tracing that can be consumed from MAF/OpenTelemetry;
- protocol plumbing already provided by MAF for supported A2A/MCP scenarios.

## 6. What is intentionally not duplication

The following are not considered duplication merely because MAF exposes related primitives:

- an HAgent runtime instance owning persistent cognitive state;
- HAgent's execution planner selecting a concrete target;
- HAgent learning candidates and governed promotion;
- HAgent resource ownership and scope;
- HAgent policy/authorization boundaries;
- HAgent host authority semantics;
- HAgent cognitive goals/intentions/plans when they serve a different lifecycle than a MAF workflow;
- HAgent-specific storage for data MAF does not own;
- HAgent WinForms integration.

## 7. Regression categories

No functionality-loss claim is accepted until the rebuild covers, as applicable:

- success paths;
- failure paths;
- cancellation;
- timeout;
- concurrency;
- stale/late result handling;
- lifecycle transitions;
- persistence/restore;
- authorization and policy boundaries;
- resource ownership/isolation;
- provider capability uncertainty;
- learning approval/rejection;
- structured output validation;
- host-side side-effect control;
- Example/manual workflows corresponding to public APIs.
