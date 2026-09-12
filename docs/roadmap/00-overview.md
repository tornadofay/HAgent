# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, current implementation state belongs under `docs/plan/`, and this directory defines the ordered delivery sequence.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage and first-class resource foundations substantially implemented; remaining repository/backend obligations are consumed by later storage/resource phases
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration complete and verified on .NET Framework 4.8.1 and .NET 9
- 0.951 — Identity, Tenancy + User Context completed and verified
- 0.952 — First-Class Event Subsystem completed and verified
- 0.953 — Unified Policy Engine completed for its canonical policy boundary; UI/backend hardening is post-phase work
- 0.954 — Prompt + Instruction Governance completed and verified on .NET Framework 4.8.1 and .NET 9
- 0.955 — Context Engineering completed and verified on .NET Framework 4.8.1 and .NET 9
- 0.956 — Observability + Distributed Tracing completed through its verified slices
- 0.957 — Evaluation + Quality Measurement completed and verified
- 0.9575 — Knowledge, Skills, Memory Governance + Learning **current**; Slices 1–7 verified, Slice 8 in progress
- 0.9576 — Learned Resource Reliability + Adaptation **planned follow-on**
- 0.958 — Agent Lifecycle + Health **planned**
- 0.9591 — Goal/Plan Persistence + Recovery **planned and ordered before intervention**
- 0.959 — Human-in-the-Loop + Intervention **planned; execution/learning intervention exists ahead of roadmap**
- 0.9592 — Provider Ecosystem + Adapter Lifecycle **planned**
- 0.96.x — Configuration, Storage + Portability **cross-cutting foundation before 0.96**
- 0.96 — Capability-Aware Execution **planned major execution foundation**
- 0.97 — Persistent Cognitive Runtime **planned production V1 cognitive layer**
- 0.98 — Model Reasoning Engineering **planned bounded reasoning-engineering layer**
- 0.10 — Workspaces, Routing + Chat **deferred user-facing product surface after the generic runtime/execution/cognition foundations are sufficient**
- 1.0 — Collaboration + Workflows **deferred orchestration layer built on 0.10, 0.959, 0.9591, 0.96, 0.97, and 0.98**
- Later — extensibility, developer platform, release hardening, and other ecosystem work

## Ordered V1 dependency chain

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Events
        ↓
0.953 Unified Policy
        ↓
0.954 Prompt / Instruction Governance
        ↓
0.955 Context Engineering
        ↓
0.956 Observability / Tracing
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 Knowledge / Skills / Memory Governance + Learning
        ↓
0.9576 Learned Resource Reliability + Adaptation
        ↓
0.958 Agent Lifecycle + Health
        ↓
0.9591 Goal / Plan Persistence + Recovery
        ↓
0.959 Human Intervention
        ↓
0.9592 Provider Ecosystem + Adapters
        ↓
0.96.x Configuration / Storage / Portability
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
        ↓
0.98 Model Reasoning Engineering
        ↓
0.10 Workspaces / Routing / Chat
        ↓
