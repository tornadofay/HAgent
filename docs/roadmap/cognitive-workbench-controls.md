# 0.97 Subdocument — Cognitive Workbench Controls

This document is part of **Phase 0.97 — Persistent Cognitive Runtime** and supports its management/diagnostic workbench.

Authorized users may inspect and, where policy permits, insert or invalidate beliefs; create, edit, reprioritize, suspend, resume and abandon goals; modify intentions; request plan reconsideration; inject observations/events; request deliberation; and pause, resume, wake, sleep or retire a runtime.

The UI must use HAgent runtime state-transition APIs and the canonical 0.959 intervention boundary where an action is an intervention. It must never write directly to persistence. Every mutation is atomic, version-aware, authorized and auditable. Record operator identity, timestamp, reason, UI action, previous revision and new revision where the owning contract exposes those fields.

If the runtime revision changed since the UI read it, reject or refresh the mutation rather than silently merging it. Existing execution snapshots remain immutable and stale executions must not overwrite newer cognition.

The workbench is not a new lifecycle system, plan store, approval engine, policy evaluator, or cognitive-state owner.
