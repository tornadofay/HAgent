# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage **and first-class resource foundations** — substantially implemented
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration **complete and verified on .NET Framework 4.8.1 and .NET 9**
- 0.951 — Identity, Tenancy + User Context — **completed and verified**
- 0.952 — First-Class Event Subsystem — completed and verified
- 0.953 — Unified Policy Engine — completed for its verified runtime/persistence/resource/learning-policy foundation
- 0.954 — Prompt + Instruction Governance — **completed and verified on .NET Framework 4.8.1 and .NET 9**
- 0.955 — Context Engineering — **completed and verified on .NET Framework 4.8.1 and .NET 9**
- 0.956 — Observability + Distributed Tracing — **current ordered milestone**
- 0.957 — Evaluation + Quality Measurement — planned architectural foundation
- 0.9575 — Knowledge, Skills, Memory Governance + Learning — planned mature resource/governance phase
- 0.958 — Agent Lifecycle + Health Management — planned architectural foundation
- 0.9591 — Goal/Plan Persistence + Recovery — **ordered before 0.959**; planned foundation
- 0.959 — Human-in-the-Loop + Intervention — planned architectural foundation; ahead-of-roadmap execution intervention implementation exists
- 0.9592 — Provider Ecosystem + Adapter Lifecycle — planned provider-platform foundation
- 0.96.x — Configuration, Storage + Portability Evolution — cross-cutting foundation for 0.96/0.97
- 0.96 — Capability-Aware Execution — planned major execution foundation
- 0.97 — Persistent Cognitive Runtime — planned long-lived cognitive layer
- 0.10 — Workspaces, Routing + Chat — paused until the generic runtime/capability/cognitive foundations are sufficient
- 1.0 — Collaboration + Workflows
- Later — extensibility, developer platform, release hardening, and other ecosystem work

## First-class resource architecture

Knowledge, Skills, Memory, and Learning are now treated as a first-class architectural concern from the foundation upward rather than as a late product feature.

The roadmap deliberately separates two layers:

```text
0.8 Resource Foundations
    identity / scope / ownership metadata
    provenance / version / lifecycle metadata
    Skill definitions and references
    Knowledge / Wiki resource contracts and retrieval boundaries
    Memory family/type foundations
    typed learning-candidate foundations
    HAgent-owned persistence substrate

0.9575 Mature Resource Governance + Learning
    policy and authorization
    capability inheritance and runtime overrides
    effective execution resource snapshots
    bounded retrieval / retention governance
    Knowledge / Skill management
    Learning modes and learning policy
    candidate validation / evaluation / approval / promotion
    resource/version conflict handling
```

This separation is intentional. HAgent establishes canonical resource contracts early enough that Context, Instruction Governance, Runtime, Evaluation, and Persistent Cognition can consume them directly. Mature governance is completed only after the identity, policy, instruction, context, and evaluation boundaries required to govern those resources exist.

Learning is not treated as a synonym for memory or as a late model feature. It is a controlled lifecycle from experience to typed candidate to validation/policy/approval and finally promotion into an authoritative resource or scoped state.

The four concepts remain distinct:

```text
Skills    = reusable executable capabilities/procedures
Knowledge = reusable retrievable information
Memory    = scoped experience/state
Learning  = governed transformation of experience into candidates/promotions
```

No phase may introduce a parallel resource model merely because the mature governance phase has not yet been completed. Existing resource primitives may be consumed by earlier phases, while access, promotion, and authoritative mutation remain subject to the governance boundaries defined later.

## Foundational sequence

The current ordered foundations are:

```text
0.951 Identity / Tenancy / User Context
        ↓
0.952 Event subsystem
        ↓
0.953 Unified Policy Engine
        ↓
0.954 Prompt / Instruction Governance — verified
        ↓
0.955 Context Engineering — verified
        ↓
0.956 Observability / Tracing — current
        ↓
0.957 Evaluation / Quality Measurement
        ↓
0.9575 Knowledge / Skills / Memory Governance + Learning
        ↓
0.958 Agent Lifecycle / Health
        ↓
0.9591 Goal / Plan Persistence / Recovery
        ↓
0.959 Human-in-the-Loop / Intervention
        ↓
0.9592 Provider Ecosystem / Adapter Lifecycle
        ↓
0.96.x Configuration / Storage / Portability Evolution
        ↓
0.96 Capability-Aware Execution
        ↓
0.97 Persistent Cognitive Runtime
```

The placement of 0.9591 before 0.959 is deliberate: durable goal/plan revisions, checkpoints, and recovery state provide the persistent authority that later goal/plan-step intervention can govern. Execution-level intervention remains independently valid.

The pre-0.96 foundations define reusable identity, events, policy, resource foundations, instruction trust, context assembly, tracing, evaluation, resource governance, lifecycle, durable goal/plan recovery, human intervention, provider adapter boundaries, and configuration/storage evolution that later execution and cognition layers should consume rather than reinvent.

## Resource relationship to later cognition

Phase 0.97 Persistent Cognitive Runtime consumes Memory, Knowledge, and Skills as first-class resources and may consume existing resource contracts before every 0.9575 management surface is complete. It must not create a second memory/knowledge/skill architecture or bypass resource governance.

Persistent cognition can generate experiences and learning signals, but learned changes remain subject to typed candidates, provenance, evaluation, policy, authorization, versioning, and explicit promotion. The cognitive kernel itself remains independently versioned and must never be silently rewritten by model output or learning.

## Configuration and storage relationship

The configuration/storage evolution phase remains a cross-cutting foundation before 0.96 because capability-aware execution and persistent cognition depend on provider/model/target separation, encrypted provider credentials, global configuration, resource relationships, shared-database behavior, snapshot invalidation, and portable configuration contracts.

## Roadmap rules

The roadmap is dependency-driven rather than a permanent product-feature lock. A phase may be reordered when new architectural understanding reveals a genuine dependency change; such changes should update the authoritative roadmap and current-state documents together. Existing ahead-of-roadmap implementation remains implementation evidence, not milestone completion, until its ordered phase and full requirements are verified.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.
