# Phased Rebuild Plan

The rebuild starts at Phase 1. It is not a continuation of the existing 0.958/0.9591 code sequence. The current implementation is audited first, then the new architecture is built from the results.

```text
Phase 1  Baseline + MAF feasibility
Phase 2  Target contracts + dependency graph
Phase 3  .NET 10 solution reset
Phase 4  MAF/MEAI foundation integration
Phase 5  Execution/runtime migration
Phase 6  Memory/Knowledge/Skills/Learning migration
Phase 7  Cognitive Runtime reconstruction
Phase 8  Persistence/storage/portability
Phase 9  Host/WinForms/HWorld integration
Phase 10 Full regression + performance + hardening
Phase 11 Final architecture reconciliation and release
```

## Phase 1 — Baseline, inventory, and MAF feasibility

**Goal:** establish a single authoritative rebuild baseline before changing implementation.

Work:

- Read current `AGENTS.md`, architecture, plan, roadmap, storage, Example, and test documentation.
- Resolve the documented checkpoint discrepancy between README and `docs/plan/00-current-state.md`.
- Inventory all HAgent projects, public contracts, persistence stores, providers, UI, Examples, and tests.
- Build a capability inventory independent of class names.
- Map every major capability to current MAF/MEAI functionality.
- Inspect exact MAF/MEAI packages, current APIs, target frameworks, persistence APIs, workflow APIs, tool APIs, sessions, context providers, middleware, skills, HITL, and observability.
- Verify actual .NET 10 compatibility of every selected MAF package.
- Identify APIs that are still preview and isolate them behind adapters when they must be consumed.
- Identify direct dependencies that prevent the .NET 10-only target.
- Record all duplicate HAgent infrastructure candidates.

Outputs:

- capability inventory;
- dependency inventory;
- MAF equivalence matrix;
- duplicate implementation list;
- partial MAF gap list requiring replaceable extensions;
- owner decision list;
- approved target project graph.

**Exit gate:** no implementation phase begins until the capability matrix is accepted and unresolved decisions are explicit.

## Phase 2 — Target contracts and dependency graph

**Goal:** freeze ownership boundaries before project conversion.

- Define HAgent Core contracts after deduplication.
- Decide public HAgent versus direct MAF contracts for each `DECIDE` item.
- Define the replacement boundary for every `EXTEND` item.
- Define execution snapshot and lifecycle ownership.
- Define MAF adapter ownership.
- Define storage ownership between MAF and HAgent.
- Define provider/execution-target planner boundary.
- Define public package boundaries and allowed references.

**Exit gate:** no circular dependency and no accidental Core-to-MAF coupling unless explicitly approved.

## Phase 3 — .NET 10 solution reset

**Goal:** remove legacy target-framework constraints.

- Remove `net481` and `net9.0` target frameworks from the target solution.
- Remove framework-compatibility conditionals that exist only for old targets.
- Remove obsolete packages and binding redirects.
- Convert project format where necessary.
- Standardize central package management.
- Align analyzers, nullable, implicit usings, and language version with .NET 10.
- Keep Windows targeting explicit for WinForms projects.

**Exit gate:** all target projects restore and compile under `.NET 10` with no obsolete target branch remaining.

## Phase 4 — MAF/MEAI foundation integration

**Goal:** replace duplicated generic AI infrastructure first.

- Integrate MAF agent abstractions.
- Integrate `IChatClient` and `IEmbeddingGenerator` where applicable.
- Replace duplicated agent invocation pipeline.
- Replace duplicated middleware pipeline.
- Replace duplicated tool invocation machinery.
- Replace duplicated generic orchestration with MAF workflows.
- Integrate MAF human-intervention/checkpoint support where appropriate.
- Integrate MAF/OpenTelemetry observability.
- Add HAgent adapter contracts only where semantics differ.

**Exit gate:** no remaining duplicate generic MAF capability without a documented semantic reason.

## Phase 5 — Execution and runtime migration

**Goal:** place HAgent runtime semantics around MAF execution.

- Rebuild canonical execution request/snapshot contracts.
- Connect HAgent runtime instances to MAF agents/sessions.
- Preserve identity separation between host correlation, execution, and runtime instances.
- Preserve cancellation, timeout, stale-result, and terminal-state arbitration.
- Connect execution-planner output to MAF/MEAI clients.
- Rebuild provider discovery and target admission around the MAF-compatible execution boundary.

