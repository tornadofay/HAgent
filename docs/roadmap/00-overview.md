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
- 0.96 — Capability-Aware Execution **planned next — provider/runtime hardening**
- 0.97 — Persistent Cognitive Runtime **planned after 0.96 and before resuming higher-level autonomous-agent features**
- 0.10 — Workspaces, Routing + Chat — paused until the generic runtime/capability/cognitive foundations are sufficient
- 0.11 — Knowledge, Skills, Memory Governance + Learning — planned platform feature layer
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

Phase 0.11 converts existing memory/skill/wiki foundations into a coherent scoped resource model and adds controlled learning, review, capability inheritance, runtime overrides, and management UI. It must consume the generic runtime contracts rather than create project-specific exceptions.

Phase 0.95 is a completed cross-cutting runtime/API hardening phase. It established the generic execution boundary for arbitrary hosts: host input/context, host correlation, structured output contracts and validation, provider-facing request isolation, execution terminality, tool identity propagation, runtime snapshot isolation, provider-native structured-output transport, and external-consumer verification. It does not introduce any host-specific domain dependency.

Phase 0.96 makes execution-target selection and admission capability-aware. It must complete before the persistent cognitive layer depends on heterogeneous-provider routing, quota/capacity management, and long-running execution semantics.

Phase 0.97 adds the missing higher-level runtime above individual executions: a long-lived cognitive runtime that owns persistent cognitive state, receives environment events, manages attention, goals, intentions and plans, decides when reactive handling is sufficient, and activates deliberative executions only when needed. It is generic and must not contain HWorld- or business-application-specific domain logic.

The roadmap distinguishes feature phases from generic runtime hardening. Higher-level features may continue later, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.
