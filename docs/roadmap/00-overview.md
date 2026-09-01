# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. It describes phases and dependencies; stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization + Internal Storage foundations substantially implemented; Skills/Wiki and remaining internal-repository parity are deferred
- 0.9 — Runtime Agent Instances foundations complete and locally verified
- 0.10 — Workspaces, Routing + Chat **active**
- 0.95 — Generic External Host Integration **planned cross-cutting hardening**
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

Phase 0.95 is a cross-cutting runtime/API hardening phase. It can be implemented alongside unfinished higher-level roadmap work, but its completion is required before HAgent can claim a complete generic external-host integration surface. It closes the remaining gap between the runtime architecture and a reusable LLM boundary: arbitrary host input/context, host correlation, structured output contracts, execution terminality, tool identity propagation, and strong runtime isolation. It does not introduce any host-specific domain dependency. Scheduling, authorization, host state, persistence, and side effects remain host-owned.

The roadmap therefore distinguishes feature phases from generic runtime hardening. Higher-level features may continue, but they must consume the generic contracts rather than create project-specific exceptions.

External consumers use HAgent through the public provider-neutral APIs. HAgent does not contain consumer-specific dependencies or domain logic.
