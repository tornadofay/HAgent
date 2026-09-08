# HAgent Design Principles

This document contains stable, cross-cutting design principles that should survive individual roadmap phases. It is intentionally small and is not a substitute for detailed architecture documents.

## Core principles

1. **Provider-neutral Core** — `HAgent.Core` must not depend on provider, storage, WinForms, or host-domain implementation details.
2. **Host authority** — the host owns domain state, authorization, scheduling policy, persistence of host state, and side effects.
3. **Complete architecture first** — do not knowingly implement a reduced temporary architecture when the intended design is already understood.
4. **One canonical mechanism** — prefer one coherent architecture over compatibility wrappers, parallel paths, duplicate models, or transitional mechanisms during redesign/rebuild mode.
5. **Models are not authorities** — LLM output may request, suggest, or reason; explicit policy, authorization, state validation, and host contracts determine what may actually happen.
6. **Snapshot and revision safety** — running work must use immutable effective snapshots, and mutable cognitive/runtime state must be revision-aware so stale results cannot silently overwrite newer state.
7. **Explicit ownership and scope** — identity, resource ownership, memory ownership, and authorization must be explicit rather than inferred from convenience identifiers.
8. **Replaceable cognition** — the Cognitive Kernel is stable infrastructure; Cognitive Strategies are replaceable implementations of cognitive behavior.
9. **Deterministic before probabilistic when sufficient** — cognition should use deterministic state, policy, memory, and learned procedures when they are sufficient and request probabilistic reasoning when needed.
10. **Test for truth, not design discovery** — tests verify contracts and reveal defects or genuinely unforeseen interactions. Known architectural requirements must be addressed before implementation.
11. **Portable configuration, isolated execution state** — configuration is serializable and portable by explicit contract; live handlers, credentials outside their defined representation, executions, synchronization primitives, and transient process state are not.
12. **Bounded resources** — context, memory retrieval, event queues, tool loops, inspection, persistence, and runtime work must have explicit bounds and failure behavior.

## Persistent project-memory principle

The repository is the durable project memory for development across constrained or interrupted AI sessions. Memory documents must preserve only information that is useful for resuming work, preventing overlap, preserving architecture, or avoiding loss of important decisions.

They are compressed state, not transcripts.