**Exit gate:** a complete execution can be admitted, executed through MAF, and finalized through HAgent without losing lifecycle or authority guarantees.

## Phase 6 — Memory, Knowledge, Skills, and Learning

**Goal:** rebuild resource systems without duplicate agent infrastructure.

- Map MAF skills/context providers to HAgent Skills where semantics match.
- Define what MAF context/session state can own versus HAgent Memory.
- Preserve scope, ownership, provenance, and versioning.
- Preserve profile/runtime tri-state overrides.
- Preserve learning candidates, review, approval/rejection, promotion, stale handling, and concurrency rules.
- Make every HAgent extension replaceable when it closes a temporary MAF gap.

**Exit gate:** resource governance works without duplicating MAF execution mechanics and learned resources cannot silently mutate authoritative state.

## Phase 7 — Persistent Cognitive Runtime

**Goal:** rebuild HAgent's distinctive cognition above MAF.

- Preserve the stable Cognitive Kernel.
- Preserve replaceable Cognitive Strategies.
- Preserve deterministic reasoning where sufficient.
- Preserve Reasoning Requirement versus Execution Planner separation.
- Preserve observations, beliefs, goals, intentions, plans, operators, impasses, experience, and cognitive actions.
- Use MAF workflows/agents as execution mechanisms where useful without equating a workflow with cognitive state.
- Integrate MAF state/checkpoint facilities only where they can own the same authoritative state without duplication.
- Preserve restart, recovery, and revision safety.

**Exit gate:** a runtime can stop, persist authoritative cognitive state, restart, recover, and continue without treating the model or a workflow object as the authority over cognition.

## Phase 8 — Persistence, storage, and portability

**Goal:** produce one coherent persistence model.

- Audit every existing HAgent storage shape against MAF state.
- Remove duplicate persistent representations.
- Keep HAgent-owned stores for HAgent-specific resources.
- Use MAF persistence for MAF-owned state where compatible.
- Preserve encrypted credential storage where HAgent owns credentials.
- Preserve explicit package/version/conflict rules for configuration portability.
- Keep live runtime objects, handlers, and transient state non-portable.

**Exit gate:** each durable datum has exactly one authoritative owner.

## Phase 9 — Host, WinForms, and HWorld integration

**Goal:** preserve real-world host usability.

- Rebuild the WinForms management surface on the new HAgent state model.
- Preserve UI Context / Control Adapter capabilities.
- Remove UI code that duplicates generic MAF management where HAgent does not need a separate surface.
- Preserve HWorld's external ownership boundary.
- Rebuild HWorld adapter scenarios against .NET 10.
- Update the external-consumer sample.

**Exit gate:** desktop hosts, generic hosts, and HWorld can use HAgent without depending on HAgent internals or owning HAgent state accidentally.

## Phase 10 — Regression, performance, and hardening

**Goal:** prove behavior was retained while duplicated infrastructure was removed.

- Full HAgent.Tests regression.
- Example verification for every meaningful migrated capability.
- Cancellation, timeout, stale-result, and lifecycle tests.
- Persistence/restore tests.
- Learning governance tests.
- Provider failure, rate-limit, and admission tests.
- Concurrency and isolation tests.
- Storage consistency tests.
- Authority and policy boundary tests.
- Performance tests for persistent cognition and multi-runtime concurrency.
- WinForms resource-leak checks.
- Dependency/package review.

**Exit gate:** all mandatory scenarios pass on .NET 10 with no unresolved duplicate implementation that violates the deduplication policy.

## Phase 11 — Final reconciliation and release

**Goal:** remove migration residue and publish the new canonical architecture.

- Remove temporary adapters superseded by final MAF capabilities.
- Remove dead code and obsolete compatibility paths.
- Update authoritative architecture documents.
- Update roadmap and current-state documents.
- Update README and package documentation.
- Verify public API surface and dependency graph.
- Tag the new release baseline.

**Final condition:** HAgent is a coherent .NET 10-only project built on MAF where practical, while retaining the HAgent-specific architecture and functionality required by the project goals.
