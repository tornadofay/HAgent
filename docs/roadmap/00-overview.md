# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. Stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage foundations substantially implemented; Skills/Wiki management and broader knowledge governance were deferred
- 0.9 — Runtime Agent Instances complete and locally verified
- 0.95 — Generic External Host Integration **complete and verified on .NET Framework 4.8.1 and .NET 9**
- 0.951 — Identity, Tenancy + User Context — planned architectural foundation
- 0.952 — First-Class Event Subsystem — planned architectural foundation
- 0.953 — Unified Policy Engine — planned architectural foundation
- 0.954 — Prompt + Instruction Governance — planned architectural foundation
- 0.955 — Context Engineering — planned architectural foundation
- 0.956 — Observability + Distributed Tracing — planned architectural foundation
- 0.957 — Evaluation + Quality Measurement — planned architectural foundation
- 0.958 — Agent Lifecycle + Health Management — planned architectural foundation
- 0.959 — Human-in-the-Loop + Intervention — planned architectural foundation
- 0.9591 — Goal/Plan Persistence + Recovery — planned foundation
- 0.9592 — Provider Ecosystem + Adapter Lifecycle — planned provider-platform foundation
- 0.96 — Capability-Aware Execution — planned next major execution foundation
- 0.97 — Persistent Cognitive Runtime — planned after the pre-0.96 foundations and capability-aware execution
- 0.10 — Workspaces, Routing + Chat — paused until the generic runtime/capability/cognitive foundations are sufficient
- 0.11 — Knowledge, Skills, Memory Governance + Learning — planned platform feature layer
- 1.0 — Collaboration + Workflows
- Later — extensibility, developer platform, release hardening, and other ecosystem work

The pre-0.96 foundation phases intentionally precede capability-aware execution because they define reusable identity, events, policy, instruction trust, context assembly, tracing, evaluation, lifecycle/intervention, durable cognitive recovery, and provider adapter boundaries that later execution and cognition layers should consume rather than reinvent.

Phase 0.11 converts existing memory/skill/wiki foundations into a coherent scoped resource model and adds controlled learning, review, capability inheritance, runtime overrides, and management UI. It must consume the generic runtime contracts rather than create project-specific exceptions.

Phase 0.95 is a completed cross-cutting runtime/API hardening phase. It established the generic execution boundary for arbitrary hosts: host input/context, host correlation, structured output contracts and validation, provider-facing request isolation, execution terminality, tool identity propagation, runtime snapshot isolation, provider-native structured-output transport, and external-consumer verification. It does not introduce any host-specific domain dependency.

Phase 0.96 makes execution-target selection and admission capability-aware. It builds on the pre-0.96 foundations and the storage evolution defined by `docs/roadmap/38-configuration-storage-and-portability.md`. It defines the Execution Planner as the execution-side planner: it selects and admits a concrete provider/model/deployment target for an already-formed inference request and exposes a normalized target assessment.

Phase 0.97 adds the missing higher-level runtime above individual executions: a long-lived cognitive runtime that owns persistent cognitive state, receives environment events, manages attention, goals, intentions and plans, selects relevant resources, applies decision policies, decides when reactive handling is sufficient, creates or revises plans, and activates deliberative executions only when needed. It is generic and must not contain HWorld- or business-application-specific domain logic.

The roadmap explicitly distinguishes two kinds of planning:

```text
Cognitive Planner
    = what should the agent do?

Execution Planner
    = where/how should the required inference execute?
```

Phase 0.97 also establishes `DecisionContext`, `DecisionPolicy`, and `Planner` as explicit provider-neutral architectural concepts. Resource retrieval and relevance ranking remain separate from attention: attention determines what matters now, while retrieval/relevance determines which Memory, Knowledge, Skills, or other resources are useful about it.

The roadmap distinguishes feature phases from generic runtime hardening. Higher-level features may continue later, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.
