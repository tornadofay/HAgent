Cognitive Workbench Controls

Authorized users may insert, edit and invalidate beliefs; create, edit, reprioritize, suspend, resume and abandon goals; modify intentions where policy permits; request plan reconsideration; inject observations/events; request deliberation; and pause, resume, wake, sleep or retire a runtime.

The UI must use HAgent runtime state-transition APIs and never write directly to persistence. Every mutation is atomic, version-aware, authorized and auditable. Record operator identity, timestamp, reason, UI action, previous revision and new revision.

If the runtime revision changed since the UI read it, reject or refresh the mutation rather than silently merging it. Existing execution snapshots remain immutable and stale executions must not overwrite newer cognition.
