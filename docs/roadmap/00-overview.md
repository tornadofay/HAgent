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
- 0.95 — Generic External Host Integration **planned next**
- 0.10 — Workspaces, Routing + Chat **active**
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

Phase 0.95 closes the remaining gap between HAgent's runtime architecture and a fully generic LLM integration surface: arbitrary host input/context, host correlation, structured output contracts, execution terminality, tool identity propagation, and strong runtime isolation. The phase does not introduce any host-specific domain dependency. Scheduling, authorization, host state, persistence, and side effects remain host-owned.

The sequence is intentional: secure host/data capabilities and the generic execution boundary come before richer autonomous collaboration. External consumers use HAgent through the public provider-neutral APIs; HAgent does not contain consumer-specific dependencies or domain logic.
