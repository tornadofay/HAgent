# Active Work

This file is the compact handoff state for work currently in progress. It is not a task history or development diary.

## Current task

- **Phase:** 0.955 Context Engineering
- **Status:** In progress
- **Primary source:** `docs/plan/20-active.md`
- **Scope:** Continue the ordered 0.955 context-engineering work from the verified Slice 1 checkpoint after 0.954.

## Current checkpoint

Phase 0.954 Prompt and Instruction Governance was verified and closed by the user on 2026-09-08.

- `COGNITION INSTRUCTIONS` passed on .NET Framework 4.8.1 and .NET 9.
- .NET 9 deterministic runtime coverage passed: `RUNTIME TERMINAL STATE`, `RUNTIME CONCURRENCY`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and corrected `RUNTIME SHUTDOWN`.
- .NET Framework 4.8.1 deterministic runtime coverage passed: `RUNTIME INSTANCES`, `RUNTIME OVERRIDES`, `RUNTIME STALE RESULTS`, and `RUNTIME SHUTDOWN`.
- The deterministic runtime Examples corrected during Slice 5 no longer require manually selected configured-agent UI state.
- Configuration-driven `RUNTIME EXECUTION` remains a live host example and was not used as a deterministic 0.954 milestone gate.

## Current run

**Verified repository checkpoint — 0.955 Slice 1 complete; 0.955 Slice 2 is current for the next run.**

0.955 Slice 1 reconciled `docs/architecture/20-context.md` with the current implementation and 0.955 roadmap requirements. The canonical target now separates provider-neutral context items, budgets, source/retrieval, policy filtering, ranking, compaction, caching, diagnostics, and immutable execution context snapshots. Existing `AgentExecutionRequest.HostContext`, `ConversationContextBuilder`, and WinForms `IUiContext` / `WinFormsUiContext` are recorded as input/producer boundaries rather than competing context architectures.

No local .NET build/test success is claimed for this Slice 1 planning/architecture checkpoint.

## Next slice

**0.955 Slice 2 — Provider-neutral context contract foundation**

Bounded objective: add the smallest Core-only contract layer for context items, explicit budget dimensions, bounded provenance/scope/trust/importance/freshness/size metadata, and the minimal candidate-source boundary. Do not implement ranking, compaction, caching, WinForms changes, or provider transport in that run.

Expected verification: focused contract-level validation/build on targeted frameworks, followed by the documentation update required by `docs/plan/20-active.md`. The next run must resume from this checkpoint and must not duplicate Slice 1 or begin Slice 3.

## Current blockers

This connected session can inspect and modify repository sources but cannot execute the local .NET/WinForms build or Example. No local build/test success is claimed from this session.
