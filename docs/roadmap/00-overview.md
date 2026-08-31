# HAgent Roadmap

The roadmap is the ordered implementation path toward the HAgent master plan. It describes phases and dependencies; stable architectural definitions belong under `docs/architecture/`, and current work belongs under `docs/plan/`.

## Current position

- 0.1–0.2 — foundations complete
- 0.3–0.4 — memory/context and provider-capability foundations complete
- 0.5 — tool foundation complete; hardening remains
- 0.6 — safety/permission foundation complete; broader authorization remains
- 0.7 — WinForms UI Context + Data Discovery complete and locally verified
- 0.8 — Data Access + Authorization **active**
- 0.9 — Runtime Agent Instances
- 0.10 — Workspaces, Routing + Chat
- 1.0 — Collaboration + Workflows
- Later — provider ecosystem, extensibility, developer platform, release hardening

The sequence is intentional: secure host/data capabilities come before rich autonomous collaboration. HWorld can begin integration at the 0.9 runtime-instance boundary; it does not need the business-application chat layers.
