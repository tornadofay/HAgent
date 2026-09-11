# 0.97 Subdocument — Cognitive Runtime Workbench

This document is part of **Phase 0.97 — Persistent Cognitive Runtime**, primarily supporting Slice 8 management, diagnostics, observability, and production verification.

HAgent.WinForms must add a top-level Cognitions view for active runtime instances. It provides complete inspection of current runtime cognition: beliefs, goals, intentions, plans, attention, working state, memory, knowledge, skills, events, executions, learning, lifecycle, and history.

The workbench is a diagnostic/management surface over the authoritative runtime-agent state owner. It must not create a second cognitive-state store, second policy evaluator, second execution planner, or independent persistence model.

All edits flow through public HAgent runtime state-transition/intervention APIs. The UI never writes directly to persistence and must preserve owner/revision/stale-result rules.

## Future 0.97 verification obligation — automated concurrency test

The 2026-09-11 single-owner/runtime-concurrency spike is architectural evidence only. Before **0.97 Slice 1** can be considered production-complete, its concurrency invariant must be covered by repeatable automated tests in `HAgent.Tests`; the manual Example spike must not be the sole verification of the concurrency claim.

The automated verification must exercise, at minimum:

- many independent runtime agents operating concurrently (at least 10);
- overlap of independent owner loops without collapsing behind one shared cognitive queue;
- serialized authoritative mutation within each individual agent;
- isolated runtime identity/revision/state between agents;
- stale-result rejection;
- cancellation protection;
- retirement/shutdown protection;
- preservation of the single-owner rule under asynchronous completion races.

The focused tests must run on both supported targets required by the 0.97 slice, including .NET Framework 4.8.1 and .NET 9. The test suite must verify the production implementation, not merely reproduce the earlier Example-only spike.

A matching `HAgent.Example` scenario remains required for the completed production capability under the repository's Example-first rules. The future automated test and the Example have different purposes: automated tests provide repeatable regression/contract verification; the Example demonstrates the externally usable capability through public APIs.

This obligation does **not** advance the current implementation milestone. The active implementation remains **0.9575 Slice 8**, and the existing 0.97 spike remains classified as verified architectural evidence rather than production completion.
