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
- 0.95 — Generic External Host Integration **active**
- 0.10 — Workspaces, Routing + Chat — routing foundation implemented and locally verified; execution/chat remains deferred
- 0.11 — Knowledge, Skills, Memory Governance + Learning **planned next major feature layer**
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

Phase 0.11 converts existing memory/skill/wiki foundations into a coherent scoped resource model and adds controlled learning, review, capability inheritance, runtime overrides, and management UI. It must consume the generic runtime contracts rather than create project-specific exceptions.

Phase 0.95 is a cross-cutting runtime/API hardening phase. It is the current active milestone and closes the remaining gap between the runtime architecture and a reusable LLM boundary: arbitrary host input/context, host correlation, structured output contracts, execution terminality, tool identity propagation, and strong runtime isolation. It does not introduce any host-specific domain dependency.

The roadmap distinguishes feature phases from generic runtime hardening. Higher-level features may continue later, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.
