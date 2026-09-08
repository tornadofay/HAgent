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
- 0.96.x — Configuration, Storage + Portability Evolution — cross-cutting foundation for 0.96/0.97
- 0.96 — Capability-Aware Execution — planned major execution foundation
- 0.97 — Persistent Cognitive Runtime — planned long-lived cognitive layer
- 0.10 — Workspaces, Routing + Chat — paused until the generic runtime/capability/cognitive foundations are sufficient
- 0.11 — Knowledge, Skills, Memory Governance + Learning — planned platform feature/governance layer
- 1.0 — Collaboration + Workflows
- Later — extensibility, developer platform, release hardening, and other ecosystem work

The pre-0.96 foundation phases intentionally precede capability-aware execution because they define reusable identity, events, policy, instruction trust, context assembly, tracing, evaluation, lifecycle/intervention, durable goal/plan recovery, provider adapter boundaries, and configuration/storage evolution that later execution and cognition layers should consume rather than reinvent.

Phase 0.11 builds on memory, skills, and knowledge primitives already present in HAgent and later provides mature scoped resource governance, learning review/promotion, capability inheritance, runtime overrides, and management UI. Phase 0.97 may consume existing resource primitives before the complete 0.11 governance layer is finished, but must not create a parallel resource architecture.

Phase 0.95 is a completed cross-cutting runtime/API hardening phase. It established the generic execution boundary for arbitrary hosts: host input/context, host correlation, structured output contracts and validation, provider-facing request isolation, execution terminality, tool identity propagation, runtime snapshot isolation, provider-native structured-output transport, and external-consumer verification. It does not introduce any host-specific domain dependency.

The configuration/storage evolution phase establishes the persistence model required by capability-aware execution and persistent cognition: provider/model/target separation, encrypted provider credentials, global settings, resource relationships, shared database deployment, cache invalidation, and portable configuration export/import. It is ordered before 0.96 even though its historical source filename contains `38`.

Phase 0.96 makes execution-target selection and admission capability-aware. It defines the Execution Planner as the execution-side planner: it selects and admits a concrete provider/model/deployment target for an already-formed inference request and exposes normalized target assessment.

Phase 0.97 adds the higher-level Persistent Cognitive Runtime above individual executions. It owns persistent cognitive state, event activation, belief state and revision, attention/global workspace, goals, intentions, plans, operators, impasses, cognitive actions, experience, learning/proceduralization, and adaptive decisions about whether probabilistic reasoning is warranted. The first reference strategy is **Adaptive Hybrid Cognition (AHC)**, but HAgent deliberately treats cognition as an extensible strategy layer rather than assuming AHC is the final architecture.

The roadmap explicitly distinguishes three decisions:

```text
Cognitive Strategy
    = how should the agent reason and manage cognitive state?

Reasoning Requirement
    = what kind of reasoning capability is needed now?

Execution Planner
    = where/how should that reasoning execute?
```

A cognitive strategy may decide that no LLM is required, use a lightweight model, escalate to a stronger model, invoke multiple reasoning passes, or request another form of information gathering. Model/provider choice remains the responsibility of the execution-planning layer after cognition has expressed its reasoning requirement.

Persistent cognition must become more effective through validated experience without silently rewriting the cognitive kernel. Experience may produce memory, knowledge, skill, policy, or cognitive-improvement candidates; validated candidates may be promoted and later make recurring situations more deterministic and less dependent on LLM inference. Unresolved or novel situations remain able to return to deliberative reasoning.

The roadmap also includes a live Cognitive Runtime Workbench for `HAgent.WinForms`. Authorized operators should be able to inspect and, through governed runtime APIs, intervene in active beliefs, goals, intentions, plans, attention, memory, experience, reasoning decisions, and cognitive history.

The roadmap distinguishes feature phases from generic runtime hardening. Higher-level features may continue later, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.
