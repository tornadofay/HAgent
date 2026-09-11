# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, current implementation state belongs under `docs/plan/`, and this directory defines the ordered delivery sequence.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage and first-class resource foundations substantially implemented
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration complete and verified on .NET Framework 4.8.1 and .NET 9
- 0.951 — Identity, Tenancy + User Context completed and verified
- 0.952 — First-Class Event Subsystem completed and verified
- 0.953 — Unified Policy Engine foundation completed and verified for current runtime/resource/learning-policy boundaries
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
- 0.10 — Workspaces, Routing + Chat **deferred until the generic runtime/execution/cognition foundations are sufficient**
- 1.0 — Collaboration + Workflows
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
0.10 Workspaces / Routing / Chat
```

The ordering is dependency-driven. A feature may be implemented early as an ahead-of-roadmap experiment, but that does not mark its ordered phase complete until the phase's full V1 boundaries and verification requirements are satisfied.

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
    decides what the agent should do

Execution Planner (0.96)
    decides where/how a requested inference should execute

Provider Adapter (0.9592)
    knows provider-specific transport/discovery details

Execution Engine
    performs the selected request
```

The cognitive layer must not become a second provider/model router. The execution planner must not become a cognitive planner.

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

This replaces the need for a general multi-writer cognitive merge architecture in production V1. Revision/stale-result protection remains mandatory; semantic multi-writer proposal arbitration is deferred.

## 0.96 scope

Phase 0.96 is the only concrete execution-selection layer. It handles:

- capabilities and constraints;
- provider/model/execution-target selection;
- cost policy;
- quota/rate limits;
- concurrency/capacity admission;
- health/availability;
- latency and bounded waiting;
- fallback/degradation;
- target diagnostics.

Cognitive strategies emit provider-neutral reasoning requirements and consume 0.96 rather than naming providers/models.

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
- lifecycle, persistence, cancellation, shutdown, observability, and evaluation.

It is explicitly **not** a claim to implement a complete cognitive theory, human cognition, AGI, consciousness, universal planning, neural continual learning, or distributed cognitive consensus.

## Ahead-of-roadmap implementation rule

Existing implementation may appear before its ordered phase. Such work is retained when useful, but it is labeled as ahead-of-roadmap evidence and does not silently reorder the roadmap.

This rule applies especially to current execution intervention, learning-candidate intervention, runtime examples, and other experiments created while earlier foundations were still being completed.

## Roadmap maintenance rules

1. One phase owns each responsibility; later phases consume earlier contracts rather than recreating them.
2. Every substantial phase has a bounded V1 exit criterion.
3. Production V1 requirements are separated from V2/research work.
4. Architectural invariants outrank implementation convenience.
5. If implementation proves a dependency wrong, update this ordered roadmap and the affected phase documents together before continuing.
6. Generated root `roadmap.md` remains a view; authoritative ordering lives in `docs/roadmap/`.