1.0 Collaboration / Workflows
```

The ordering is dependency-driven. A feature may be implemented early as an ahead-of-roadmap experiment, but that does not mark its ordered phase complete until the phase's full V1 boundaries and verification requirements are satisfied.

## Phase ownership map

Each responsibility has one authoritative owner. Later phases consume the contract instead of recreating it.

| Responsibility | Authoritative phase | Later consumers |
|---|---|---|
| Identity, tenancy, user context | 0.951 | all identity-aware phases |
| Generic events | 0.952 | runtime, cognition, workspace, workflows |
| Authorization/policy/precedence | 0.953 | instruction, context, learning, intervention, execution, cognition |
| Instruction authority/provenance | 0.954 | context, execution, cognition, reasoning engineering |
| Context retrieval/assembly/bounds | 0.955 | execution, cognition, reasoning engineering |
| Tracing/observability | 0.956 | all runtime subsystems |
| Evaluation/quality evidence | 0.957 | learning, reliability, execution diagnostics, cognition, reasoning engineering |
| Skill/Knowledge/Memory governance + learning promotion | 0.9575 | reliability, cognition, workspace, reasoning engineering |
| Post-promotion reliability/applicability/forgetting | 0.9576 | lifecycle, cognition, reasoning engineering |
| Runtime-agent lifecycle + runtime health | 0.958 | durable recovery, intervention, cognition |
| Durable goals/plans/checkpoints/recovery | 0.9591 | intervention, cognition, workflows |
| Human/host intervention boundary | 0.959 | lifecycle, plans, cognition, workflows, reasoning engineering |
| Provider adapter/discovery/operational evidence | 0.9592 | 0.96 execution, reasoning engineering |
| Configuration/storage/portability | 0.96.x | execution, cognition, workspaces, workflows |
| Concrete execution-target selection/admission | 0.96 | all inference-capable subsystems |
| Persistent per-agent cognition | 0.97 | reasoning engineering, workspace, collaboration, workflows |
| Model reasoning responsibility / deterministic-before-inference discipline | 0.98 | 0.10, 1.0, host applications |
| User-facing workspace/routing/chat | 0.10 | collaboration/workflows |
| Multi-agent collaboration/workflow orchestration | 1.0 | host applications |

The table is an architectural ownership rule, not merely a planning convenience.

## First-class resource architecture

Knowledge, Skills, Memory, and Learning are one coherent resource architecture.

```text
0.8 resource foundations
    identity / ownership / scope
    provenance / lifecycle / version metadata
    Skill definitions/references
    Knowledge/Wiki contracts
    Memory family/type foundations
    storage substrate

0.9575 mature resource governance + learning
    capability inheritance + runtime overrides
    authorization and resource governance
    bounded retrieval + retention
    Learning Mode
    typed candidates
    lifecycle / review / promotion
    version-safe authoritative resource creation
    context/runtime integration

0.9576 post-promotion reliability
    applicability / validity
    outcome-based trust evidence
    stale / contradiction / drift handling
    revalidation / replacement
    archival / forgetting
```

No later phase may create a parallel Memory, Knowledge, Skill, or Learning model merely because an earlier phase has not yet completed its management surface.

## Runtime and execution separation

The roadmap maintains a strict separation:

```text
Persistent Cognitive Runtime (0.97)
    decides whether/what cognitive progress is required

Model Reasoning Engineering (0.98)
    defines the bounded reasoning responsibility once probabilistic reasoning is requested

Execution Planner (0.96)
    decides where/how the requested inference executes

Provider Adapter (0.9592)
    knows provider-specific transport/discovery details

Execution Engine
    performs the selected request
```

The cognitive layer must not become a second provider/model router. The 0.98 reasoning layer must not become a second cognitive runtime or execution planner.

## Single-owner runtime invariant

Persistent cognition uses one authoritative state owner per runtime agent instance.

```text
one runtime instance
    → one authoritative state owner
    → serialized state mutation
    → asynchronous work returns results/events

many independent runtime instances
    → concurrent operation
    → isolated state and identity
