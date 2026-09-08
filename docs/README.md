# HAgent project documentation sources

The root `README.md`, `roadmap.md`, and `plan.md` are public entry points. Large evolving documents are maintained from smaller source files under `docs/roadmap/` and `docs/plan/`.

Do not hand-edit generated `roadmap.md` or `plan.md`. Update the relevant source file, then the GitHub workflow regenerates the root document.

Use small topical files. Keep implementation state factual and mark a feature complete only after matching `HAgent.Example` verification exists.

## Persistent project memory

The repository also contains a small set of purpose-specific persistent-memory documents used to preserve project context across constrained or interrupted development sessions:

- `docs/plan/00-current-state.md` — current milestone and verified project state.
- `docs/plan/00-active-work.md` — current unfinished work and handoff state; update rather than append history.
- `docs/plan/00-decisions.md` — durable architectural decisions and important supersessions only.
- `docs/architecture/00-design-principles.md` — stable cross-cutting design principles.

These files are compressed project state, not conversation logs. Do not create duplicate sources of truth or turn them into development diaries.