```

This removes the need for a general multi-writer cognitive merge architecture in production V1. Revision/stale-result protection remains mandatory.

## 0.96 scope

Phase 0.96 is the only concrete execution-selection layer. It handles:

- capabilities and constraints;
- provider/model/execution-target selection;
- cost policy;
- quota/rate limits;
- concurrency/capacity admission;
- health/availability of execution targets;
- latency and bounded waiting;
- fallback/degradation;
- target diagnostics.

Provider health evidence originates at 0.9592; 0.96 consumes it. Cognitive strategies emit provider-neutral reasoning requirements and consume 0.96 rather than naming providers/models.

## 0.97 production V1 scope

The Persistent Cognitive Runtime provides:

- authoritative per-agent cognitive state;
- observations and beliefs;
- bounded DecisionWorkspace selection;
- goals and intentions;
- deterministic/reactive decisions;
- bounded deliberation and provider-neutral reasoning requirements;
- plans and recovery integration;
- governed learning candidates;
- learned-resource reliability consumption;
- lifecycle, persistence, cancellation, shutdown, observability, and evaluation;
- a production management/diagnostic workbench built on runtime state-transition APIs rather than direct persistence mutation.

The three auxiliary roadmap documents `cognitive-workbench.md`, `cognitive-workbench-controls.md`, and `cognitive-workbench-learning.md` are subdocuments of 0.97, not separate roadmap phases.

## 0.98 scope

Phase 0.98 is the bounded engineering layer between persistent cognition and provider/model inference. It does not decide whether the agent should reason in the first place; 0.97 owns that decision. It does not select the provider/model; 0.96 owns that decision.

0.98 owns:

- deterministic-before-inference responsibility boundaries;
- bounded model reasoning responsibilities;
- decomposition of semantic reasoning work;
- explicit use of authoritative evidence/context;
- reasoning-result contracts and uncertainty representation;
- validation of semantic reasoning results before authoritative use;
- provider/model-neutral reasoning quality verification.

The canonical shape of a future `ReasoningTask` or equivalent contract is intentionally **not decided by this roadmap entry**.

## 0.10 vs 1.0 boundary

0.10 is the user-facing workspace/routing/chat product surface. It does not become a general multi-agent workflow engine.

1.0 is the later orchestration layer for bounded multi-agent collaboration and workflows. It consumes 0.10 routing, 0.959 intervention, 0.9591 durable planning/recovery, 0.96 execution selection, 0.97 persistent cognition, and 0.98 reasoning engineering. It must not recreate those lower-level authorities.

## Ahead-of-roadmap implementation rule

Existing implementation may appear before its ordered phase. Such work is retained when useful, but it is labeled as ahead-of-roadmap evidence and does not silently reorder the roadmap.

This rule applies especially to current execution intervention, learning-candidate intervention, runtime examples, resource foundations, and other experiments created while earlier foundations were still being completed.

## Cross-phase V1 verification scenarios

Phase exit criteria prove their own bounded contracts. V1 completion also requires a small set of concrete scenarios that exercise the seams between authoritative phases. These are integration scenarios, not replacements for phase-specific verification.

### Scenario 1 — Provider failure and recovery

A persistent cognitive decision in 0.97 produces a provider-neutral `ReasoningRequirement` → 0.96 selects an execution target → the selected provider fails or reaches a governed rate/capacity boundary → 0.96 admits a valid fallback target → execution completes → 0.97 continues the active goal/plan without creating a second provider-selection authority.

### Scenario 2 — Learning and restart recovery

0.97 captures a meaningful experience → 0.9575 admits a typed learning candidate through policy and Learning Mode → governed promotion creates authoritative resource state → the host/runtime restarts → 0.958/0.9591 recovery restores the durable cognitive state → the promoted resource remains authoritative, version-safe, and governed after restart.

### Scenario 3 — Intervention and stale asynchronous result

0.97 starts bounded asynchronous reasoning → 0.959 intervention changes the authoritative target state or lifecycle revision → the old reasoning result arrives → the owning runtime rejects it as stale/superseded → the runtime continues from the current authoritative state without corrupting newer state.

### Scenario 4 — Independent concurrent agents without a shared cognitive bottleneck

At least 10 independent runtime agents receive work concurrently → each agent serializes mutation only within its own state owner → each agent completes its work without waiting behind another agent's owner queue → each agent retains isolated identity, revision, and state → observed concurrency demonstrates that one agent's workload does not collapse all agents behind a shared cognitive queue.

These scenarios must use deterministic/fake infrastructure where possible and must report the concrete phase boundaries exercised. They do not require every execution to pass through every phase; each scenario tests a specific real cross-phase dependency.

## Roadmap maintenance rules

1. One phase owns each responsibility; later phases consume earlier contracts rather than recreating them.
2. Every substantial phase has a bounded V1 exit criterion.
3. Production V1 requirements are separated from V2/research work.
4. Architectural invariants outrank implementation convenience.
5. Historical foundation checklists are normalized when later implementation proves the item already exists; genuine missing work is assigned to the phase that consumes it.
6. If implementation proves a dependency wrong, update this ordered roadmap and the affected phase documents together before continuing.
7. Auxiliary documents under `docs/roadmap/` must name their parent phase and may not create an independent milestone or authority.
8. Generated root `roadmap.md` remains a view; authoritative ordering lives in `docs/roadmap/`.
9. Verification depth must match the risk and nature of the claim. Concurrency, cancellation/lifecycle, persistence/recovery, authorization/security, performance, and public API claims require verification appropriate to those claims rather than a blanket requirement that every feature use the same test type.
10. A production architecture mechanism that materially affects V1 scope must have a corresponding roadmap item or explicit scope reference; architecture and roadmap must not silently drift apart.
